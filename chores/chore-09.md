# Chore 9 — Automate infra deployment with a staged GitHub Actions workflow

Every change to the infrastructure Bicep should flow through CI, not a laptop. Build a staged
pipeline: lint, auto-deploy to test, then a human-gated deploy to prod.

## Requirements

- A workflow at `.github/workflows/infra-deploy.yml` triggers on `push` to `main` (filtered to
  `infra/**`) and on manual dispatch.
- Authentication uses **OIDC federation** — no long-lived secrets — reusing the per-environment
  deploy identities and federated credentials from the bootstrap chore.
- Three jobs chained with `needs:`: **`lint`** → **`deploy-test`** → **`deploy-prod`**.
- **GitHub Environments** do the gating: `test` unprotected, `prod` requiring a reviewer and
  restricted to `main`.
- Every deploy job runs **what-if first** and posts the output to the job summary so the prod
  reviewer sees what they are approving.
- The only difference between the test and prod stages is the **parameter file**; the template
  is identical.

## Success criteria

**Done when**
- `.github/workflows/infra-deploy.yml` exists and is valid — the staged pipeline is fully
  authored, even though it is not committed or run in this chore.

**Verify** (by reading the workflow — nothing is committed or executed yet)
- Three jobs are chained `lint` → `deploy-test` → `deploy-prod` with `needs:`, and the triggers
  are `push` to `main` filtered to `infra/**` plus manual dispatch.
- Each deploy job authenticates with **OIDC** (`permissions: id-token: write`, `azure/login`
  reading the environment's `vars.AZURE_*`) — no secrets anywhere in the file.
- `deploy-test` targets the unprotected `test` environment; `deploy-prod` targets the
  reviewer-gated `prod` environment.
- Each deploy job runs **what-if first** and writes the output to the job summary.
- Test and prod stages differ only by **parameter file**; the template path is identical.

**Enough to move on**
- The workflow reads as a correct staged pipeline that will gate prod on a human once it runs —
  ready to commit in a later step.

---
Background, the job/stage table, and the OIDC consumption details: [details-09.md](details-09.md).
