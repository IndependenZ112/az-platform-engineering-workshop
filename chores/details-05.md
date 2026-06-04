# Chore 5 — Deploy the workload infrastructure

### Background

The Bicep from the previous chore is reviewed. Time to actually land it in the subscription.

### Hints

- Confirm `az account show` points at the **same subscription** as the hub and the spoke.
- Run **`azure-deployment-preflight`** end-to-end: validate, what-if, permission check. Do not proceed until all three are clean.
- Execute `./infra/workload-01/Deploy-Workload.ps1`.
- Re-run immediately — second run's what-if must be a **no-op**.
- Verifications:
  - Public surface = frontend FQDN + ACR login server only.
  - `nslookup <sqlserver>.database.windows.net` from inside the spoke returns a **private IP** in `snet-private-endpoints`; from elsewhere it resolves but **refuses connections** (`publicNetworkAccess: Disabled`).
  - Container apps exist with `minReplicas = 0`, registry has `adminUserEnabled = false`, both UAMIs hold `AcrPull` on the registry scope.
  - SQL database in **paused** state shortly after deploy (auto-pause = 60 min).
- **Don't deploy container images yet** — container apps will spin up with the placeholder `mcr.microsoft.com/k8se/quickstart` image. That's expected; the next chore fixes it.

### Outcome

Workload infrastructure exists in the subscription, fully private (except registry and frontend), idempotent, with a resting cost that matches the design.

### Safety note

This is the first irreversible step. If anything in the what-if surprises you (resource deletions, role-assignment changes on resources you didn't expect to touch, edits to the hub RG), stop and reconcile with the design.
