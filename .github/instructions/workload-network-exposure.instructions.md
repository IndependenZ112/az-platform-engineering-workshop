---
applyTo: '**/*.bicep,**/*.bicepparam,**/infra/**,**/infrastructure/**,**/design*.md,**/*architecture*.md,**/chores/**/*.md,**/README.md'
description: 'Workload network exposure rules. Use when designing, reviewing, or implementing any Azure resource for the workshop spoke — defines which services stay public and which must be private-only.'
---

# Workload network exposure rules

These rules are **not negotiable** and apply to every design document, diagram, Bicep file, and chore artifact in this repo. They preserve the workshop topology: Monitor ingestion endpoints remain public, and the SPA is the only user-facing workload surface. Locking these down breaks the workshop. Application images are pulled anonymously from public GHCR packages; GHCR is external to the Azure workload and is not a workload endpoint.

## Services that MUST stay public

The following resources are **public on purpose**. Do not put them behind a private endpoint, do not flip `publicNetworkAccess` to `Disabled`, do not create private DNS zones for their `privatelink.*` namespaces, and do not add them to a Private Link Scope (AMPLS) for the workshop spoke.

| Resource | Reason it stays public |
| --- | --- |
| Log Analytics workspace | Monitor ingestion + query for the workshop runs over the public endpoint. |
| Application Insights | Telemetry ingestion from the workload runs over the public endpoint. |
| Data Collection Endpoint / Data Collection Rule (if used) | Same ingestion path as above. |
| The workload **frontend** (SPA hosting / Front Door / Static Web App / public App Service) | This is the only user-facing surface. |

Configure no private endpoints or private networking for Monitor resources. In Bicep, none of the following may appear for a Monitor resource:

- `privateEndpoints: [...]` blocks
- `publicNetworkAccess: 'Disabled'`
- Private DNS zones or zone groups referencing:
  - `privatelink.monitor.azure.com`
  - `privatelink.oms.opinsights.azure.com`
  - `privatelink.ods.opinsights.azure.com`
  - `privatelink.agentsvc.azure-automation.net`
  - `privatelink.applicationinsights.azure.com`
  - `privatelink.blob.core.windows.net` **when** it is the AI/LA ingestion storage (the workload's own storage accounts still go private)
- `azureMonitorPrivateLinkScopes` / AMPLS resources scoped to the workshop spoke

## Services that MUST stay private

Everything else in the workload spoke is **private-only** and reachable through private endpoints + Private DNS zones linked back to the hub vnet. Non-exhaustive list:

- Azure SQL (private endpoint only; Entra admin = workload managed identity; deploying principal has no data-plane access)
- Key Vault
- Storage accounts owned by the workload (blob/file/queue/table)
- App Configuration
- Service Bus / Event Hubs / Event Grid topics
- Cosmos DB
- Container Apps internal ingress / App Service with private endpoint (backend tier)
- Any other PaaS data/control plane used by the workload

For each of these the design must include:

1. A private endpoint in the workload spoke subnet.
2. `publicNetworkAccess: 'Disabled'`.
3. A Private DNS zone **co-located in the workload resource group** (distributed Private DNS model) and linked to the hub vnet.

## When designing or implementing

- Do not "harden" Monitor by adding private endpoints "just to be consistent." That is a regression, not an improvement.
- If a chore appears to require putting Monitor behind a private endpoint, stop and re-read the chore — it does not.
- Do not provision ACR for workload images. Architecture diagrams must show GHCR as an external public image source and the Monitor stack outside the private-endpoint boundary (e.g. annotated "public endpoint — workshop requirement"); the frontend remains the public user-facing surface.

## Self-check before you finish

Before saving any Bicep file, design markdown, or diagram, scan your output for these regexes. If any match **and** the surrounding resource is Log Analytics, Application Insights, a Data Collection Endpoint/Rule, or an AMPLS, you have violated this instruction — revert and re-design.

- `privatelink\.(monitor|oms\.opinsights|ods\.opinsights|agentsvc|applicationinsights)`
- `azureMonitorPrivateLinkScopes`
- `publicNetworkAccess:\s*'Disabled'` near a `Microsoft.ContainerRegistry/registries`, `Microsoft.OperationalInsights/workspaces`, `Microsoft.Insights/components`, or `Microsoft.Insights/dataCollectionEndpoints` resource
- A `privateEndpoints` array on a Monitor AVM module (`br/public:avm/res/operational-insights/workspace`, `br/public:avm/res/insights/component`, `br/public:avm/res/insights/data-collection-endpoint`)
