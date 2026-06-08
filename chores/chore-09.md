# Chore 9 — Automate infra deployment with a staged GitHub Actions workflow

Every change to the infrastructure Bicep should flow through CI, not a laptop. Build a staged
pipeline: lint, auto-deploy to test, then a human-gated deploy to prod.

## Requirements

- A workflow at `.github/workflows/infra-deploy.yml` triggers on `push` to `main` (filtered to
  `infra/**`) and on manual dispatch.
- Authentication uses **OIDC federation** — no long-lived secrets — reusing the per-environment
  deploy identities and federated credentials from the bootstrap chore.
- The workflow deploys **every Bicep template the workload needs**, not just the spoke. That
  means the shared/app-supporting infrastructure that lives outside the per-env template
  (e.g. the single workshop container registry) **and** the per-environment workload template
  (spoke vnet + peering, Container Apps environment, container apps, Azure SQL, managed
  identities, private endpoints, Private DNS). A run that only stamps out the spoke is
  incomplete — the app cannot pull images or reach SQL without the rest.
- Jobs chain with `needs:` in this order: **`lint`** → **`deploy-shared`** →
  **`deploy-test`** → **`deploy-prod`**. The `deploy-shared` job runs once per workflow run
  against the shared resource group and emits the shared resource ids (e.g. the ACR resource
  id and login server) as **job outputs** that the per-env deploys consume as parameters.
- **GitHub Environments** do the gating: `test` unprotected, `prod` requiring a reviewer and
  restricted to `main`. The `deploy-shared` job runs under the `test` environment's identity
  (it owns the shared RG); no separate "shared" environment.
- Every deploy job runs **what-if first** and posts the output to the job summary so the prod
  reviewer sees what they are approving — including the what-if for the shared stage.
- The only difference between the test and prod per-env stages is the **parameter file**; the
  workload template is identical and both stages reference the same shared resource ids from
  `deploy-shared`.

## Success criteria

**Done when**
- `.github/workflows/infra-deploy.yml` exists and is valid — the staged pipeline is fully
  authored, even though it is not committed or run in this chore.

**Verify** (by reading the workflow — nothing is committed or executed yet)
- Jobs are chained `lint` → `deploy-shared` → `deploy-test` → `deploy-prod` with `needs:`,
  and the triggers are `push` to `main` filtered to `infra/**` plus manual dispatch.
- `deploy-shared` deploys the shared/app-supporting template (the one that owns the single
  workshop ACR) against the shared resource group and emits the shared resource ids as job
  outputs.
- `deploy-test` and `deploy-prod` deploy the per-env workload template (spoke + ACA + SQL +
  identities + private endpoints + DNS) and **consume the shared outputs** from
  `deploy-shared` as parameters — they do not provision a registry of their own.
- Each deploy job authenticates with **OIDC** (`permissions: id-token: write`, `azure/login`
  reading the environment's `vars.AZURE_*`) — no secrets anywhere in the file.
- `deploy-shared` and `deploy-test` target the unprotected `test` environment;
  `deploy-prod` targets the reviewer-gated `prod` environment.
- Each deploy job runs **what-if first** and writes the output to the job summary.
- Test and prod per-env stages differ only by **parameter file**; the workload template path
  is identical.

**Enough to move on**
- The workflow reads as a correct staged pipeline that will gate prod on a human once it runs —
  ready to commit in a later step.

---
Background, the job/stage table, and the OIDC consumption details: [details-09.md](details-09.md).
