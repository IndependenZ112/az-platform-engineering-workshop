# Chore 9 — Staged infra deploy workflow

### Background

Every change to the Bicep under `infra/` should flow through CI, not be deployed from a laptop. The platform team wants a **staged pipeline**: lint, then auto-deploy to **test**, then wait for a human to approve **prod**. Same shape a real landing zone uses — test as the safety net, prod gated by a reviewer.

### Hints

Workflow shape:

| Stage | Job             | Runs on                              | Purpose |
| ----- | --------------- | ------------------------------------ | ------- |
| 1     | `lint`          | every trigger                        | `az bicep build` + `az bicep lint` against `infra/**`. No Azure login. |
| 2     | `deploy-shared` | `needs: lint`, env `test`            | OIDC login, `what-if` + `az deployment group create` for the **shared/app-supporting template** (the one that owns the single workshop ACR and any other resources both envs share) against the shared RG (e.g. `rg-workload-shared`). Emits the shared resource ids — at minimum the ACR resource id and login server — as **job outputs** for the per-env deploys to consume. Auto-approved. |
| 3     | `deploy-test`   | `needs: deploy-shared`, env `test`   | OIDC login, `what-if` + `az deployment group create` for the **per-env workload template** (spoke + peering, Container Apps environment, container apps, Azure SQL, managed identities, private endpoints + Private DNS) against `rg-workload-01-test`. Passes the shared outputs (`acrResourceId`, `acrLoginServer`) as parameters — does **not** provision an ACR. Auto-approved. |
| 4     | `deploy-prod`   | `needs: deploy-test`, env `prod`     | Same shape as `deploy-test` but `rg-workload-01-prod` with `main.prod.bicepparam`, consuming the same shared outputs. **Blocks on required reviewer.** |

A run that stops after `deploy-shared` (or after only the spoke part of the workload template) leaves the app unable to pull images or reach SQL — every infra run must produce a fully deployable environment, end to end. The split is about ownership and lifecycle (the shared registry is deployed once and shared across envs), not about deploying less.

OIDC details (everything below was provisioned in a previous chore — this chore just consumes it):

- `azure/login@v2` with `client-id: ${{ vars.AZURE_CLIENT_ID }}` / `tenant-id: ${{ vars.AZURE_TENANT_ID }}` / `subscription-id: ${{ vars.AZURE_SUBSCRIPTION_ID }}`, `permissions: id-token: write`.
- **No long-lived secrets** in repo or org secrets.
- One **user-assigned managed identity per environment** (the workload's GitHub deploy identity), each with a federated credential whose subject is `repo:<owner>/<repo>:environment:<env>`.

GitHub Environments do the gating, not workflow logic (also already configured):

- `test`: no protection rules. Variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP=rg-workload-01-test`. The `deploy-shared` job runs under this identity too and targets the shared RG (e.g. `rg-workload-shared`); the shared RG name is a workflow-level constant rather than a per-env variable so it does not silently fork between envs.
- `prod`: **required reviewers** (at least one human), optional wait timer. Same four variables pointing at the prod deploy identity and `rg-workload-01-prod`.

For the `deploy-shared` identity to deploy into the shared RG, it needs `Owner` (or `Contributor` + `User Access Administrator`) on that RG — same scoping rule as the per-env workload RGs (see [github-oidc-federation](../.github/instructions/github-oidc-federation.instructions.md)). It only ever holds `AcrPush` on the one shared registry; per-env identities pick up the registry resource id from the `deploy-shared` outputs and grant their runtime MIs `AcrPull` on it from the workload Bicep.

Two `bicepparam` files (`main.test.bicepparam`, `main.prod.bicepparam`) are the **only** thing that differs between the per-env stages. The workload template is identical and both stages reference the same shared resource ids emitted by `deploy-shared`.

Every deploy job runs `what-if` first and writes the output to `$GITHUB_STEP_SUMMARY` so the prod reviewer sees what they're approving.

### Outcome

First end-to-end run: test deploys without prompting; prod sits in **Waiting** on the Actions tab until you approve. After approval, the same commit's prod deploy uses the exact templates and params verified in test — no drift.

### Workshop scope note

You only have one subscription, so test and prod are different **resource groups** in the same subscription. The federated credentials and the workflow are still split per environment so the muscle memory matches a real multi-subscription landing zone — when you later have separate test and prod subscriptions, only `AZURE_SUBSCRIPTION_ID` changes.
