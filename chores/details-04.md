# Chore 4 — Deploy the workload infrastructure

### Background

The Bicep from the previous chore is reviewed. Time to actually land it in the subscription.

### Hints

- Confirm `az account show` points at the **same subscription** as the hub and the spoke.
- Run **`azure-deployment-preflight`** end-to-end: validate, what-if, permission check. Do not proceed until all three are clean.
- Execute `./infra/workload-01/Deploy-Workload.ps1`.
- Re-run immediately — second run's what-if must be a **no-op**.
- Verifications:
  - Public surface = frontend FQDN + ACR login server only.
  - **Private DNS is wired up correctly** — verify the A record exists in the workload's Private DNS zone and points at the private endpoint's IP:

    ```powershell
    az network private-dns record-set a list `
      --resource-group <workload-rg> `
      --zone-name privatelink.database.windows.net `
      --query "[].{name:name, ip:aRecords[0].ipv4Address}" -o table

    az network private-endpoint show `
      --name <pe-sql-name> --resource-group <workload-rg> `
      --query "customDnsConfigs[0].ipAddresses" -o tsv
    ```

    The two IPs must match, and the IP must sit inside the `snet-private-endpoints` range.
  - **Public DNS for the SQL FQDN resolves to a CNAME ending in `.privatelink.database.windows.net`** — from your laptop (outside the spoke):

    ```powershell
    Resolve-DnsName <sqlserver>.database.windows.net | Format-Table Name, Type, NameHost, IPAddress
    ```

    You'll see the CNAME chain. A direct connection still **refuses** because `publicNetworkAccess` is `Disabled`.
  - **Optional live resolution test from inside the spoke** (only useful once *any* container app revision is running — the placeholder image is fine):

    ```powershell
    az containerapp exec `
      --resource-group <workload-rg> --name <backend-ca-name> `
      --command "/bin/sh -c 'getent hosts <sqlserver>.database.windows.net'"
    ```

    If `getent`/`nslookup` aren't present in the placeholder image, defer this check to the rollout chore where the real backend image runs.
  - Container apps exist with `minReplicas = 0`, registry has `adminUserEnabled = false`, both managed identities hold `AcrPull` on the registry scope.
  - SQL database in **paused** state shortly after deploy (auto-pause = 60 min).
- **Don't deploy container images yet** — container apps will spin up with the placeholder `mcr.microsoft.com/k8se/quickstart` image. That's expected; the next chore fixes it.

### Outcome

Workload infrastructure exists in the subscription, fully private (except registry and frontend), idempotent, with a resting cost that matches the design.

### Safety note

This is the first irreversible step. If anything in the what-if surprises you (resource deletions, role-assignment changes on resources you didn't expect to touch, edits to the hub RG), stop and reconcile with the design.
