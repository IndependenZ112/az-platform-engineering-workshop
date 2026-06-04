# Chore 8 — Rebuild the deployment as two fully isolated environments

### Background

Two things came up after the previous chore: (1) the **single spoke from earlier is no longer fit for purpose** — test and prod must not share a VNet; (2) the **existing deployment carries baggage** from before the split (resources named without an environment token, the original `rg-workload-01`, the shared spoke). Tear it down and rebuild cleanly.

### Hints

Teardown order (with what-if previews before each destructive step):

1. Container apps and their environment.
2. SQL server, ACR, Key Vault, other workload PaaS.
3. **Original spoke VNet** *and* the hub-side peerings (orphaned peerings cause confusing what-if output later).
4. The `rg-workload-01` resource group itself.

`mock-alz/` and `rg-platform` are **not** touched.

Two new spokes:

- `vnet-spoke-workload01-test-<region>-001` in `rg-workload-01-test`.
- `vnet-spoke-workload01-prod-<region>-001` in `rg-workload-01-prod`.
- Non-overlapping address spaces. Suggested: test = `10.10.0.0/22`, prod = `10.20.0.0/22`. Write the convention into the design doc **before** the Bicep.
- Each spoke carries the same subnet layout as the original (private endpoints subnet, ACA infrastructure subnet sized for the zone-redundant requirements, room to grow).
- Each spoke peers to the hub in both directions. **Spokes do not peer to each other.**

Distributed Private DNS still applies per environment. `Deploy-Workload.ps1` is extended to deploy spoke, workload, or both; spoke first.

### Outcome

- Hub has exactly two workload peerings (`...-test`, `...-prod`), both `Connected`, no orphans.
- Spokes do not peer to each other.
- Both environments deploy green from a clean subscription state; test→prod and prod→test orderings both work.
- `nslookup` on the SQL private endpoint returns the test private IP from the test spoke, the prod private IP from the prod spoke.
- Design doc + diagram updated **before** any Bicep changes.

### Safety note

First chore that **destroys** deployed resources. Run every `az group delete` / `az resource delete` only after a what-if and a manual sanity check that the scope is `rg-workload-01` — **not** `rg-platform`. Deleting the hub resets the workshop for everyone.
