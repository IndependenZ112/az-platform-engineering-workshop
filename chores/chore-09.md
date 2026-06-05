# Chore 9 — Automate infra deployment with a staged GitHub Actions workflow

- A workflow at `.github/workflows/infra-deploy.yml` triggers on `push` to `main` (with a `paths:` filter on `infra/**`) and on `workflow_dispatch`.
- Auth uses **OIDC federation** — no long-lived secrets — reusing the per-environment deploy identities and federated credentials wired up in a previous chore.
- Three jobs: **`lint`** → **`deploy-test`** → **`deploy-prod`**, chained with `needs:`.
- **GitHub Environments** do the gating (already created in a previous chore): `test` (no protection), `prod` (required reviewers, branch policy `main`).
- Every deploy job runs `what-if` first and posts the output to the job summary.
- First end-to-end run: test deploys without prompting, prod waits in **Waiting** until you approve.

Stuck or want to check your work? See [details-09.md](details-09.md).
