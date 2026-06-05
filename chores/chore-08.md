# Chore 8 — Bootstrap GitHub Actions OIDC federation per environment

- A PowerShell script (e.g. `scripts/Setup-GitHubOidc.ps1`) provisions the **GitHub Actions deploy identity** for each environment end-to-end. **No portal clicks.**
- Per environment, the script creates (idempotently) a **user-assigned managed identity** dedicated to GitHub Actions (separate from any runtime managed identity on the container apps), with **`Owner` on its own workload RG**, **`Network Contributor` on the hub VNet**, and **`AcrPush` on the workload container registry**.
- The script adds a **federated credential** on each deploy identity with subject `repo:<owner>/<repo>:environment:<env>` (issuer `https://token.actions.githubusercontent.com`, audience `api://AzureADTokenExchange`).
- The script creates the **GitHub Environments** `test` and `prod` (using `gh api`), sets `prod`'s **required reviewers** and **deployment-branch policy = `main`**, and writes the four environment **variables** (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP`) per environment via `gh variable set --env`.
- Repo owner/name are inferred from `git remote get-url origin`; the script accepts overrides for forks.
- Re-running the script is a **no-op** (federated credentials with the same name are updated, not duplicated; role assignments and GitHub variables are checked before writing).
- After it runs, `az identity federated-credential list` shows one subject per environment per identity, and `gh variable list --env test` / `--env prod` show the four variables.

Stuck or want to check your work? See [details-08.md](details-08.md).
