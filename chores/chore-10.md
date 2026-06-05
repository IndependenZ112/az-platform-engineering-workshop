# Chore 10 — Automate app container deployment with a build-once, promote-everywhere workflow

Automate the application rollout so an image is built **once** and the **same bytes** are
promoted through test and prod — prod never sees a freshly rebuilt artifact.

## Requirements

- A workflow at `.github/workflows/app-deploy.yml` triggers on `push` to `main` (filtered to
  `workload-app/**`) and on manual dispatch.
- Four jobs chained with `needs:`: **`build`** → **`deploy-test`** → **`smoke-test`** →
  **`deploy-prod`**.
- The **build job runs once per run** and emits image **digests** as job outputs.
- Both deploy jobs pin to the **digest** (not the tag); prod deploys the exact same digest that
  passed test. **No build runs in the prod stage.**
- Prod is gated by the `prod` GitHub Environment's required reviewer.
- Each deploy job writes the digest it deployed to the job summary.

## Success criteria

**Done when**
- `.github/workflows/app-deploy.yml` exists and is valid — the build-once, promote-everywhere
  pipeline is fully authored, even though it is not committed or run in this chore.

**Verify** (by reading the workflow — nothing is committed or executed yet)
- Four jobs are chained `build` → `deploy-test` → `smoke-test` → `deploy-prod` with `needs:`,
  and the triggers are `push` to `main` filtered to `workload-app/**` plus manual dispatch.
- The **build job runs once** and emits image **digests** as job outputs.
- Both deploy jobs pin to the **digest** (`@sha256:`), not a tag, and the prod stage contains
  **no** build/`az acr build`/re-tag step — it reuses the digest that passed test.
- Prod targets the reviewer-gated `prod` environment; each deploy job writes the digest it
  deploys to the job summary.

**Enough to move on**
- The workflow reads as a correct build-once pipeline that promotes identical digests test →
  prod and gates prod on a human once it runs — ready to commit in a later step.

---
Background, the four-job shape, and why build-once matters: [details-10.md](details-10.md).
