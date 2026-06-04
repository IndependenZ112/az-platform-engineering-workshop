# Chore 10 — Automate infra deployment with a staged GitHub Actions workflow

- A workflow at `.github/workflows/infra-deploy.yml` triggers on `push` to `main` (with a `paths:` filter on `infra/**`) and on `workflow_dispatch`.
- Auth uses **OIDC federation** — no long-lived secrets — with **separate app registrations per environment**.
- Three jobs: **`lint`** → **`deploy-test`** → **`deploy-prod`**, chained with `needs:`.
- **GitHub Environments** do the gating: `test` (no protection), `prod` (required reviewers, branch policy `main`).
- Every deploy job runs `what-if` first and posts the output to the job summary.
- First end-to-end run: test deploys without prompting, prod waits in **Waiting** until you approve.

Stuck or want to check your work? See [details-10.md](details-10.md).
