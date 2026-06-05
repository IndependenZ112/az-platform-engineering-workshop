# Chore 3 — Implement the workload infrastructure in Bicep

The signed-off design becomes deployable infrastructure. Turn it into Bicep that lands the
workload in the existing spoke without re-opening architectural decisions.

## Requirements

- All Bicep lives under `infra/workload-01/` and **uses Azure Verified Modules** wherever a
  module exists. Raw `Microsoft.*` resources only where no AVM module exists, with a comment
  explaining the gap.
- Resource names follow **Microsoft CAF** and embed the `test` environment token from day one.
- Identity is wired end-to-end with **user-assigned managed identities** and **no secrets** in
  parameters, outputs, or configuration. Authentication between Azure services is
  identity-based throughout.
- Each container app pulls its image from ACR **using its own managed identity** (registry
  admin user stays disabled).
- The **backend's managed identity is the SQL server's Microsoft Entra admin, set
  declaratively in Bicep**, with Microsoft Entra-only authentication enabled. No post-deploy
  admin commands, no deployment scripts, no jumpbox — the app creates its own schema on first
  startup.
- The backend reaches SQL **passwordlessly** over the private endpoint, using a connection
  built from resource properties at deploy time.
- The **distributed Private DNS** pattern is implemented: zones live in the workload RG, linked
  to the spoke (registration) and the hub (resolution).
- Location, naming tokens, address space, and SKU sizes are **parameterised** with sensible
  defaults.
- A `Deploy-Workload.ps1` script wraps the deployment and runs **preflight (what-if +
  permission check)** before every deployment.
- The deployment is **idempotent**.

## Success criteria

**Done when**
- `infra/workload-01/` holds a complete Bicep template plus the `Deploy-Workload.ps1` wrapper.
- The template compiles/lints clean and passes preflight (what-if + permission check) against
  the target resource group — without surprises in the what-if.

**Verify** (by inspecting the Bicep / lint / what-if — nothing is deployed in this chore)
- The container registry is set to **`publicNetworkAccess` enabled** with the **admin user
  disabled** (no `networkRuleSet`/`ipRules` lockdown).
- Each app has a **user-assigned managed identity** with an **`AcrPull`** role assignment on
  the registry scope, and that identity is referenced in the app's `registries[]` entry.
- The SQL server's **Entra admin is the backend MI**, set declaratively, with Entra-only auth
  enabled and `publicNetworkAccess` disabled; the backend's connection string is built from
  resource properties with no secret.
- Private endpoints + distributed Private DNS zones exist for every private PaaS service, in
  the workload RG and linked to spoke and hub.
- **No secret** appears in any parameter, variable, or output.

**Enough to move on**
- The template is complete, lints clean, and its what-if is understood and ready to deploy in
  the next chore.

---
Background, identity-wiring hints, and the common one-shot failures: [details-03.md](details-03.md).
