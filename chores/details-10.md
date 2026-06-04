# Chore 10 — Staged infra deploy workflow

### Background

Every change to the Bicep under `infra/` should flow through CI, not be deployed from a laptop. The platform team wants a **staged pipeline**: lint, then auto-deploy to **test**, then wait for a human to approve **prod**. Same shape a real landing zone uses — test as the safety net, prod gated by a reviewer.

### Hints

Workflow shape:

| Stage | Job          | Runs on             | Purpose |
| ----- | ------------ | ------------------- | ------- |
| 1     | `lint`       | every trigger       | `az bicep build` + `az bicep lint` against `infra/**`. No Azure login. |
| 2     | `deploy-test`| `needs: lint`, env `test` | OIDC login, `az deployment group what-if`, then `az deployment group create` against `rg-workload-01-test`. Auto-approved. |
| 3     | `deploy-prod`| `needs: deploy-test`, env `prod` | Same shape but `rg-workload-01-prod`. **Blocks on required reviewer.** |

OIDC details:

- `azure/login@v2` with `client-id` / `tenant-id` / `subscription-id`, `permissions: id-token: write`.
- **No long-lived secrets** in repo or org secrets.
- Two separate Entra app registrations — one per environment — each federated to the matching GitHub environment.
- Federated credential subject for test: `repo:<owner>/<repo>:environment:test`. Same shape for prod.

GitHub Environments do the gating, not workflow logic:

- `test`: no protection rules. Variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP=rg-workload-01-test`.
- `prod`: **required reviewers** (at least one human), optional wait timer, deployment-branch policy restricted to `main`. Same four variables pointing at the prod app reg and `rg-workload-01-prod`.

Two `bicepparam` files (`main.test.bicepparam`, `main.prod.bicepparam`) are the **only** thing that differs between stages. Template is identical.

Every deploy job runs `what-if` first and writes the output to `$GITHUB_STEP_SUMMARY` so the prod reviewer sees what they're approving.

### Outcome

First end-to-end run: test deploys without prompting; prod sits in **Waiting** on the Actions tab until you approve. After approval, the same commit's prod deploy uses the exact templates and params verified in test — no drift.

### Workshop scope note

You only have one subscription, so test and prod are different **resource groups** in the same subscription. The federated credentials and the workflow are still split per environment so the muscle memory matches a real multi-subscription landing zone — when you later have separate test and prod subscriptions, only `AZURE_SUBSCRIPTION_ID` changes.
