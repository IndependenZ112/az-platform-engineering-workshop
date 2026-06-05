---
applyTo: '**/.github/workflows/*.yml,**/.github/workflows/*.yaml'
description: 'Build-once, promote-everywhere release rule for the workshop app pipeline — build images once, pin deploys to the immutable digest, never rebuild for prod. Use when authoring or reviewing the application deploy workflow.'
---

# Build once, promote everywhere

The contract for the application pipeline: **build each image exactly once, then deploy that
same immutable artifact to every environment.** Prod must never run bytes that no environment
has tested. Applies to the app deploy workflow. Complements
[github-actions-ci-cd-best-practices](github-actions-ci-cd-best-practices.instructions.md) and
[github-oidc-federation](github-oidc-federation.instructions.md).

## Rules

- **Build runs once per workflow run.** A single build job builds both images and emits their
  **digests** (`@sha256:...`) as job outputs.
- **Capture the digest from the build output itself** — not by re-querying the registry or
  inspecting a tag afterwards, which can race with another build. The deployable reference is
  `<registry>/<repo>@sha256:...`.
- **Every deploy job pins to the digest**, not a moving tag. Test and prod deploy the *same*
  digest, so "it passed in test" is evidence about the exact bytes prod will run.
- **No build step in the prod stage.** No `az acr build`, no re-tag, no rebuild. If the prod
  job builds anything, the contract is broken.
- **Gate prod** behind the `prod` GitHub Environment's required reviewer; chain jobs with
  `needs:` (build → deploy-test → smoke-test → deploy-prod).
- Allow a **`workflow_dispatch` with an explicit image reference** to skip the build and
  redeploy a known-good digest (rollback / promote-an-older-image).
- Each deploy job writes the digest it deployed to `$GITHUB_STEP_SUMMARY` so the reviewer can
  confirm test and prod match.

## The proof

`az containerapp show` for test and prod returns **identical** `image` fields ending in the
same `@sha256:` digest. If they differ, something rebuilt between environments — fix the
workflow, don't paper over it.
