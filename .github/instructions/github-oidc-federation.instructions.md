---
applyTo: '**/.github/workflows/*.yml,**/.github/workflows/*.yaml,**/*.ps1'
description: 'GitHub Actions OIDC federation to Azure for the workshop — per-environment deploy identities, federated-credential subject format, role scopes, and GitHub Environment wiring. Use when bootstrapping CI identity or authoring a workflow that logs in to Azure.'
---

# GitHub Actions OIDC federation (Azure)

How the workshop authenticates GitHub Actions to Azure **without any long-lived secret**.
Applies to the OIDC bootstrap script and to any workflow that runs `azure/login`. Complements
[github-actions-ci-cd-best-practices](github-actions-ci-cd-best-practices.instructions.md) —
this file carries the workshop-specific, load-bearing details that are easy to get subtly
wrong.

## One deploy identity per environment

- Provision a **user-assigned managed identity per environment** (`test`, `prod`) dedicated to
  GitHub Actions, **separate** from any runtime identity on the container apps (see
  [workload-identity](workload-identity.instructions.md)).
- Scope each deploy identity to exactly:
  - **`Contributor`** on its **own workload resource group** — enough to deploy the workload
    resources without granting unnecessary role-assignment permissions.
  - **`Network Contributor`** on the **hub resource group** — needed both to write the
    spoke→hub peering on the hub VNet *and* because the AVM `virtualNetwork` module creates
    the **remote** peering via a nested deployment in the hub RG, which requires
    `Microsoft.Resources/deployments/write` at RG scope. Scoping to just the VNet resource
    fails with `AuthorizationFailed` on `<deploymentName>-virtualNetworkPeering-remote-0`.

The GHCR image-build workflow uses the repository-scoped `GITHUB_TOKEN` with
`packages: write`; it does not require an Azure deploy identity or ACR push permissions.

## Federated credential — get the subject exactly right

A single character wrong here fails as `AADSTS70021: No matching federated identity record
found`, and only inside the workflow run — not at bootstrap. Use:

- **Issuer:** `https://token.actions.githubusercontent.com`
- **Audience:** `api://AzureADTokenExchange`
- **Subject:** `repo:<owner>/<repo>:environment:<env>` — generate it from the repo remote and
  the environment name; never hand-type it. The `environment:` form must match the GitHub
  Environment the job declares.

## GitHub Environments and variables

- Create the **`test`** and **`prod`** GitHub Environments. `prod` carries **required
  reviewers**; `test` has no protection. No deployment-branch policy is used in this
  workshop.
- Publish these as environment **variables, not secrets** (they are not sensitive and may
  appear in logs): `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`,
  `AZURE_RESOURCE_GROUP` (the env's workload RG).
- In the workflow, `azure/login` consumes `${{ vars.AZURE_CLIENT_ID }}` /
  `${{ vars.AZURE_TENANT_ID }}` / `${{ vars.AZURE_SUBSCRIPTION_ID }}`, and the job sets
  `permissions: id-token: write` (plus `contents: read`). No repo or org secrets.

## Idempotency

Any bootstrap script must be **safe to re-run**: check-then-write every role assignment,
federated credential (update rather than duplicate on name clash), GitHub Environment, and
variable. A second run is a clean no-op.
