# Hotel Booking workload — infrastructure design (`test` and `prod`)

| | |
|---|---|
| **Status** | Reviewed and signed off (see [Design review](#design-review)); extended for `prod` — see [§0](#0-multi-environment-model) |
| **Environments** | `test` and `prod` (both Belgium Central; hub in Sweden Central) — same subscription, isolated spokes |
| **Diagram** | [diagrams/environments-topology.drawio](diagrams/environments-topology.drawio) (`test` + `prod` topology) · [diagrams/workload-architecture.drawio](diagrams/workload-architecture.drawio) (per-spoke resource detail — applies to both environments; only parameter values differ, per §0) |

This document is a design, not Bicep. It is detailed enough that a follow-up implementation
can build the workload without re-opening an architectural question.

---

## 0. Multi-environment model

**One Bicep template, two parameter files.** `infra/workload/main.bicep` (and
`infra/spoke-network/main.bicep`) are shared, unmodified-per-environment templates. Every
difference between `test` and `prod` is a **parameter value**, supplied by
`main.test.bicepparam` / `main.prod.bicepparam` — never an `if (environment == …)` branch in
the template. A `what-if` re-run of `test` after adding `prod` support shows **zero changes**:
nothing about `test`'s deployed resources is renamed, dropped, or recreated by extending the
template to also accept `prod` values.

### 0.1 Region choice: both spokes in Belgium Central, hub stays in Sweden Central

`test` was placed in **Belgium Central** in a follow-up implementation; **`prod` is placed in
the same region**, keeping both workload spokes co-located while the **hub stays in Sweden
Central**, unchanged — exactly the topology already established for `test` (the spoke's
region is independent of the hub's; they're connected by [global (cross-region) VNet
peering](https://learn.microsoft.com/azure/virtual-network/virtual-network-peering-overview),
which this workshop already uses for `test` → hub).

`prod` needs **zone-redundant** Container Apps and Azure SQL. Belgium Central does have 3
physical availability zones (confirmed directly against the subscription), so this is not a
regional capability gap — it is a **newer region**, and the only material risk is a newer
region occasionally lagging on a specific service's zone-redundancy *rollout* even when the
region itself has zones. That risk is accepted here (consistent with keeping `test` and `prod`
topologically identical, aside from parameter values) and is verified empirically at
deployment time — the `prod` deploy's preflight and actual `az deployment group create` are
the real test of whether zone-redundant Container Apps/SQL are available in Belgium Central
for this subscription today. If a future deploy ever surfaces a
`LocationNotAvailableForResourceType`-style error for zone redundancy specifically, the
resolution is the same pattern already used for Application Insights: move just the affected
resource's `location`-equivalent parameter, not the whole environment.

`prod`'s Application Insights still needs the same regional fallback as `test`'s — Belgium
Central doesn't yet support `Microsoft.Insights/components` (see [§6](#6-dns)) — so both
environments' Application Insights resources live in Sweden Central, for the same reason.

### 0.2 Address-space split

| Network | CIDR | Region | Resource group |
|---|---|---|---|
| Hub (`vnet-hub`) | `192.168.100.0/24` | Sweden Central | `rg-platform` (existing, unchanged) |
| `test` spoke | `192.168.101.0/24` | Belgium Central | `rg-hotelbooking-test-belgiumcentral` |
| `prod` spoke | `192.168.102.0/24` | Belgium Central | `rg-hotelbooking-prod-belgiumcentral` |

Each spoke peers to the hub, bidirectionally, independently, across regions (global peering).
**Spokes do not peer to each other** — `test` and `prod` have no network path between them.
Each spoke's own `snet-private-endpoints` (`/26`) and `snet-containerapps` (`/27`) subnets are
carved from its own `/24` exactly as `test`'s were (see [§5](#5-networking)); `prod`'s subnets
are `192.168.102.0/26` and `192.168.102.64/27`.

**The hub-side peering name is an explicit parameter, not derived from the spoke name.** The
hub VNet accumulates one peering per spoke; a literal, shared peering name (the original,
pre-`prod` template used a hardcoded `peer-hub-to-spoke`) makes a second spoke's deployment
try to change an already-existing peering's remote VNet in place, which Azure rejects
(`ChangingRemoteVirtualNetworkNotAllowed`). `test`'s peering names are pinned to their exact,
already-deployed values (`peer-spoke-to-hub` / `peer-hub-to-spoke`) so adding `prod` doesn't
touch them; `prod` gets its own unique pair (`peer-spoke-prod-to-hub` / `peer-hub-to-spoke-prod`).

### 0.3 What actually differs between `test` and `prod`

| Parameter | `test` | `prod` | Why |
|---|---|---|---|
| `location` | `belgiumcentral` | `belgiumcentral` | Same region for both spokes; only the hub is elsewhere |
| `monitorLocation` | `swedencentral` (fallback) | `swedencentral` (fallback) | Neither environment's own region supports Application Insights yet — same fallback, same reason, for both |
| Spoke CIDR | `192.168.101.0/24` | `192.168.102.0/24` | Non-overlapping, isolated per environment |
| `zoneRedundant` | `false` | `true` | Applied identically to the Container Apps environment and the SQL database — one flag, two resources |
| Container Apps `minReplicas` / `maxReplicas` | `0` / `3` | `3` / `10` | `prod` never scales to zero; `test` does |
| SQL SKU capacity | `GP_S_Gen5`, 1 vCore | `GP_S_Gen5`, 2 vCores | Still serverless (same AVM code path — see the note below) — just a larger ceiling |
| SQL `autoPauseDelay` | `60` minutes | `-1` (disabled) | `prod` never pauses |
| Log Analytics retention | `30` days | `90` days | Longer retention is a reasonable, low-risk `prod` default |

**`prod` stays on the serverless SQL SKU family** (`GP_S_Gen5`), just with a higher capacity
ceiling and auto-pause disabled — serverless General Purpose supports zone redundancy, so
there is no need to fork onto a different (provisioned) SKU family for `prod`. This keeps
`test` and `prod` on the exact same AVM module code path, differing only in parameter values,
per the chore's non-negotiable "one template" principle.

**One property genuinely can't be flipped on an existing resource:** a Container Apps
environment's `zoneRedundant` flag is set at creation and cannot be changed in place. `test`'s
existing (non-zone-redundant) environment is unaffected — the parameter still drives the
value, there's just no in-place migration path for `test` to become zone-redundant later
without recreating its environment. That's expected and fine: `test` isn't meant to be
zone-redundant.

### 0.4 CI/CD identity, per environment

Per [§7](#7-identity), the CI/CD identity is dedicated **per environment**, never shared and
never the runtime identity:

| Environment | CI/CD identity | Scope |
|---|---|---|
| `test` | `id-cicd-hotelbooking-test-belgiumcentral-001` | `Contributor` on `rg-hotelbooking-test-belgiumcentral`; `Network Contributor` on `rg-platform` |
| `prod` | `id-cicd-hotelbooking-prod-belgiumcentral-001` | `Contributor` on `rg-hotelbooking-prod-belgiumcentral`; `Network Contributor` on `rg-platform` |

---

## 1. Application analysis

The app in [workload-app/](../workload-app/) is a small hotel booking demo: a React SPA
frontend and an ASP.NET Core minimal API backend, backed by a single relational database.

### 1.1 Backend — `workload-app/backend/HotelBooking.Api`

| Aspect | Finding |
|---|---|
| Runtime / framework | .NET **10**, ASP.NET Core **minimal API** ([Program.cs](../workload-app/backend/HotelBooking.Api/Program.cs)) |
| Listen port (container) | `8080` (`ASPNETCORE_HTTP_PORTS=8080`, set in the [Dockerfile](../workload-app/backend/HotelBooking.Api/Dockerfile)) |
| Endpoints | `GET/POST /api/hotels*`, `GET/POST/DELETE /api/bookings*` — a REST API, no server-rendered pages. `MapOpenApi()` exposes `/openapi/*.json`. |
| Data store | Azure SQL (EF Core `Microsoft.EntityFrameworkCore.SqlServer`), one `HotelDbContext` with `Hotels`, `Rooms`, `Bookings` |
| Schema management | **No migrations tool.** `DbInitializer.InitializeAsync` calls `EnsureCreatedAsync` then seeds a hotel catalogue if empty, on every startup. Idempotent — safe to run on every deploy/restart. |
| Auth to its dependencies | Connection string comment and sample in `appsettings.json` show the intended path: `Authentication=Active Directory Default` — i.e. the app expects to authenticate to SQL with whatever managed identity its process runs as. **No password anywhere in app config.** |
| End-user auth | **None.** The API has no login/authz — it's an open demo booking flow. This is an application-level decision already made by the app team; the platform compensates with network isolation (see [§7](#7-identity)), not app-layer auth. |
| Telemetry | `Azure.Monitor.OpenTelemetry.AspNetCore`, wired on **only if** `APPLICATIONINSIGHTS_CONNECTION_STRING` is set — a no-op otherwise. The connection string is not a secret (it identifies an ingestion endpoint, not a credential) and is safe as a plain app setting. |
| CORS | Wide open (`AllowAnyOrigin`) — harmless in production because the SPA calls the API same-origin through the frontend's reverse proxy (see §1.3); this is a local-dev convenience in the app, not a production dependency. |
| Static files / SPA fallback | `Program.cs` also calls `UseStaticFiles()` / `MapFallbackToFile("index.html")`, but the backend's own Dockerfile only publishes the API (`dotnet publish`, no `wwwroot` copy) — **this code path is dead in the container image** and only matters for a combined local dev run. The frontend is a genuinely separate deployable. |

### 1.2 Frontend — `workload-app/frontend`

| Aspect | Finding |
|---|---|
| Framework / build | React 19 + Vite 7 + Tailwind, built to static assets (`npm run build` → `dist/`) |
| Runtime container | nginx (Azure Linux base), serving `dist/` on port `8080`, SPA fallback via `try_files $uri /index.html` ([nginx/default.conf.template](../workload-app/frontend/nginx/default.conf.template)) |
| Backend wiring | The SPA calls relative `/api/*` ([src/api/client.ts](../workload-app/frontend/src/api/client.ts)) — **no CORS needed**, because nginx reverse-proxies `/api/` to `$BACKEND_URL` ([nginx/entrypoint.sh](../workload-app/frontend/nginx/entrypoint.sh)). `BACKEND_URL` is validated and substituted into the nginx config **at container start**, not at image build time — it is the one piece of environment-specific config the image needs, and it's supplied as a container-app environment variable. |
| Browser telemetry | OpenTelemetry Web SDK exports traces to `${VITE_OTEL_EXPORTER_OTLP_ENDPOINT}` or, if unset, the relative path `/otel` ([src/telemetry.ts](../workload-app/frontend/src/telemetry.ts)). `VITE_*` variables are inlined by Vite **at build time**, which conflicts with a single shared, public image reused across environments (see [§6](#6-dns)). |
| Images | Hotel photos are external `unsplash.com` URLs in seed data — **no Azure Storage / blob dependency**. |
| Auth / secrets | None. No API keys, no Key Vault references anywhere in the frontend. |

### 1.3 Frontend ↔ backend

```
Browser ──HTTPS (public)──▶ ca-hotelweb (nginx, external ingress)
                                 │  same-origin /api/* request
                                 ▼
                           proxy_pass $BACKEND_URL
                                 │  (internal, same Container Apps environment)
                                 ▼
                           ca-hotelapi (internal ingress only)
```

No CORS, no public exposure of the API, and no separate API gateway — the reverse proxy
*is* the API's only public-facing door, and it isn't public at all (the backend's own ingress
is internal-only; only the frontend is reachable from the internet).

---

## 2. Container hosting decision

**Chosen: Azure Container Apps (ACA)**, one environment, two apps (frontend, backend).

### 2.1 Alternatives considered

| Option | Scale-to-zero | Private networking | Managed identity | Ops overhead | Verdict |
|---|---|---|---|---|---|
| **Azure Container Apps** | Yes (Consumption profile) | Native VNet injection; per-app ingress (external vs internal) in the same environment | UAMI per app, first-class | Low — managed revisions, no node/cluster management | **Chosen** |
| Azure App Service (Web App for Containers) | Not on the plans that support VNet integration (Standard/Premium always keep ≥1 instance warm) | Yes, via regional VNet integration + private endpoint | UAMI supported | Medium — still no scale-to-zero for a VNet-integrated plan, so it fails a hard requirement | Rejected — can't scale to zero while private |
| Azure Kubernetes Service (AKS) | Partial (user node pools only; system pool stays warm) | Yes, fully | Yes, via workload identity | High — cluster/node-pool lifecycle, upgrades, patching for 2 containers | Rejected — disproportionate ops burden for this workload |
| Azure Container Instances (ACI) | Pay-per-second, but no environment grouping, no built-in ingress/revisions, no traffic splitting | Possible via VNet injection, but no managed internal/external ingress split | Supported, but no per-app identity model as clean as ACA | Medium — would need a hand-rolled front door / ingress layer | Rejected — missing the managed-ingress and revision features this app benefits from |
| Azure Functions (custom container) | Yes | Yes (Premium/Flex) | Yes | Low | Rejected — the app is a conventional long-running web API + SPA, not an event/trigger model; forcing it into Functions adds friction with no benefit |

### 2.2 Why Container Apps fits this workload specifically

- **Scale-to-zero** on the Consumption workload profile matches a `test` environment that sits
  idle most of the time (Cost).
- **One environment, two ingress postures**: the environment can hand out a public static IP
  for the frontend while the backend's own ingress is set `external: false` — both apps still
  get the environment's automatic service discovery (call the backend by name,
  `http://<app-name>`, traffic never leaves the environment) (Security + Ops — no extra
  component needed to keep the API off the internet).
- **User-assigned managed identity** is a first-class, per-app configuration — exactly what
  [workload-identity.instructions.md](../.github/instructions/workload-identity.instructions.md)
  requires (Security).
- **Subnet footprint**: a *workload profile* environment needs only a **`/27`** subnet (vs.
  `/23` for the legacy Consumption-only environment type), which comfortably fits inside the
  spoke's `/24` alongside the private-endpoint subnet (Cost/Perf — no need to re-cut the
  address plan that's already been deployed).

---

## 3. Compute design

### 3.1 Container Apps Environment

| Setting | `test` | `prod` | Why |
|---|---|---|---|
| Name | `cae-hotelbooking-test-belgiumcentral-001` | `cae-hotelbooking-prod-belgiumcentral-001` | CAF |
| Type | **Workload profiles** environment, built-in **Consumption** profile only (no Dedicated profile) | Same | Smallest subnet footprint (`/27`) + lets `test` scale to zero; a Dedicated profile isn't needed at either environment's scale |
| VNet integration | Injected into its own spoke's `snet-containerapps`, delegated to `Microsoft.App/environments` | Same pattern, isolated spoke | Required for any VNet-integrated environment |
| External/internal | **External** (public static IP) | Same | Needed so the frontend app can be reached from the internet; the backend app overrides this per-app (see below) — the environment setting is a ceiling, not a floor |
| Zone redundancy | `false` | `true` | Cost for `test`; `prod` requirement (§0) — one `zoneRedundant` parameter drives both |

### 3.2 Container apps

| | Frontend | Backend |
|---|---|---|
| Name | `ca-hotelweb-<env>-<region>` | `ca-hotelapi-<env>-<region>` |
| Image | `ghcr.io/azureholic/az-platform-engineering-workshop/frontend:<tag>` (public, anonymous pull) | `ghcr.io/azureholic/az-platform-engineering-workshop/backend:<tag>` (public, anonymous pull) |
| Ingress | **External** — public, target port `8080` | **Internal only** (`external: false`) — target port `8080`, reachable only from inside the Container Apps environment |
| Identity | None — no Azure data-plane calls | The environment's own backend UAMI — also the SQL Entra admin |
| Scaling (`test`) | `minReplicas: 0`, `maxReplicas: 3` | `minReplicas: 0`, `maxReplicas: 3` |
| Scaling (`prod`) | `minReplicas: 3`, `maxReplicas: 10` | `minReplicas: 3`, `maxReplicas: 10` |
| Key env vars | `BACKEND_URL` = backend's internal FQDN (injected by the platform, read by [nginx/entrypoint.sh](../workload-app/frontend/nginx/entrypoint.sh) at container start) | `ConnectionStrings__HotelDb` (built from the SQL module's output FQDN, `Authentication=Active Directory Default`, no secret), `AZURE_CLIENT_ID` = the UAMI's `clientId` (disambiguates the credential — see [§7](#7-identity)), `APPLICATIONINSIGHTS_CONNECTION_STRING` (App Insights connection string — not a secret) |
| Registry auth | None — anonymous public pull, no `registries[]` entry, no image-pull role assignment | Same |

`minReplicas: 3` on a zone-redundant environment is what actually spreads `prod`'s replicas
across availability zones — Container Apps handles the zone placement automatically once the
environment is zone-redundant and enough replicas are requested; no extra per-app parameter is
needed.

No Dapr, no custom domains, no client-certificate auth — none of those are required by this
app in either environment, and adding them would be unjustified complexity.

---

## 4. Data

| Setting | `test` | `prod` | Rationale |
|---|---|---|---|
| Service | Azure SQL Database, single database (no elastic pool, no Hyperscale, no Managed Instance) | Same | The app's data model (3 small tables, a demo catalogue) doesn't need anything beyond a single database (Cost) |
| Server | `sql-hotelbooking-test-belgiumcentral-${uniqueString(...)}` | `sql-hotelbooking-prod-belgiumcentral-${uniqueString(...)}` | CAF + global uniqueness; deterministic per resource group, no implementation-time naming choice |
| Database | `sqldb-hotelbooking-test` | `sqldb-hotelbooking-prod` | CAF |
| Compute tier | Serverless `GP_S_Gen5`, 1 vCore, auto-pause after 60 min | Serverless `GP_S_Gen5`, 2 vCores, auto-pause **disabled** (`-1`) | Same AVM code path, one template — `prod` just raises the ceiling and turns auto-pause off (Cost for `test`, Reliability for `prod`) |
| Zone redundancy | `false` | `true` | `prod` requirement (§0) |
| Network | `publicNetworkAccess: Disabled`; reachable only via a private endpoint in each environment's own `snet-private-endpoints` | Same pattern, isolated per spoke | [workload-network-exposure.instructions.md](../.github/instructions/workload-network-exposure.instructions.md) |
| **Connection policy** | **`Proxy`** (not the service default, `Default`/`Redirect`) | Same — applies identically in both environments | **Discovered during the `prod` deployment, retroactively fixed in `test` too** (see callout below) |
| Auth | **Microsoft Entra-only authentication** (`azureADOnlyAuthentication: true`); Entra admin = that environment's own backend UAMI, set **declaratively** in Bicep | Same pattern, isolated identity per environment | [workload-identity.instructions.md](../.github/instructions/workload-identity.instructions.md) — no `az sql server ad-admin create`, no deployment script, no jumpbox |
| Schema/seed | Created by the app itself (`EnsureCreatedAsync` + seed) on first backend startup, as the Entra admin | Same | Matches what the app already does; no separate migration step to design |
| Connection string | Built from the SQL module's FQDN output at deploy time: `Server=tcp:<server>.database.windows.net,1433;Database=<db>;Authentication=Active Directory Default;Encrypt=True;` | Same pattern | No secret anywhere |

**Documented workshop simplification** (per workload-identity.instructions.md): a production
landing zone would make an Entra **group** the SQL admin and grant the app's MI a contained
database user with least privilege. Collapsing both into one UAMI is intentional for this
workshop; it is not a mistake to "fix" in implementation.

**Connection policy bug, found and fixed during the `prod` deployment:** Azure SQL's default
connection policy (`Default`, which resolves to `Redirect`) sends clients to a direct node
hostname (`*.worker.database.windows.net`) after the initial login. That hostname has no
record in our private DNS zone — only the server's own FQDN does — so the client falls back to
public DNS, gets a public IP, and is correctly refused (`publicNetworkAccess: Disabled`).
**Private Link requires `connectionPolicy: 'Proxy'`**, which routes every packet through the
gateway the private endpoint already reaches directly. `test` had been running on the broken
default and happened not to have hit it yet; once found in `prod`, the same one-line fix was
applied to `test` too — a deliberate, understood exception to "`test`'s what-if shows zero
changes" (that contract is about not accidentally perturbing `test` while adding `prod`
support, not about preserving a latent bug). Both environments were smoke-tested
(`/api/hotels` returning the full catalogue) immediately after.

---

## 5. Networking

Each environment has its own spoke, peered to the hub independently (see [§0.2](#02-address-space-split)
for the full address-space split). Per spoke:

| Subnet | `test` range | `prod` range | Purpose |
|---|---|---|---|
| `snet-private-endpoints` | `192.168.101.0/26` | `192.168.102.0/26` | Private endpoint for Azure SQL |
| `snet-containerapps` | `192.168.101.64/27` | `192.168.102.64/27` | Container Apps environment infrastructure subnet, delegated to `Microsoft.App/environments` |
| *(free)* | `192.168.101.96/27` + `192.168.101.128/25` | `192.168.102.96/27` + `192.168.102.128/25` | Headroom for future subnets in either environment |

No NSG is specified in this design beyond the defaults the AVM virtual-network module applies;
a dedicated NSG per subnet is a reasonable follow-up hardening step but isn't required to meet
this chore's success criteria and isn't blocking implementation.

---

## 6. DNS

| Zone | Hosted in | Linked to | Purpose |
|---|---|---|---|
| `privatelink.database.windows.net` (`test`) | `rg-hotelbooking-test-belgiumcentral` | `test`'s own spoke VNet + the hub VNet | Resolves `test`'s SQL private endpoint FQDN to its private IP |
| `privatelink.database.windows.net` (`prod`) | `rg-hotelbooking-prod-belgiumcentral` | `prod`'s own spoke VNet + the hub VNet | Resolves `prod`'s SQL private endpoint FQDN to its private IP |

**Each environment owns its own zone — zones are never shared across environments.** Even
though both zones have the identical name `privatelink.database.windows.net`, Azure Private
DNS zone names are scoped per resource group, so this isn't a naming collision; it is the
distributed Private DNS pattern applied per environment, giving `test` and `prod` fully
isolated DNS resolution with no cross-environment dependency.

No other private DNS zones are needed:

- The Container Apps environment's own service discovery (frontend → backend) is handled
  automatically by the platform (apps resolve each other by name within the same environment);
  it is **not** a Private Link resource and needs no `privatelink.*` zone.
- Monitor (Log Analytics, Application Insights, its Data Collection Endpoint) stays fully
  public — **no** `privatelink.monitor.azure.com` / `privatelink.applicationinsights.azure.com`
  zone is created, per
  [workload-network-exposure.instructions.md](../.github/instructions/workload-network-exposure.instructions.md).
- No Key Vault, Storage Account, or other private PaaS is part of this workload (see the app
  analysis in §1) — so there's nothing else to add a zone for.

### Browser telemetry decision

The frontend's OTLP exporter needs a trace-ingestion URL. Azure Monitor now ingests OTLP
natively via Application Insights, once OTLP support is turned on for the resource — this
provisions a public Data Collection Endpoint (DCE) with a resource-specific ingestion URL
(`https://<dce>.<region>.ingest.monitor.azure.com/traces/otlp/v1/…`), publicly *reachable*
under the same rule that keeps the rest of the Monitor stack public.

**Reachable is not the same as unauthenticated.** The DCE's OTLP endpoint still requires a
Microsoft Entra bearer token on every ingestion call. A browser client cannot hold an Azure
credential, and a plain nginx reverse proxy (the mechanism used for `/api`) cannot attach one
on the browser's behalf — nginx has no identity. Simply pointing nginx at the DCE, the way it
points at the backend, is **not implementable**.

**Decision: browser RUM/OTLP export is out of scope for this design, in both environments.**
The frontend's OpenTelemetry Web SDK is left wired to its default relative `/otel` path with no
backing endpoint — the SDK fails closed (dropped spans, no user-facing effect), the same
fail-safe shape as the backend's own telemetry, which is a no-op unless
`APPLICATIONINSIGHTS_CONNECTION_STRING` is set (§1.1). No nginx template change is needed for
this design.

If browser RUM becomes a requirement later, the correct pattern is a small internal
OpenTelemetry Collector container app (its own UAMI, internal-only ingress, authenticating
server-side to the DCE) that nginx proxies `/otel` to anonymously — the collector holds the
credential, not the browser or nginx. That is a new component and a new decision, intentionally
not made here; it is **not** implied or required by anything else in this design.

---

## 7. Identity

Two managed identities **per environment**, both **user-assigned**, kept strictly separate per
[workload-identity.instructions.md](../.github/instructions/workload-identity.instructions.md).
Identities are never shared across environments any more than they're shared across runtime
and CI/CD within one environment:

| Identity | Assigned to | Role / scope | Purpose |
|---|---|---|---|
| `id-hotelapi-test-belgiumcentral-001` | **Runtime** — `test`'s backend container app only | `test`'s SQL server Entra admin (declarative); no other RBAC | Lets `test`'s backend authenticate to SQL passwordlessly and own the schema. **Cannot redeploy or modify infrastructure.** |
| `id-hotelapi-prod-belgiumcentral-001` | **Runtime** — `prod`'s backend container app only | `prod`'s SQL server Entra admin (declarative); no other RBAC | Same pattern, fully isolated from `test`'s identity and database. |
| `id-cicd-hotelbooking-test-belgiumcentral-001` | **CI/CD** — GitHub Actions, `test` environment only, federated via OIDC (no stored secret) | `Contributor` on `rg-hotelbooking-test-belgiumcentral`; `Network Contributor` on `rg-platform` (needed to write the spoke→hub peering and because the AVM virtual-network module's remote-peering nested deployment runs in the hub RG) | Deploys `test`'s infrastructure. **Has no data-plane access to SQL and is not attached to any container app.** |
| `id-cicd-hotelbooking-prod-belgiumcentral-001` | **CI/CD** — GitHub Actions, `prod` environment only, federated via OIDC (no stored secret) | `Contributor` on `rg-hotelbooking-prod-belgiumcentral`; `Network Contributor` on `rg-platform` | Deploys `prod`'s infrastructure only. **Cannot touch `test`'s resource group, and `test`'s CI/CD identity cannot touch `prod`'s.** |

The frontend app has **no identity** — it makes no Azure data-plane calls (§1.2), so assigning
it one would be an unused credential.

Wiring the federated credential (issuer `https://token.actions.githubusercontent.com`,
audience `api://AzureADTokenExchange`, subject `repo:<owner>/<repo>:environment:<env>` for each
of `test` and `prod`) and the GitHub Environment/variables is a follow-up implementation step,
not an open design question — the identities, their names, and their exact scopes are fixed
here, per environment.

### 7.1 Infrastructure deployment pipeline

[.github/workflows/infra-deploy.yml](../.github/workflows/infra-deploy.yml) deploys
infrastructure on every push to `main` that touches `infra/**`, and on manual dispatch, as three
chained jobs: **`lint`** (Bicep build + lint of every file under `infra/`, no Azure login) →
**`deploy-test`** (GitHub Environment `test`, unprotected, deploys automatically) →
**`deploy-prod`** (GitHub Environment `prod`, waits for a required reviewer, runs only from
`main`). Each deploy job logs in with `azure/login` over OIDC using that environment's
`vars.AZURE_*`, runs `what-if` first and writes it to the job summary, then deploys. Because
the approval gate sits before the job starts, the `prod` what-if is produced right after
approval, immediately ahead of the apply; the `test` run on the same commit is the preview.

Both stages deploy the **same template**, [infra/main.bicep](../infra/main.bicep), selecting
only `infra/main.test.bicepparam` or `infra/main.prod.bicepparam`. It is a thin
resource-group-scoped composition of the spoke VNet + hub peering (the same AVM module the
spoke template uses) and the unchanged [infra/workload/main.bicep](../infra/workload/main.bicep),
so one deployment covers the whole environment. It stays at **resource-group scope** because the
CI/CD identities (§7) only hold `Contributor` on an existing workload resource group — they
cannot deploy at subscription scope or create resource groups. The resource group itself is
created once by [infra/spoke-network](../infra/spoke-network/main.bicep) (an onboarding step by a
principal with subscription rights). A what-if of the composition against the live `test` and
`prod` environments shows no creates or deletes — only the same server-defaulted property noise
as the standalone templates — so adopting the pipeline does not change existing resources.

The per-environment values in `infra/main.<env>.bicepparam` mirror the values in
`infra/spoke-network` and `infra/workload`; when changing an environment's settings, update
both places.

"`prod` restricted to `main`" is enforced in the workflow (`if: github.ref ==
'refs/heads/main'` on `deploy-prod`) rather than as a GitHub deployment-branch policy, matching
this workshop's environment setup.

---

## 8. GHCR image references

| App | Image |
|---|---|
| Backend | `ghcr.io/azureholic/az-platform-engineering-workshop/backend:<tag>` |
| Frontend | `ghcr.io/azureholic/az-platform-engineering-workshop/frontend:<tag>` |

Both are built and published by
[.github/workflows/build-and-publish-workload-images.yml](../.github/workflows/build-and-publish-workload-images.yml)
whenever `workload-app/**` changes, tagged with **both** the full commit SHA (`type=sha,format=long`)
and `latest` (on the default branch only). GHCR is **external** to the Azure workload — not a
workload endpoint, not something the platform operates — and both packages must be set
**public** (separately, in GHCR package settings; the workflow does not do this) so the
container apps can pull anonymously. No registry credentials, no `Microsoft.ContainerRegistry`
resource for workload images.

**Image tag is a Bicep parameter, not a hardcoded `latest`.** `latest` is mutable — the same
tag can point at different code over time, so a container app revision pinned to `latest` can
silently start running different code on a restart/new revision, which is both a reproducibility
and a rollback problem. The implementation must expose an `imageTag` parameter (consumed by
both container apps) so a given deployment is pinned to a specific, immutable commit SHA; `latest`
remains available as a convenience default for the very first bootstrap deploy only.

---

## 9. Inbound exposure summary

Applies identically to both environments (each with its own instances of every resource):

| Component | Reachable from | Mechanism |
|---|---|---|
| `ca-hotelweb` (frontend) | **Public internet** | External ingress on the Container Apps environment's public static IP |
| `ca-hotelapi` (backend) | Only `ca-hotelweb`, inside the Container Apps environment | Internal-only ingress (`external: false`) |
| Azure SQL | Only `ca-hotelapi`, over the private endpoint | `publicNetworkAccess: Disabled` |
| Log Analytics / Application Insights / its DCE | **Public internet** (ingestion + query) | Unchanged — explicitly required to stay public |
| GHCR | External, public | Not a workload endpoint |

The frontend and the Monitor stack are the **only** public workload surfaces, in both `test`
and `prod`. Everything else is private-only, matching
[workload-network-exposure.instructions.md](../.github/instructions/workload-network-exposure.instructions.md).

---

## 10. Naming reference (CAF, environment token from day one)

| Resource | `test` | `prod` |
|---|---|---|
| Resource group | `rg-hotelbooking-test-belgiumcentral` | `rg-hotelbooking-prod-belgiumcentral` |
| Spoke VNet | `vnet-hotelbooking-test-belgiumcentral-001` | `vnet-hotelbooking-prod-belgiumcentral-001` |
| Private endpoint subnet | `snet-private-endpoints` | `snet-private-endpoints` |
| Container Apps subnet | `snet-containerapps` | `snet-containerapps` |
| Container Apps environment | `cae-hotelbooking-test-belgiumcentral-001` | `cae-hotelbooking-prod-belgiumcentral-001` |
| Frontend container app | `ca-hotelweb-test-belgiumcentral` | `ca-hotelweb-prod-belgiumcentral` |
| Backend container app | `ca-hotelapi-test-belgiumcentral` | `ca-hotelapi-prod-belgiumcentral` |
| SQL logical server | `sql-hotelbooking-test-belgiumcentral-${uniqueString(...)}` | `sql-hotelbooking-prod-belgiumcentral-${uniqueString(...)}` |
| SQL database | `sqldb-hotelbooking-test` | `sqldb-hotelbooking-prod` |
| Private endpoint (SQL) | `pep-sql-hotelbooking-test-001` | `pep-sql-hotelbooking-prod-001` |
| Private DNS zone | `privatelink.database.windows.net` (in `test`'s RG) | `privatelink.database.windows.net` (in `prod`'s RG, isolated) |
| Runtime managed identity | `id-hotelapi-test-belgiumcentral-001` | `id-hotelapi-prod-belgiumcentral-001` |
| CI/CD managed identity | `id-cicd-hotelbooking-test-belgiumcentral-001` | `id-cicd-hotelbooking-prod-belgiumcentral-001` |
| Log Analytics workspace | `log-hotelbooking-test-belgiumcentral-001` | `log-hotelbooking-prod-belgiumcentral-001` |
| Application Insights | `appi-hotelbooking-test-swedencentral-001` | `appi-hotelbooking-prod-swedencentral-001` |

Container app names drop the usual `-001` instance suffix — see the rationale inline in
`main.bicep` (a tight 32-character ARM name limit leaves no room for it once combined with a
longer region token).

---

## 11. Decisions and rationale

| Decision | Primary pillar | One-line rationale |
|---|---|---|
| Azure Container Apps over App Service/AKS/ACI/Functions | Cost + Ops | Only option that combines scale-to-zero with private VNet networking and low operational overhead for a 2-container app |
| Single environment per deployment, per-app ingress split (external/internal) | Security + Ops | Keeps the backend off the internet without an extra gateway component |
| Workload-profile environment (not Consumption-only) | Cost | `/27` subnet vs. `/23` — fits the spoke address plan in both environments |
| SQL serverless, in both `test` and `prod` | Cost + Ops | One AVM code path for both environments; `prod` just raises the capacity ceiling and disables auto-pause rather than forking onto a provisioned SKU family |
| Backend UAMI = SQL Entra admin, Entra-only auth, per environment | Security | No password anywhere; matches workload-identity.instructions.md exactly; `test` and `prod` identities are never shared |
| Separate CI/CD UAMI per environment, scoped to Contributor (own RG) + Network Contributor (hub RG) | Security | CI cannot touch SQL data-plane; runtime identity cannot redeploy infrastructure; `test`'s CI identity cannot touch `prod`'s resource group or vice versa |
| No identity on the frontend app | Security | Unused credentials are an attack surface with no benefit |
| One `privatelink.database.windows.net` zone per environment, no others, never shared | Ops | Nothing else in this workload is a Private Link resource; isolates DNS resolution per environment |
| Monitor stays fully public | Reliability/Ops | Required by workshop constraint; also avoids a private-ingestion failure mode blocking diagnostics when the spoke itself is unhealthy |
| Frontend OTLP export descoped | Ops | A plain reverse proxy can't authenticate anonymous browser calls to Azure Monitor's DCE; avoids shipping an unimplementable design |
| Image tag is a Bicep parameter, not hardcoded `latest` | Reliability | `latest` is mutable; pinning to an immutable commit-SHA tag makes deployments reproducible and rollback-able |
| SQL server name includes a deterministic `uniqueString()` suffix | Ops | Satisfies SQL's global-uniqueness requirement without leaving an ad hoc naming choice for implementation |
| `prod` co-located with `test` in Belgium Central (hub stays in Sweden Central) | Ops | Keeps both spokes topologically identical; the region already has 3 availability zones, verified directly — the real test of per-service zone-redundancy rollout is the actual `prod` deploy |
| Every `test`/`prod` difference is a parameter value; zero environment `if` branches | Ops | A working `test` deployment is real evidence `prod` will work too — the same lines of Bicep deploy both |
| No NSG design beyond AVM defaults | Cost/Ops | Not required to meet this chore's success criteria; flagged as an optional hardening follow-up, not a gap |

---

## 12. Explicitly out of scope for this design

- The federated credential and GitHub Environment/variable wiring for the CI/CD identities (the
  identities, names, and scopes are fixed here per environment; the wiring is a follow-up
  implementation step).
- Browser RUM/OTLP export (an authenticated OpenTelemetry Collector would be required — see
  §6; not needed for either environment's success criteria).
- Per-subnet NSGs (optional hardening, not required for sign-off).
- The multi-subscription split described in this chore's own background reading (hub,
  `test`, and `prod` each in their own subscription) — this workshop runs everything in one
  subscription; that's a real landing-zone pattern, not a gap in this design.

---

## Design review

This design was challenged before sign-off (per this chore's requirement that disagreements be
resolved, not skipped), in two passes.

### Self-review

Run while drafting, focused on whether any stated mechanism actually holds up against how
Azure Container Apps and Private DNS work:

| # | Challenge raised | Resolution |
|---|---|---|
| 1 | Does ingress really split per-app inside one environment, or does "Internal" vs "External" apply only at the environment level? | Confirmed via Microsoft Learn: the environment's internal/external setting is a **ceiling** (an Internal environment can't expose anything publicly), but **within an External environment, each app's own `ingress.external` flag can still be `false`** to stay internal-only. Design in §3 relies on exactly this — documented explicitly so implementation doesn't re-derive it. |
| 2 | Does the backend tier need its own Private Endpoint + Private DNS zone on the Container Apps environment, the way the instructions' private-services list suggests? | Re-read as the compute-specific alternative it is: "Container Apps internal ingress **/** App Service with private endpoint" — internal ingress is ACA's native equivalent of a private endpoint for this purpose (the environment is already VNet-injected; internal ingress alone makes the backend unreachable from outside the VNet). No separate PE/DNS zone is created for the environment itself; §6 states this explicitly so it isn't re-litigated during implementation. |
| 3 | Is a Private DNS zone linked only to the hub (as the general instruction literally lists), or does it also need the spoke link? | The backend, which needs to resolve the SQL private endpoint, lives in the **spoke** — a zone with no spoke link would not resolve for its own consumer. §6 links the zone to **both** spoke and hub, with the reasoning spelled out, rather than leaving a resolution gap. |
| 4 | Is collapsing the SQL Entra admin and the app's runtime identity into one UAMI a design smell? | Yes, by production standards — but it is a **documented workshop simplification** mandated by workload-identity.instructions.md, not an oversight. Recorded as such in §4 rather than "fixed" by inventing a second identity the workshop doesn't call for. |

### Independent review

A separate review pass (an agent with no prior involvement in drafting the document) was run
against the same constraints and asked to find concrete, severity-rated issues rather than
confirm the draft. It returned two real issues, both resolved by edits to this document (not
by argument):

| # | Severity | Issue raised | Resolution |
|---|---|---|---|
| 5 | Blocking | The original §6 had nginx proxy browser OTLP straight to the Azure Monitor DCE — but the DCE's OTLP endpoint requires an authenticated (Entra bearer token) call, and neither the browser nor a plain nginx proxy holds a credential. As drafted, it was not implementable. | §6 rewritten: browser RUM/OTLP is explicitly **descoped** for this environment (fails closed, no user-facing effect, matches the backend's own conditional-telemetry pattern). An authenticated OTel Collector container app is named as the correct future path, explicitly not built now. |
| 6 | Should-fix | Both container apps were specified pulling `:latest`, which is mutable — a later revision/restart could silently run different code than what was tested, with no rollback anchor. | §3.2 and §8 changed to an `imageTag` Bicep parameter (pinned to an immutable commit SHA); `latest` remains only as the bootstrap-deploy default. |
| 7 | Should-fix | The SQL server name deferred its required global-uniqueness suffix to "implementation time," leaving a naming decision unmade. | §4 and §10 now specify a deterministic name, `sql-hotelbooking-test-swedencentral-${uniqueString(resourceGroup().id)}`, stable across redeploys of the same resource group. |

No disagreement remains open — every raised challenge resulted in a document change, not a
rebuttal.

### Prod-addition review

Run specifically against the `prod` extension in this update, checking for the two failure
modes the chore itself warns about:

| # | Challenge raised | Resolution |
|---|---|---|
| 8 | Does adding `prod` support require any `if (environment == …)` branch anywhere in the template? | No — every `test`/`prod` difference (region, address space, replica counts, SQL SKU/capacity, auto-pause, `zoneRedundant`) is already a parameter in the existing template or a straightforward new parameter. Verified by grepping the implemented `main.bicep` for `environment ==` / `environmentName ==` after implementation: zero matches. |
| 9 | Does `zoneRedundant: true` actually work in Belgium Central, or should `prod` move to a region explicitly confirmed by the chore's reliability note? | Checked directly: Belgium Central does have 3 physical availability zones. The chore's reliability note doesn't list it among confirmed-good regions, and newly-opened regions can lag on a specific service's zone-redundancy rollout even when the region itself has zones — so this was flagged as a real, accepted risk rather than hidden. `prod` is deployed alongside `test` in Belgium Central to keep both spokes topologically identical; the actual `prod` deployment (what-if + real apply) is the authoritative test of whether zone-redundant Container Apps/SQL are available there today. |
| 10 | Will redeploying `test` after these template changes actually show a no-op, or does adding new parameters implicitly change `test`'s resources? | Every new/changed parameter (`zoneRedundant`, replica counts, etc.) is given a default equal to `test`'s already-deployed value in `main.test.bicepparam`. Verified empirically after implementation: `Deploy-Workload.ps1 -Environment test` what-if shows the same stable benign-noise set as before, zero new creates/modifies tied to the new parameters. |
| 11 | Does the hub-side peering name collide once a second spoke peers to the same hub VNet? | Yes — caught by the real `prod` deployment: the original template hardcoded the hub-side peering name (`peer-hub-to-spoke`), and Azure rejects a second peering trying to point that same name at a different remote VNet (`ChangingRemoteVirtualNetworkNotAllowed`). Fixed by making both peering names explicit parameters, pinned to `test`'s exact existing names in `main.test.bicepparam` so `test` is unaffected, with a distinct pair for `prod`. |
| 12 | Can both environments' Private DNS zones link to the shared hub VNet, as originally designed? | No — caught by the real `prod` deployment: Azure refuses to link one VNet to two zones sharing the same namespace (`privatelink.database.windows.net`), even across resource groups (`BadRequest: cannot be linked to multiple zones with overlapping namespaces`). Only the already-linked `test` zone keeps its hub link (`linkPrivateDnsZoneToHub: true`, matching its deployed state); `prod`'s zone links only to its own spoke (`linkPrivateDnsZoneToHub: false`) — the spoke link is the only one functionally required, since only the backend container app (which lives in the spoke) ever resolves the private endpoint's FQDN. |
| 13 | Does Azure SQL actually work through its private endpoint with the connection string this design specifies? | Not by default — caught by the real `prod` deployment (the backend crashed in a loop with "Deny Public Network Access"). Azure SQL's default `Redirect` connection policy bypasses the private endpoint for the post-login data connection. Fixed with `connectionPolicy: 'Proxy'` (§4), applied to both environments once found. |

All four issues raised during the real `prod` deployment were caught by actually running the
deployment, not by review alone — consistent with this design's own principle that `test`
passing is only partial evidence until `prod` is proven the same way.

---

## Success criteria check

| Chore requirement | Where addressed |
|---|---|
| App analysed end-to-end (runtime, framework, endpoints, frontend↔backend, data store, auth, startup config) | §1 |
| Design + diagram in `docs/` | This file + [diagrams/workload-architecture.drawio](diagrams/workload-architecture.drawio) / `.png` |
| Container hosting service chosen and justified against alternatives | §2 |
| CAF names carry the environment token from the start, for both `test` and `prod` | §10 |
| Well-Architected, scale-to-zero (`test`)/zone-redundant (`prod`), private endpoints for private PaaS | §3, §4, §9, §11 |
| Dedicated CI/CD managed identity per environment, separate from runtime, scope recorded | §7, §0.4 |
| Decisions recorded with rationale; detailed enough for a follow-up implementation | §11, §12 |
| Design reviewed and challenged, disagreements resolved | [Design review](#design-review) |
| Private PaaS private; frontend + Monitor the only public workload endpoints; GHCR external | §9 |
| One template, two parameter files; address-space split; parameter-driven model documented before Bicep changes | §0 |
| Each environment on its own spoke, independent hub peering, isolated distributed Private DNS | §0.2, §5, §6 |
