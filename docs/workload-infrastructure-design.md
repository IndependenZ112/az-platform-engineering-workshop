# Hotel Booking workload — `test` environment infrastructure design

| | |
|---|---|
| **Status** | Reviewed and signed off (see [Design review](#design-review)) |
| **Environment framed** | `test` |
| **Region** | `swedencentral` |
| **Owning resource group** | `rg-hotelbooking-test` (spoke VNet already deployed) |
| **Diagram** | [diagrams/workload-architecture.drawio](diagrams/workload-architecture.drawio) · [diagrams/workload-architecture.png](diagrams/workload-architecture.png) |

This document is a design, not Bicep. It is detailed enough that a follow-up implementation
can build the workload without re-opening an architectural question.

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

| Setting | Value | Why |
|---|---|---|
| Name | `cae-hotelbooking-test-swedencentral-001` | CAF |
| Type | **Workload profiles** environment, with only the built-in **Consumption** profile (no Dedicated profile) | Smallest subnet footprint (`/27`) + scale-to-zero; a Dedicated profile isn't needed at `test` scale |
| VNet integration | Injected into `snet-containerapps` (new subnet, see [§5](#5-networking)), delegated to `Microsoft.App/environments` | Required for any VNet-integrated environment |
| External/internal | **External** (environment has a public static IP) | Needed so the frontend app can be reached from the internet; the backend app overrides this per-app (see below) — the environment setting is a ceiling, not a floor |
| Zone redundancy | Off for `test` | Cost; revisit for a production environment — out of scope for this design |

### 3.2 Container apps

| | `ca-hotelweb-test-swedencentral-001` (frontend) | `ca-hotelapi-test-swedencentral-001` (backend) |
|---|---|---|
| Image | `ghcr.io/azureholic/az-platform-engineering-workshop/frontend:<tag>` (public, anonymous pull) | `ghcr.io/azureholic/az-platform-engineering-workshop/backend:<tag>` (public, anonymous pull) |
| Ingress | **External** — public, target port `8080` | **Internal only** (`external: false`) — target port `8080`, reachable only from inside the Container Apps environment |
| Identity | None — no Azure data-plane calls | **`id-hotelapi-test-swedencentral-001`** (UAMI) — also the SQL Entra admin |
| Scaling | HTTP scale rule, `minReplicas: 0`, `maxReplicas: 3` (test) | HTTP scale rule, `minReplicas: 0`, `maxReplicas: 3` (test) |
| Key env vars | `BACKEND_URL` = backend's internal FQDN (injected by the platform, read by [nginx/entrypoint.sh](../workload-app/frontend/nginx/entrypoint.sh) at container start) | `ConnectionStrings__HotelDb` (built from the SQL module's output FQDN, `Authentication=Active Directory Default`, no secret), `AZURE_CLIENT_ID` = the UAMI's `clientId` (disambiguates the credential — see [§7](#7-identity)), `APPLICATIONINSIGHTS_CONNECTION_STRING` (App Insights connection string — not a secret) |
| Registry auth | None — anonymous public pull, no `registries[]` entry, no image-pull role assignment | Same |

No Dapr, no custom domains, no client-certificate auth — none of those are required by this
app, and adding them would be unjustified complexity for a `test` environment.

---

## 4. Data

| Setting | Value | Rationale |
|---|---|---|
| Service | Azure SQL Database, single database (no elastic pool, no Hyperscale, no Managed Instance) | The app's data model (3 small tables, a demo catalogue) doesn't need anything beyond a single database (Cost) |
| Server | `sql-hotelbooking-test-swedencentral-${uniqueString(resourceGroup().id)}` — deterministic (stable across redeploys of the same resource group), satisfying SQL server's global-uniqueness requirement without an ad hoc implementation-time choice | CAF + global uniqueness |
| Database | `sqldb-hotelbooking-test` | CAF |
| Compute tier | **Serverless, General Purpose** (e.g. `GP_S_Gen5`, 1–2 vCores), **auto-pause enabled** | Extends the scale-to-zero philosophy to the data tier for a `test` environment that's idle most of the time (Cost); auto-pause/resume latency is acceptable for a demo workload |
| Network | `publicNetworkAccess: Disabled`; reachable only via a private endpoint in `snet-private-endpoints` | [workload-network-exposure.instructions.md](../.github/instructions/workload-network-exposure.instructions.md) |
| Auth | **Microsoft Entra-only authentication** (`azureADOnlyAuthentication: true`); Entra admin = `id-hotelapi-test-swedencentral-001`, the **same UAMI the backend runs as**, set **declaratively** in Bicep | [workload-identity.instructions.md](../.github/instructions/workload-identity.instructions.md) — no `az sql server ad-admin create`, no deployment script, no jumpbox |
| Schema/seed | Created by the app itself (`EnsureCreatedAsync` + seed) on first backend startup, as the Entra admin | Matches what the app already does; no separate migration step to design |
| Connection string | Built from the SQL module's FQDN output at deploy time: `Server=tcp:<server>.database.windows.net,1433;Database=sqldb-hotelbooking-test;Authentication=Active Directory Default;Encrypt=True;` | No secret anywhere |

**Documented workshop simplification** (per workload-identity.instructions.md): a production
landing zone would make an Entra **group** the SQL admin and grant the app's MI a contained
database user with least privilege. Collapsing both into one UAMI is intentional for this
workshop; it is not a mistake to "fix" in implementation.

---

## 5. Networking

The spoke (`vnet-hotelbooking-test-swedencentral-001`, `192.168.101.0/24`, peered to the hub)
already exists. This design adds one subnet to it:

| Subnet | Range | Status | Purpose |
|---|---|---|---|
| `snet-private-endpoints` | `192.168.101.0/26` | **Exists** (already deployed) | Private endpoint for Azure SQL |
| `snet-containerapps` | `192.168.101.64/27` | **New** — to add in implementation | Container Apps environment infrastructure subnet, delegated to `Microsoft.App/environments` |
| *(free)* | `192.168.101.96/27` + `192.168.101.128/25` | Reserved | Headroom for future subnets (e.g. a prod-style dedicated workload profile, or an added data service) |

No NSG is specified in this design beyond the defaults the AVM virtual-network module applies;
a dedicated NSG per subnet is a reasonable follow-up hardening step but isn't required to meet
this chore's success criteria and isn't blocking implementation.

---

## 6. DNS

| Zone | Hosted in | Linked to | Purpose |
|---|---|---|---|
| `privatelink.database.windows.net` | `rg-hotelbooking-test` (distributed Private DNS pattern — **not** the hub) | The **spoke** VNet (so the backend, which lives there, resolves the private endpoint) and the **hub** VNet (consistent with the hub-and-spoke pattern this workshop uses for every private workload service) | Resolves the SQL private endpoint's FQDN to its private IP |

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

**Decision: browser RUM/OTLP export is out of scope for this `test` environment design.** The
frontend's OpenTelemetry Web SDK is left wired to its default relative `/otel` path with no
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

Two managed identities, both **user-assigned**, kept strictly separate per
[workload-identity.instructions.md](../.github/instructions/workload-identity.instructions.md):

| Identity | Assigned to | Role / scope | Purpose |
|---|---|---|---|
| `id-hotelapi-test-swedencentral-001` | **Runtime** — the backend container app only | SQL server Entra admin (declarative, via the SQL module's `administrator` block); no Azure RBAC role assignments beyond that | Lets the backend authenticate to SQL passwordlessly and own the schema. **Cannot redeploy or modify infrastructure.** |
| `id-cicd-hotelbooking-test-swedencentral-001` | **CI/CD** — GitHub Actions only, federated via OIDC (no stored secret) | `Contributor` on `rg-hotelbooking-test` (its own workload resource group); `Network Contributor` on `rg-platform` (the hub resource group — needed to write the spoke→hub peering and because the AVM virtual-network module's remote-peering nested deployment runs in the hub RG) | Deploys the workload's infrastructure. **Has no data-plane access to SQL and is not attached to any container app.** |

The frontend app has **no identity** — it makes no Azure data-plane calls (§1.2), so assigning
it one would be an unused credential.

Wiring the federated credential (issuer `https://token.actions.githubusercontent.com`,
audience `api://AzureADTokenExchange`, subject `repo:<owner>/<repo>:environment:test`) and the
GitHub Environment/variables is a follow-up implementation step, not an open design question —
the identity, its name, and its exact scope are fixed here.

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

| Component | Reachable from | Mechanism |
|---|---|---|
| `ca-hotelweb` (frontend) | **Public internet** | External ingress on the Container Apps environment's public static IP |
| `ca-hotelapi` (backend) | Only `ca-hotelweb`, inside the Container Apps environment | Internal-only ingress (`external: false`) |
| Azure SQL | Only `ca-hotelapi`, over the private endpoint | `publicNetworkAccess: Disabled` |
| Log Analytics / Application Insights / its DCE | **Public internet** (ingestion + query) | Unchanged — explicitly required to stay public |
| GHCR | External, public | Not a workload endpoint |

The frontend and the Monitor stack are the **only** public workload surfaces. Everything else
is private-only, matching
[workload-network-exposure.instructions.md](../.github/instructions/workload-network-exposure.instructions.md).

---

## 10. Naming reference (CAF, `test` token from day one)

| Resource | Name |
|---|---|
| Resource group | `rg-hotelbooking-test` *(exists)* |
| Spoke VNet | `vnet-hotelbooking-test-swedencentral-001` *(exists)* |
| Private endpoint subnet | `snet-private-endpoints` *(exists)* |
| Container Apps subnet | `snet-containerapps` *(new)* |
| Container Apps environment | `cae-hotelbooking-test-swedencentral-001` |
| Frontend container app | `ca-hotelweb-test-swedencentral-001` |
| Backend container app | `ca-hotelapi-test-swedencentral-001` |
| SQL logical server | `sql-hotelbooking-test-swedencentral-${uniqueString(resourceGroup().id)}` |
| SQL database | `sqldb-hotelbooking-test` |
| Private endpoint (SQL) | `pep-sql-hotelbooking-test-001` |
| Private DNS zone | `privatelink.database.windows.net` |
| Runtime managed identity | `id-hotelapi-test-swedencentral-001` |
| CI/CD managed identity | `id-cicd-hotelbooking-test-swedencentral-001` |
| Log Analytics workspace | `log-hotelbooking-test-swedencentral-001` |
| Application Insights | `appi-hotelbooking-test-swedencentral-001` |

---

## 11. Decisions and rationale

| Decision | Primary pillar | One-line rationale |
|---|---|---|
| Azure Container Apps over App Service/AKS/ACI/Functions | Cost + Ops | Only option that combines scale-to-zero with private VNet networking and low operational overhead for a 2-container app |
| Single environment, per-app ingress split (external/internal) | Security + Ops | Keeps the backend off the internet without a second environment or an extra gateway component |
| Workload-profile environment (not Consumption-only) | Cost | `/27` subnet vs. `/23` — fits the existing spoke address plan |
| SQL serverless + auto-pause | Cost | Matches scale-to-zero philosophy at the data tier for an idle-most-of-the-time `test` environment |
| Backend UAMI = SQL Entra admin, Entra-only auth | Security | No password anywhere; matches workload-identity.instructions.md exactly |
| Separate CI/CD UAMI, scoped to Contributor (own RG) + Network Contributor (hub RG) | Security | CI cannot touch SQL data-plane; runtime identity cannot redeploy infrastructure |
| No identity on the frontend app | Security | Unused credentials are an attack surface with no benefit |
| Single `privatelink.database.windows.net` zone, no others | Ops | Nothing else in this workload is a Private Link resource; avoids speculative zones |
| Monitor stays fully public | Reliability/Ops | Required by workshop constraint; also avoids a private-ingestion failure mode blocking diagnostics when the spoke itself is unhealthy |
| Frontend OTLP export descoped for `test` | Ops | A plain reverse proxy can't authenticate anonymous browser calls to Azure Monitor's DCE; avoids shipping an unimplementable design |
| Image tag is a Bicep parameter, not hardcoded `latest` | Reliability | `latest` is mutable; pinning to an immutable commit-SHA tag makes deployments reproducible and rollback-able |
| SQL server name includes a deterministic `uniqueString()` suffix | Ops | Satisfies SQL's global-uniqueness requirement without leaving an ad hoc naming choice for implementation |
| No NSG design beyond AVM defaults | Cost/Ops | Not required to meet this chore's success criteria; flagged as an optional hardening follow-up, not a gap |

---

## 12. Explicitly out of scope for this design

- Zone-redundant SQL / Container Apps environment, and any production-specific sizing — a
  follow-up implementation, once a production environment is in scope.
- The federated credential and GitHub Environment/variable wiring for the CI/CD identity (the
  identity, name, and scope are fixed here; the wiring is a follow-up implementation step).
- Browser RUM/OTLP export (an authenticated OpenTelemetry Collector would be required — see
  §6; not needed for this environment's success criteria).
- Per-subnet NSGs (optional hardening, not required for sign-off).

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

---

## Success criteria check

| Chore requirement | Where addressed |
|---|---|
| App analysed end-to-end (runtime, framework, endpoints, frontend↔backend, data store, auth, startup config) | §1 |
| Design + diagram in `docs/` | This file + [diagrams/workload-architecture.drawio](diagrams/workload-architecture.drawio) / `.png` |
| Container hosting service chosen and justified against alternatives | §2 |
| Framed as `test`, CAF names carry the `test` token from the start | §10 |
| Well-Architected, scale-to-zero, private endpoints for private PaaS | §3, §4, §9, §11 |
| Dedicated CI/CD managed identity per environment, separate from runtime, scope recorded | §7 |
| Decisions recorded with rationale; detailed enough for a follow-up implementation | §11, §12 |
| Design reviewed and challenged, disagreements resolved | [Design review](#design-review) |
| Private PaaS private; frontend + Monitor the only public workload endpoints; GHCR external | §9 |
