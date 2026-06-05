# Chore 3 — Implement the workload infrastructure in Bicep

### Background

The design from the previous chore is signed off. Turn it into **deployable Bicep** that lands the workload in the existing spoke — without re-opening architectural decisions.

### Hints

- Bicep under `infra/workload-01/`. Use AVM wherever a module exists; raw `Microsoft.*` only with an inline comment explaining the gap.
- CAF naming — see [.github/instructions/azure-naming.instructions.md](../.github/instructions/azure-naming.instructions.md). Every name embeds the `test` environment segment (`rg-workload-01-test`, `vnet-spoke-workload01-test-<region>-001`, `kv-hotelapi-test-<region>-001`, `id-hotelapi-test-<region>-001`, `ca-hotelapi-test-<region>-001`, `cae-hotelapi-test-<region>-001`, `sql-hotelapi-test-<region>-001`, `crhotelapitest<region>001`, etc.).
- If you discover a design gap while writing Bicep, **fix it in the design doc and diagram first**, then change the template.
- Identity wiring:
  - Managed identity per container app, with `AcrPull` (role definition ID `7f951dda-4ed3-4680-a7ca-43fe172d538d`) granted on the **registry scope** — grant it from the Bicep that creates the identity, not a post-deploy script.
  - Wire that same managed identity into the container app's **`registries[]`** entry (on the AVM `app/container-app` module: `server` = the ACR login server, `identity` = the UAMI's resource ID) so the app pulls with the managed identity — not with admin creds, and not with a system-assigned identity nobody granted `AcrPull` to. Skip this and the placeholder `mcr.microsoft.com/k8se/quickstart` revision still works (MCR is anonymous), but the rollout chore fails with `UNAUTHORIZED` the moment the app tries to pull `cr<...>.azurecr.io/...`.
  - **Backend managed identity = SQL server's Entra admin**, set declaratively on the AVM `sql/server` module — no `az sql server ad-admin create`, no post-deploy script. Because the managed identity is the server admin, it has full DDL/DML rights on every database; the app's startup code creates schema/seeds data on first run with no separate `CREATE USER ... FROM EXTERNAL PROVIDER` step.
  - **No secrets** — construct connection strings from resource properties at deploy time; auth is MI-based.

  On the AVM `br/public:avm/res/sql/server` module, set the `administrator` block to an `ActiveDirectory` admin whose `login` is the backend UAMI's name and `sid` is its `principalId`, with `tenantId` = the subscription tenant and `azureADOnlyAuthentication: true`. Set `publicNetworkAccess: 'Disabled'`, attach a private endpoint in `snet-private-endpoints` linked to `privatelink.database.windows.net`, and define the `hoteldb` database with serverless / scale-to-zero settings. Three common one-shot failures all live in that `administrator` block:
  - `principalType` must be **`'Application'`** for a user-assigned managed identity (not `'User'`).
  - `sid` must be the managed identity's **`principalId`** (not its `clientId`, not its `resourceId`).
  - Omitting `azureADOnlyAuthentication: true` leaves SQL auth enabled even when no SQL login is provisioned.

- Backend container app env vars (no secret, no Key Vault round-trip): set `ConnectionStrings__HotelDb` to a connection string built from `sqlServer.outputs.fqdn` that authenticates with `Active Directory Default` and `Encrypt=True` (no password anywhere), and set `AZURE_CLIENT_ID` to the backend UAMI's `clientId` so `DefaultAzureCredential` selects the right identity when several are attached.

  The application's `DbInitializer.InitializeAsync` (see [workload-app/backend/HotelBooking.Api/Data/DbInitializer.cs](../workload-app/backend/HotelBooking.Api/Data/DbInitializer.cs)) runs on first startup, authenticates with the MI, and creates tables as the server admin. **No platform-team work after deploy.**

- Decision-record note for the design doc: collapsing DBA and app-identity into one managed identity is a workshop simplification. In a production landing zone you'd typically use an Entra **group** as the admin (ops + break-glass), then create a contained DB user for the app's managed identity with only `db_datareader` / `db_datawriter` / `EXECUTE`. Call out the trade-off; don't change the workshop path.

- Distributed Private DNS: every zone created in the workload RG, linked to the spoke (registration) and the hub (resolution).
- Parameterise location, naming tokens, address space, SKU sizes — with sensible defaults.
- `Deploy-Workload.ps1` mirrors [mock-alz/Deploy-Hub.ps1](../mock-alz/Deploy-Hub.ps1). Run `azure-deployment-preflight` (what-if + permission checks) before every `az deployment group create`.
- Post-deploy steps that can't live in Bicep (e.g. contained DB users for managed identities) are scripted alongside the template — not done by hand.

### Outcome

By the end of this chore you have a **complete, reviewable Bicep template** under `infra/workload-01/` plus the `Deploy-Workload.ps1` wrapper — authored, not yet deployed (the deploy is a follow-up chore). Concretely:

- The template compiles and lints clean, and `azure-deployment-preflight` (what-if + permission check) against `rg-workload-01-test` runs with no surprises.
- Reading the template confirms the guardrails: ACR public with the admin user disabled; a user-assigned managed identity per app holding `AcrPull` and wired into `registries[]`; the backend MI as the SQL Entra admin with Entra-only auth and `publicNetworkAccess` disabled; a secret-free, MI-based connection string; and private endpoints with distributed Private DNS for every private PaaS service.
- The only public surface the template defines is the frontend FQDN and the container registry; everything else is private.
- The what-if previews a scale-to-zero resting footprint that matches the design's cost estimate.

The template is ready to deploy in the next chore.

### Hint — workflow

Use the **`bicep-plan`** chat mode to draft file layout and module choices, then switch to **`bicep-implement`** to write the resources. Keep the design open in the chat context.
