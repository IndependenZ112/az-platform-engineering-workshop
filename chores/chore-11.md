# Chore 11 — Automate app container deployment with a build-once, promote-everywhere workflow

- A workflow at `.github/workflows/app-deploy.yml` triggers on `push` to `main` (`paths:` filter on `workload-app/**` and `dockerfiles/**`) and on `workflow_dispatch`.
- Four jobs: **`build`** → **`deploy-test`** → **`smoke-test`** → **`deploy-prod`**.
- **The build job runs once per workflow run** and emits image **digests** as job outputs.
- Both deploy jobs pin to the **digest** (not the tag) — prod deploys the **exact same bytes** that passed test.
- **No `build` / `az acr build` runs in the prod stage.**
- Prod is gated by the `prod` GitHub Environment's required reviewer.
- Each deploy job writes the digest it deployed to the job summary.

Stuck or want to check your work? See [details-11.md](details-11.md).
