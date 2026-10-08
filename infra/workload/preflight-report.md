# Azure Deployment Preflight Report

**Generated:** 2026-10-08T10:35:00Z
**Status:** ✅ Pass

---

## Summary

| Property | Value |
|----------|-------|
| **Template File(s)** | `infra/workload/main.bicep` |
| **Parameter File(s)** | `infra/workload/main.bicepparam` |
| **Project Type** | standalone-bicep |
| **Deployment Scope** | resourceGroup (`rg-hotelbooking-test`) |
| **Target** | Azure subscription 1 (`<redacted-subscription-id>`) |
| **Validation Level** | Provider |

### Validation Results

| Check | Status | Details |
|-------|--------|---------|
| Bicep Syntax | ✅ Pass | `az bicep build --file infra/workload/main.bicep --stdout` — no errors, no warnings (one `no-hardcoded-env-urls` warning on the fixed `privatelink.database.windows.net` zone name was explicitly suppressed with a documented `#disable-next-line`, since Private Link DNS zone names are fixed well-known values, not environment-specific endpoints). |
| What-If Analysis | ✅ Pass | 16 resources to create, 0 to modify, 0 to delete, 1 ignored (an AVM module's own no-op telemetry ping deployment — expected and benign). No surprises. |
| Permission Check | ✅ Pass | `Provider` validation level succeeded on the first attempt — no fallback to `ProviderNoRbac` was needed, confirming the deploying principal's permissions are sufficient for every resource in the template, including the DNS zone's virtual network link into the hub VNet (`rg-platform`), which only requires read access to resolve the VNet reference (the link resource itself lives in the workload RG). |

---

## Tools Executed

### Commands Run

| Step | Command | Exit Code |
|------|---------|-----------|
| 1 | `az bicep build --file infra\workload\main.bicep --stdout` | 0 |
| 2 | `az bicep build-params --file infra\workload\main.bicepparam --stdout` | 0 |
| 3 | `infra\workload\Deploy-Workload.ps1` (preflight-only; no `-Deploy`) — runs Bicep build, then `az deployment group what-if --validation-level Provider` | 0 |

### Tool Versions

| Tool | Version |
|------|---------|
| Azure CLI | 2.85.0 |
| Bicep CLI | bundled with Azure CLI (`az bicep build` succeeded) |
| Azure Developer CLI | n/a |

---

## Issues

✅ **No issues found.** The template is ready for deployment in a follow-up chore.

---

## What-If Results

### Change Summary

| Change Type | Count |
|-------------|-------|
| 🆕 Create | 16 |
| 📝 Modify | 0 |
| 🗑️ Delete | 0 |
| ⚠️ Ignore | 1 |

### Resources to Create

| Resource Type | Resource Name |
|---|---|
| Microsoft.Network/virtualNetworks/subnets | `snet-containerapps` (new subnet on the existing spoke VNet) |
| Microsoft.ManagedIdentity/userAssignedIdentities | `id-hotelapi-test-swedencentral-001` |
| Microsoft.Network/privateDnsZones | `privatelink.database.windows.net` |
| Microsoft.Network/privateDnsZones/virtualNetworkLinks | `link-vnet-hotelbooking-test-swedencentral-001` (spoke) |
| Microsoft.Network/privateDnsZones/virtualNetworkLinks | `link-vnet-hub` (hub) |
| Microsoft.Sql/servers | `sql-hotelbooking-test-swedencentral-<uniqueString>` |
| Microsoft.Sql/servers/databases | `sqldb-hotelbooking-test` |
| Microsoft.Sql/servers/auditingSettings | `default` (AVM module default) |
| Microsoft.Sql/servers/connectionPolicies | `default` (AVM module default) |
| Microsoft.Network/privateEndpoints | `pep-sql-hotelbooking-test-001` |
| Microsoft.Network/privateEndpoints/privateDnsZoneGroups | `default` |
| Microsoft.OperationalInsights/workspaces | `log-hotelbooking-test-swedencentral-001` |
| Microsoft.Insights/components | `appi-hotelbooking-test-swedencentral-001` |
| Microsoft.App/managedEnvironments | `cae-hotelbooking-test-swedencentral-001` |
| Microsoft.App/containerApps | `ca-hotelapi-test-swedencentral-001` (backend, internal ingress) |
| Microsoft.App/containerApps | `ca-hotelweb-test-swedencentral-001` (frontend, external ingress) |

### Resources to Modify

*No resources will be modified.* (The existing resource group and spoke VNet are referenced, not changed; the new subnet is an addition to the VNet's subnet collection, not a modification of an existing subnet.)

### Resources to Delete

*No resources will be deleted.*

### Key property confirmations (from the what-if diff)

- `Microsoft.Sql/servers` → `properties.publicNetworkAccess: "Disabled"`, `properties.administrators.azureADOnlyAuthentication: true`, `properties.administrators.principalType: "Application"`, `properties.administrators.sid` built via `reference(...).principalId` on the backend UAMI (no secret).
- `Microsoft.App/containerApps/ca-hotelapi-...` → `identity.type: "UserAssigned"` (the backend UAMI only), `properties.configuration.ingress.external: false`, env vars built entirely via `reference()`/`format()` expressions (`ConnectionStrings__HotelDb`, `AZURE_CLIENT_ID`, `APPLICATIONINSIGHTS_CONNECTION_STRING`) — no literal secret values.
- `Microsoft.App/containerApps/ca-hotelweb-...` → `properties.configuration.ingress.external: true`, `BACKEND_URL` built via `reference()` to the backend app's ingress FQDN.
- No `Microsoft.ContainerRegistry/registries` resource anywhere in the plan; neither container app has a `registries` block.
- No private endpoint, `publicNetworkAccess: 'Disabled'`, or `privatelink.*` zone for Log Analytics or Application Insights — both stay fully public as required.

---

## Recommendations

1. The template is complete and its what-if is understood — ready to deploy in a follow-up chore.
2. No action required before sign-off.

---

## Next Steps

Preflight passed. Deployment itself is intentionally **not** performed in this chore
(`Deploy-Workload.ps1` defaults to preflight-only and requires an explicit `-Deploy` switch).

---

*Report generated by Azure Deployment Preflight Skill*
