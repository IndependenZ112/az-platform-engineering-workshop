# Chore 11 — Build-once, promote-everywhere app workflow

### Background

The previous chore handles infra; this handles the application. The rule is simple: **build each container image exactly once, then deploy that same image — same tag, same digest, byte-for-byte — to test and then to prod.** Prod must never see a freshly-rebuilt image, because a rebuild is a new artifact that hasn't been verified anywhere.

### Hints

Four-job workflow, all chained with `needs:`:

| Stage | Job           | Env  | Purpose |
| ----- | ------------- | ---- | ------- |
| 1     | `build`       | `test` (for ACR push perms) | OIDC login, `az acr login`, `az acr build` for both Dockerfiles. Tag each image `:${{ github.sha }}`. **Emit the `@sha256:...` digests as job outputs** (`backend-digest`, `frontend-digest`). |
| 2     | `deploy-test` | `test` | `az containerapp update --image <registry>/<repo>@${{ needs.build.outputs.backend-digest }}`. Pin to **digest**, not tag. Wait for the new revision to be healthy. Auto-approved. |
| 3     | `smoke-test`  | `test` | `curl --fail` the test frontend FQDN and a couple of `/api/` health checks. If smoke fails, prod is never offered the image. |
| 4     | `deploy-prod` | `prod` | **Blocks on required reviewer.** Same `az containerapp update --image ...@<digest>` against prod. **No build. No `az acr build`. No re-tag.** Same digest that passed test. |

Other essentials:

- **Reuse the per-env OIDC app registrations from the previous chore.**
- Build is gated by `if: github.event.inputs.image_tag == ''` (or equivalent) so a `workflow_dispatch` with an explicit `image_tag` **skips the build entirely** and goes straight to redeploy. Covers two scenarios with one mechanism: redeploying yesterday's good image after a bad one, and rolling prod to an older tag while you investigate.
- Each deploy job writes `backend → <registry>/<repo>@<digest>` and `frontend → <registry>/<repo>@<digest>` to `$GITHUB_STEP_SUMMARY` so the prod reviewer can eyeball it.
- A multi-subscription setup would add `az acr import` between test and prod registries to copy the **same digest**. You still wouldn't rebuild.

### Outcome

A diff of `az containerapp show` outputs across test and prod shows identical `image` fields ending in `@sha256:<same-digest>`. That's the contract.

### Why build once

Every rebuild creates a new artifact: base-image patches, transient package mirrors, build-time clocks and ARGs all change the bytes. If prod has its own `build` job, prod is running an artifact that **no one has ever tested**. Build once, deploy the same image everywhere, and "it worked in test" actually means something in prod.

### Why two pipelines, not one

Infra (previous chore) and app (this chore) move at different speeds and have different blast radius. Separate workflows mean a typo in Bicep can't accidentally redeploy the app, and a hotfix container build doesn't drag a half-finished infra change along. Both pipelines share the same OIDC app registrations and the same `test` / `prod` environments, so the security model stays consistent.
