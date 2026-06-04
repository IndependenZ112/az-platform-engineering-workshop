# Chore 7 — Add a production environment alongside test

### Background

The application team needs a real **production** environment alongside test. Prod must run **at least 3 replicas spread across availability zones**, no scale-to-zero. Both environments coexist in the same subscription.

### Hints

- Single `environmentName` parameter with allowed values `test` / `prod`, or two `*.bicepparam` files (`workload.test.bicepparam`, `workload.prod.bicepparam`). **No code fork** — same template produces both.
- Each environment in its own RG (`rg-workload-01-test`, `rg-workload-01-prod`) with **non-overlapping spoke address spaces**. Both spokes peer to the same hub.
- CAF naming carries the environment token (`ca-hotelapi-test-weu-001` vs `ca-hotelapi-prod-weu-001`).
- ACA env **`zoneRedundant: true`** is set at environment creation time and cannot be flipped later — so prod gets its **own** ACA environment.
- SQL: prod uses a SKU that supports **zone redundancy** (e.g. General Purpose serverless with `zoneRedundant: true`, or provisioned Business Critical) and does **not** auto-pause.
- `Deploy-Workload.ps1` takes `-Environment test|prod` and selects the matching RG / parameter file / address space. Preflight runs before every deploy.

### Outcome

- Deploying test then prod (or prod then test) leaves both workloads healthy on their own frontend FQDNs.
- Each environment's what-if shows only the resources for that environment.
- Design doc + diagram are updated to show both environments and the zone-spread prod topology.

### Reliability note

Zone redundancy on Container Apps and Azure SQL is **only available in regions with 3 availability zones**. Confirm your region qualifies (e.g. `westeurope`, `northeurope`, `eastus2`). If not, either move prod to one that does or call out the limitation in the updated design.
