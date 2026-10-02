# Chore 4 — Deploy the workload infrastructure

### Background

The Bicep from the previous chore is reviewed. Time to actually land it in the subscription.

### Hints

- Confirm `az account show` points at the **same subscription** as the hub and the spoke.
- Run **`azure-deployment-preflight`** end-to-end: validate, what-if, permission check. Do not proceed until all three are clean.
- Execute `./infra/workload-01/Deploy-Workload.ps1`.
- Re-run immediately — second run's what-if must be a **no-op**.
- Verifications (write the `az` / `Resolve-DnsName` queries yourself):
  - Public surface = frontend FQDN + ACR login server only.
  - **Private DNS is wired correctly** — the A record in the workload's `privatelink.database.windows.net` zone must resolve to the SQL private endpoint's private IP, and that IP must sit inside the `snet-private-endpoints` range. Compare the zone's A record against the private endpoint's `customDnsConfigs` IP; the two must match.
  - **Public DNS for the SQL FQDN resolves to a CNAME ending in `.privatelink.database.windows.net`** from outside the spoke (your laptop). You'll see the CNAME chain, but a direct connection still **refuses** because `publicNetworkAccess` is `Disabled`.
  - **Optional live resolution from inside the spoke** is only meaningful once a container app revision is running. If the workload image lacks `getent`/`nslookup`, use the platform's DNS diagnostics instead.
  - Container apps exist with `minReplicas = 0`, the registry has `adminUserEnabled = false`, and both managed identities hold `AcrPull` on the registry scope.
  - SQL database in **paused** state shortly after deploy (auto-pause = 60 min).
  - **The frontend's public URL loads the hotel SPA in a browser** — confirms the public ingress and prebuilt workload image work end to end.
- Deploy the public workshop images from GHCR. Set the frontend's `BACKEND_URL` to the backend's internal HTTPS ingress URL so `/api/` stays on the private application path.

### Outcome

The workload infrastructure now **exists in the subscription** and is fully private except for the two intentional public surfaces (frontend FQDN + container registry). Specifically:

- A second deploy is a clean **no-op** what-if — the template is idempotent.
- SQL is reachable only through its private endpoint: its private A record matches the endpoint IP inside `snet-private-endpoints`, and from outside the spoke the FQDN resolves through a `privatelink.*` CNAME while a direct connection refuses.
- Container apps are up on the public GHCR images with `minReplicas = 0`, ACR admin disabled, and both managed identities holding `AcrPull`.
- The frontend's public URL loads the hotel SPA and `/api/hotels` returns JSON, confirming public ingress and the private backend path.
- Resting cost matches the scale-to-zero design estimate.

This is the first irreversible step; the application images come in the next chore.

### Safety note

This is the first irreversible step. If anything in the what-if surprises you (resource deletions, role-assignment changes on resources you didn't expect to touch, edits to the hub RG), stop and reconcile with the design.
