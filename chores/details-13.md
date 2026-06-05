# Chore 13 — Prove the app pipeline by rebranding the frontend

### Background

The previous chore proved the **infra** pipeline. This proves the **app** pipeline — make the smallest possible, visually obvious edit to the frontend, push it, watch `app-deploy` build a new image, deploy to test, smoke test, wait for prod approval, roll out to prod. When you load the site in the browser, your change is staring back at you.

> **You do this one by hand.** This chore edits **application source** (`index.html`), which Copilot is not allowed to touch — the only files Copilot may write inside `workload-app/` are container build assets (Dockerfile, nginx config, etc.), and `index.html` is not one of them. Open the file in the editor yourself, change one line, save, commit. Treat it as a one-time stunt: rebrand the title, ship it, move on. **Do not ask Copilot to make the edit, and do not start adding features to the app.**

### Hints

**Manually** edit one line in [workload-app/frontend/index.html](../workload-app/frontend/index.html) — change the `<title>` from `StayBright Hotels` to something with your name/handle/team in it (e.g. `Azureholic Hotels`). Nothing else in the file. Nothing else in application source under `workload-app/`.

If you've never touched HTML before and don't know what to change, **ask Copilot for instructions** — something like "where is the page title in this file and what does the syntax look like?" Copilot can explain it and point at the line. **You** still make the edit by hand; Copilot must not write to application source under `workload-app/`.

```powershell
git add workload-app/frontend/index.html
git commit -m "app(frontend): rebrand title to Azureholic Hotels"
git push origin main
```

`infra-deploy` does **not** run (its filter is `infra/**`) — exactly the separation the app workflow was built for.

Watch the run (`gh run watch` or the web UI):

1. **`build`** — builds both images, pushes to ACR, emits digests. Note the frontend digest.
2. **`deploy-test`** — `az containerapp update --image <registry>/frontend@<digest>` against test. No rebuild. New revision points at the new digest.
3. **`smoke-test`** — hits the public test URL for HTTP 200.
4. **`deploy-prod`** — sits in **Waiting** for the reviewer. Approve. `az containerapp update` against prod with the **same digest**. No build, no `az acr build`, no re-tag.

Verify byte-for-byte promotion:

```powershell
az containerapp show -g rg-workload-01-test -n ca-hotelweb-test-weu-001 --query "properties.template.containers[0].image"
az containerapp show -g rg-workload-01-prod -n ca-hotelweb-prod-weu-001 --query "properties.template.containers[0].image"
```

Both must end in `@sha256:<same-digest>`. If not, something rebuilt between test and prod — re-read the build-once workflow design.

If the workflow does **not** trigger or fails:

1. Commit only touched files outside `workload-app/**` (`git show --stat HEAD`).
2. `build` failed because the frontend `Dockerfile` `FROM` line points at Docker Hub instead of `mcr.microsoft.com` — fix per the repo's container-image rule.
3. `deploy-test` failed at OIDC — same federated-credential subject check as the infra pipeline, but for the `test` environment.
4. Smoke test fails because the new revision isn't taking traffic yet — Container Apps takes a few seconds to swap revisions; add a short retry loop if the workflow doesn't already.

### Outcome

Test and prod URLs both show the new title in the browser tab. `az containerapp show` returns the same `@sha256:` digest for both. You've seen, with your own eyes, the same image artifact you built once flow through test → smoke → prod.

### Why a title change

The `<title>` tag is the cheapest possible "real" app edit: one byte of meaningful change, visible to a human without logging into anything, and it forces the full build-once / promote-everywhere pipeline to run end-to-end. That's the whole reason that workflow exists.
