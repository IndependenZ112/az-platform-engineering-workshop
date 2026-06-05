# Chore 3 — Implement the workload infrastructure in Bicep

### Background

The design from the previous chore is signed off. Turn it into **deployable Bicep** that lands the workload in the existing spoke — without re-opening architectural decisions.

### Hints

- Bicep under `infra/workload-01/`. Use AVM wherever a module exists; raw `Microsoft.*` only with an inline comment explaining the gap.
- CAF naming — see [.github/instructions/azure-naming.instructions.md](../.github/instructions/azure-naming.instructions.md). Every name embeds the `test` environment segment (`rg-workload-01-test`, `vnet-spoke-workload01-test-<region>-001`, `kv-hotelapi-test-<region>-001`, `id-hotelapi-test-<region>-001`, `ca-hotelapi-test-<region>-001`, `cae-hotelapi-test-<region>-001`, `sql-hotelapi-test-<region>-001`, `crhotelapitest<region>001`, etc.).
- If you discover a design gap while writing Bicep, **fix it in the design doc and diagram first**, then change the template.
- Identity wiring:
  - Managed identity per container app, with `AcrPull` (role definition ID `7f951dda-4ed3-4680-a7ca-43fe172d538d`) granted on the **registry scope** — grant it from the Bicep that creates the identity, not a post-deploy script.
  - Wire the managed identity into the container app's **`registries[]`** entry so the app actually pulls with that identity (not with admin creds, not with a system-assigned identity that nobody granted `AcrPull` to). Pattern with the AVM `app/container-app` module:

    ```bicep
    registries: [
      {
        server: acr.outputs.loginServer
        identity: backendUami.outputs.resourceId  // the same managed identity that holds AcrPull
      }
    ]
    ```

    Without this, the placeholder `mcr.microsoft.com/k8se/quickstart` revision still works (MCR is anonymous), but the rollout chore will fail with `UNAUTHORIZED` the moment the app tries to pull `cr<...>.azurecr.io/...`.
  - **Backend managed identity = SQL server's Entra admin**, set declaratively on the AVM `sql/server` module — no `az sql server ad-admin create`, no post-deploy script. Because the managed identity is the server admin, it has full DDL/DML rights on every database; the app's startup code creates schema/seeds data on first run with no separate `CREATE USER ... FROM EXTERNAL PROVIDER` step.
  - **No secrets** — construct connection strings from resource properties at deploy time; auth is MI-based.

  Pattern (AVM `br/public:avm/res/sql/server`):

  ```bicep
  module sqlServer 'br/public:avm/res/sql/server:<pinned-version>' = {
    name: 'sql-server'
    params: {
      name: sqlServerName
      location: location
      managedIdentities: {
        userAssignedResourceIds: [
          backendUami.outputs.resourceId
        ]
      }
      // Declarative Entra admin = backend managed identity. principalType MUST be 'Application'
      // for a user-assigned managed identity; sid is the managed identity's principalId (NOT clientId, NOT resourceId).
      administrator: {
        administratorType: 'ActiveDirectory'
        login: backendUami.outputs.name
        sid: backendUami.outputs.principalId
        tenantId: subscription().tenantId
        principalType: 'Application'
        azureADOnlyAuthentication: true
      }
      publicNetworkAccess: 'Disabled'
      privateEndpoints: [ /* PE in snet-private-endpoints, link to privatelink.database.windows.net */ ]
      databases: [
        {
          name: 'hoteldb'
          // serverless / scale-to-zero settings here
        }
      ]
    }
  }
  ```

  Three common one-shot failures, all in that `administrator` block:
  - `principalType: 'User'` (wrong — must be `'Application'` for a user-assigned managed identity).
  - `sid: backendUami.outputs.clientId` (wrong — must be `principalId`).
  - Forgetting `azureADOnlyAuthentication: true` — leaves SQL auth enabled even when no SQL login is provisioned.

- Backend container app env var (no secret, no Key Vault round-trip needed):

  ```bicep
  env: [
    {
      name: 'ConnectionStrings__HotelDb'
      value: 'Server=tcp:${sqlServer.outputs.fqdn},1433;Database=hoteldb;Authentication=Active Directory Default;Encrypt=True;'
    }
    {
      name: 'AZURE_CLIENT_ID'
      value: backendUami.outputs.clientId   // tells DefaultAzureCredential which managed identity to use when several are attached
    }
  ]
  ```

  The application's `DbInitializer.InitializeAsync` (see [workload-app/backend/HotelBooking.Api/Data/DbInitializer.cs](../workload-app/backend/HotelBooking.Api/Data/DbInitializer.cs)) runs on first startup, authenticates with the MI, and creates tables as the server admin. **No platform-team work after deploy.**

- Decision-record note for the design doc: collapsing DBA and app-identity into one managed identity is a workshop simplification. In a production landing zone you'd typically use an Entra **group** as the admin (ops + break-glass), then create a contained DB user for the app's managed identity with only `db_datareader` / `db_datawriter` / `EXECUTE`. Call out the trade-off; don't change the workshop path.

- Distributed Private DNS: every zone created in the workload RG, linked to the spoke (registration) and the hub (resolution).
- Parameterise location, naming tokens, address space, SKU sizes — with sensible defaults.
- `Deploy-Workload.ps1` mirrors [mock-alz/Deploy-Hub.ps1](../mock-alz/Deploy-Hub.ps1). Run `azure-deployment-preflight` (what-if + permission checks) before every `az deployment group create`.
- Post-deploy steps that can't live in Bicep (e.g. contained DB users for managed identities) are scripted alongside the template — not done by hand.

### Outcome

- `infra/workload-01/` deploys cleanly end-to-end.
- Second run of the script produces a no-op what-if (idempotent).
- Public surface = frontend FQDN + container registry. Everything else private.
- Resting cost matches the scale-to-zero estimate from the design.

### Hint — workflow

Use the **`bicep-plan`** chat mode to draft file layout and module choices, then switch to **`bicep-implement`** to write the resources. Keep the design open in the chat context.
