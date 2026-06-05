# Chore 8 — Bootstrap GitHub Actions OIDC federation per environment

Wire your published repo to Azure so GitHub Actions can deploy **without any long-lived
secret**. Do it once, in a re-runnable script — no portal clicks.

## Requirements

- A PowerShell script provisions the **GitHub Actions deploy identity per environment**
  end-to-end — idempotently, with no manual portal steps.
- Per environment, a **user-assigned managed identity** dedicated to GitHub Actions (separate
  from any runtime identity) is created and scoped to: **`Owner`** on its own workload RG,
  **`Network Contributor`** on the hub VNet resource, and **`AcrPush`** on the workload
  registry.
- Each deploy identity gets a **federated credential** whose subject targets the matching
  GitHub Environment (`repo:<owner>/<repo>:environment:<env>`).
- The script creates the **GitHub Environments** `test` and `prod` and sets the four
  environment **variables** (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`,
  `AZURE_RESOURCE_GROUP`) per environment.
- **`prod` requires reviewers** so a prod deploy waits for a manual approval; **`test` has no
  protection** and deploys without a gate.
- Repo owner/name are **inferred** from the `origin` remote, with overrides accepted.
- Re-running the script is a **clean no-op**.

## Success criteria

**Done when**
- Both deploy identities exist with the correct scopes and can mint Azure tokens from their
  GitHub Environment.

**Verify**
- Each identity lists exactly **one** federated credential with the right subject.
- Each environment shows the **four** variables.
- `prod` carries required-reviewer protection.

**Enough to move on**
- No app registrations, no client secrets, no portal steps — and a second run changes nothing.

---
Background, the script outline, and why subject typos fail silently: [details-08.md](details-08.md).
