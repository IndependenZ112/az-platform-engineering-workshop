# Chore 8 — Rebuild the deployment as two fully isolated environments

- **Tear down the existing single-environment workload deployment** (container apps → SQL/ACR/etc → spoke VNet + peerings → resource group). `mock-alz` / `rg-platform` are not touched.
- Stand up **two spoke VNets**, one per environment, each in its own resource group, each peered independently to the hub. The two spokes do **not** peer to each other.
- Workload Bicep deploys end-to-end into the **matching spoke** for its environment.
- The **distributed Private DNS** pattern still applies per environment.
- The deploy script can deploy spoke, workload, or both; spoke is deployed first.
- After rebuild, the hub shows exactly two workload peerings (`...-test`, `...-prod`), both `Connected`, no orphans.
- Design doc and diagram are updated **before** any Bicep changes.

Stuck or want to check your work? See [details-08.md](details-08.md).
