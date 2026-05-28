# Azure Platform Engineering Workshop

A hands-on workshop that walks through the day-to-day work of a platform team on Azure: extending a centralized network, onboarding a new workload, and wiring up secure, identity-based access to a managed data service — all using Infrastructure as Code and GitHub Copilot.

## Introduction

In a real environment, the baseline for this workshop would be a full [Azure Landing Zone](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/landing-zone/) deployment — either the [Enterprise-scale](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/enterprise-scale/) Accelerator (ALZ) for larger organizations, the [SMB landing zone](https://learn.microsoft.com/azure/cloud-adoption-framework/scenarios/small-medium-enterprise/) variant, or the **SMB Ready Foundation**. These accelerators provision a full management-group hierarchy, policy baseline, identity, connectivity, and management subscriptions.

Standing all of that up takes time we don't have in a workshop. Instead, we **simulate** the centralized "platform" side of a landing zone by deploying a minimal **hub virtual network** into a single subscription. That hub stands in for the connectivity subscription you'd normally have in a full ALZ, so the workload-onboarding patterns we practice (peering, private endpoints, Private DNS) are the same ones you'd use in production.

A few design choices we lock in up front:

- **Single subscription.** All resources — platform and workload — live in one subscription. In a real ALZ they'd be split across a connectivity subscription and one or more landing-zone (workload) subscriptions.
- **Distributed Private DNS.** Private DNS zones for Private Link (e.g. `privatelink.database.windows.net`) are deployed and linked **per workload**, not centrally in the hub. This is the [distributed Private DNS pattern](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/azure-best-practices/private-link-and-dns-integration-at-scale) — simpler to reason about for a workshop and a valid ALZ option (the alternative being centralized zones in the connectivity subscription, managed via Azure Policy).
- **Hub-and-spoke topology.** Workload VNets are spokes that [peer](https://learn.microsoft.com/azure/virtual-network/virtual-network-peering-overview) to the hub.

## Prerequisites

- An **Azure subscription** with **Owner** permissions (needed for role assignments on the resources we deploy).
- **[Visual Studio Code](https://code.visualstudio.com/)**.
- A **GitHub Copilot** license (the workshop leans on Copilot — plus the Azure and Bicep MCP servers — to author IaC and run Azure operations).
- **[PowerShell 7+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)** (PowerShell Core) — the deployment scripts use `#requires -Version 7.0`.
- **[Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)** — the deployment scripts wrap `az` commands. Sign in with `az login` and select the target subscription with `az account set --subscription <id>` before running them.
- **[draw.io Desktop](https://www.drawio.com/)** — Chore 3 produces an architecture diagram via the `drawio` skill, which shells out to the draw.io CLI to export the `.drawio` source to a PNG with the editable XML embedded. Install on Windows with:

  ```powershell
  winget install --id JGraph.Draw -e --accept-source-agreements --accept-package-agreements
  ```

  On macOS use `brew install --cask drawio`; on Linux use the [official package](https://github.com/jgraph/drawio-desktop/releases).

## Prep the environment — deploy the mock landing zone

Goal: get a "platform" resource group in place that mimics the connectivity subscription of a landing zone.

What we deploy into **`rg-platform`**:

- A **hub VNet** (`vnet-hub`, `192.168.100.0/24`) with placeholder subnets — no gateway or firewall is actually deployed, the subnets exist so the topology is realistic:

  | Subnet                 | Range                 | Purpose                              |
  | ---------------------- | --------------------- | ------------------------------------ |
  | `AzureFirewallSubnet`  | `192.168.100.0/26`    | Reserved for Azure Firewall (`/26` minimum) |
  | `GatewaySubnet`        | `192.168.100.64/27`   | Reserved for VPN / ExpressRoute gateway |
  | `snet-shared-services` | `192.168.100.96/27`   | Shared platform services (DNS forwarders, jumpbox, etc.) |

  `192.168.100.128/25` is left free for growth.

- No central Private DNS zones — remember, we're using the **distributed** pattern, so each spoke/workload will own its own zones.

The Bicep template and deployment script live under [mock-alz/](mock-alz/). To deploy:

```powershell
cd mock-alz
./Deploy-Hub.ps1            # optionally: -Location westeurope -ResourceGroupName rg-platform
```

This resource group is the "given" for the rest of the workshop. Treat it as if another team owns it.

## How to work through the chores

Each chore below is meant to be completed **with GitHub Copilot** in agent mode — not by copy-pasting a finished solution. The flow for every chore is the same:

1. Open the chore in Copilot Chat and share the goals as the prompt.
2. Let Copilot draft the Bicep (and any deployment script) using the Azure and Bicep MCP servers for up-to-date schemas and best practices.
3. Review the diff, push back on anything that doesn't match the design choices in the intro (single subscription, distributed Private DNS, hub-and-spoke).
4. Deploy with `az` from PowerShell 7, verify against the goals, and iterate.

If Copilot proposes something that contradicts the workshop's choices (e.g. a centralized Private DNS zone in the hub), correct it and continue — that feedback loop is the point of the exercise.

## Chore 1 — Curate the Copilot toolbox for this repo

Scenario: before you start writing IaC or designing the workload, you set up the **agent toolbox** that Copilot will reach for during the rest of the workshop. The community maintains a large catalogue of reusable Copilot customizations at [aka.ms/awesome-copilot](https://aka.ms/awesome-copilot) (the [`github/awesome-copilot`](https://github.com/github/awesome-copilot) repo) — **agents**, **instructions**, and **skills**. Picking the right ones up front means Copilot follows your conventions (CAF naming, AVM, Bicep best practices, draw.io diagram authoring, deployment preflight, …) without you having to re-prompt them every chore.

Goals:

- Read this [README.md](README.md) and the [.github/copilot-instructions.md](.github/copilot-instructions.md) **with Copilot**, then ask it to **propose a shortlist** of items from [aka.ms/awesome-copilot](https://aka.ms/awesome-copilot) that fit this workshop. Browse the [`agents/`](https://github.com/github/awesome-copilot/tree/main/agents), [`instructions/`](https://github.com/github/awesome-copilot/tree/main/instructions), and [`skills/`](https://github.com/github/awesome-copilot/tree/main/skills) folders — or use the [machine-readable index](https://awesome-copilot.github.com/llms.txt) — and let Copilot match them to the chores ahead (Bicep + AVM authoring, Azure naming, deployment preflight, draw.io diagrams, container best practices, etc.).
- Review the shortlist together. Push back on anything that doesn't earn its place (overlap, scope creep, outdated guidance) — exactly the same review loop you'll apply to Copilot's IaC later.
- Install the agreed items into the repo so they apply automatically:
  - **Instructions** → `.github/instructions/*.instructions.md` (use `applyTo` glob front-matter so they activate on the right files).
  - **Skills** → `.github/skills/<skill-name>/SKILL.md` (plus any bundled assets the skill ships).
  - **Agents** → `.github/agents/<agent-name>.agent.md`.
- Verify with a quick smoke test: open a chat, ask Copilot to (for example) propose a Bicep resource name and confirm it cites the Azure naming instructions; ask for a Dockerfile and confirm it pulls from `mcr.microsoft.com` (per [.github/copilot-instructions.md](.github/copilot-instructions.md) rule 5).
- Commit the toolbox before moving on. From here, every chore assumes those skills/instructions/agents are loaded.

> **Tip.** The exact set of skills/instructions/agents will drift over time as the community publishes new ones. Treat this chore as recurring — re-running it every few months keeps the repo's Copilot configuration current.

## Chore 2 — Onboard a workload spoke

Scenario: a new application team needs network space. As the platform team, you add a **spoke VNet** that will host their workload and connect it to the hub.

Goals:

- A new resource group for the workload exists (e.g. `rg-workload-01`).
- A **spoke VNet** is deployed in a non-overlapping address space with at least:
  - A subnet for **private endpoints** (with `privateEndpointNetworkPolicies` configured appropriately).
  - Room to grow for app/data subnets later.
- **VNet peering** is in place between the hub and the spoke in both directions. For a hub without a gateway, `allowGatewayTransit` / `useRemoteGateways` stay off; `allowVirtualNetworkAccess` is on and `allowForwardedTraffic` is typically on too.
- The peering shows `Connected` on both sides and a resource placed in the spoke can reach the hub address space.

## Chore 3 — Investigate the workload and design its infrastructure

Scenario: the application team has dropped a workload in [workload-app/](workload-app/) — a .NET 10 Minimal API backend and a React/Vite frontend, with data persisted in SQL Server. Before we deploy anything, the platform team needs to **understand the app** and **design the Azure footprint** that will host it.

We're deliberately not writing any Bicep in this chore. The output is a **plan** — what Azure services we'll need, how they connect to the hub-and-spoke we built in Chore 2, and which decisions we're locking in before we touch IaC.

Goals:

- The workload is **analyzed end-to-end**:
  - What the backend is (runtime, framework, exposed endpoints, dependencies).
  - What the frontend is (build tooling, how it talks to the backend, how it's served in production).
  - What data store it expects, what authentication it uses, what configuration it reads at startup.
  - What runs in-process vs. what it calls out to.
- The app should be deployed as a modern containerized application.
- A short **infrastructure design** is produced (e.g. a `docs/` note or a Copilot chat summary) that answers:
  - Which compute service hosts the containers, and why it fits this workload.
  - Where container images live and how that registry is exposed.
  - How the workload reaches its data tier privately, reusing the spoke and the distributed Private DNS pattern.
  - How identity flows end-to-end — which managed identities exist, what they're allowed to do, and how secrets are kept out of config.
  - Which subnets the workload needs in the spoke and whether the Chore 2 address plan still fits.
  - Inbound exposure: how users reach the frontend, and whether anything in the hub is on the path.
- The design is **well-architected** — explicitly evaluated against the five pillars of the [Azure Well-Architected Framework](https://learn.microsoft.com/azure/well-architected/) (Reliability, Security, Cost Optimization, Operational Excellence, Performance Efficiency). Trade-offs are called out, not hidden.
- The design **scales to zero** to keep cost down when the app is idle. Every layer that can scale to zero should; layers that can't must be justified.
- **Private endpoints everywhere — with two explicit exceptions:**
  - The **container registry** stays publicly reachable (so the workshop's build/push from a laptop or runner works without a private build agent).
  - The **frontend app** stays publicly reachable (it's the user-facing entry point).
  - Every other PaaS service the workload depends on is reached only over **Private Link**, with Private DNS zones deployed per the distributed pattern from the intro.
- The plan is realistic enough that the **next chore can pick it up and start implementing** without re-litigating the architecture.

## Chore 4 — Implement the workload infrastructure in Bicep

Scenario: the design from Chore 3 is signed off. Now the platform team turns it into **deployable Bicep** that lands the workload in the spoke from Chore 2 — without re-opening any of the architectural decisions already locked in.

Goals:

- All Bicep lives under `infra/workload-01/` (or a similarly scoped folder) and **uses Azure Verified Modules** wherever an AVM module exists for the resource — per [.github/instructions/azure-verified-modules-bicep.instructions.md](.github/instructions/azure-verified-modules-bicep.instructions.md). Raw `Microsoft.*` resources are only used where AVM has no equivalent, and that exception is called out in a comment.
- Resource names follow **Microsoft CAF** — see [.github/instructions/azure-naming.instructions.md](.github/instructions/azure-naming.instructions.md).
- The template provisions exactly what the Chore 3 design document specifies — **no service is added or dropped without updating the design first.** If you discover a gap while writing Bicep, fix it in the design doc and the diagram before changing the template.
- Identity and access are wired end-to-end:
  - User-assigned managed identities for each container app, with `AcrPull` on the registry.
  - The backend's MI is granted access to the data tier as the design prescribes.
  - **No secrets in parameters, outputs, or config** — connection strings are constructed from resource properties at deploy time and passed in as plain env vars (auth is MI-based).
- Networking honours the **distributed Private DNS** pattern from the intro: every Private DNS zone is created in the workload resource group and linked to both the spoke VNet (registration) and the hub VNet (resolution).
- The template is **parameterised** (location, naming tokens, address space, SKU sizes) but ships with sensible defaults so a fresh participant can deploy with one command.
- A `Deploy-Workload.ps1` script (mirroring [mock-alz/Deploy-Hub.ps1](mock-alz/Deploy-Hub.ps1)) wraps the deployment. Before every `az deployment group create`, the script runs `azure-deployment-preflight` (Bicep what-if + permission checks) and refuses to deploy if either step fails.
- The deployment is **idempotent**: running it twice in a row produces a no-op what-if on the second run.
- Post-deploy steps that can't live in Bicep (e.g. creating the contained DB users for the UAMIs) are scripted alongside the template, **not** done by hand.
- After a successful deploy, the platform team can verify the design's promises in the portal / CLI: the public surface is exactly the frontend FQDN and the container registry; everything else is private; resting cost matches the scale-to-zero estimate from Chore 3.

> **Workflow tip.** Use the **`bicep-plan`** chat mode to draft the file layout and module choices, then switch to **`bicep-implement`** to write the resources. Keep the design doc from Chore 3 open in the chat context so Copilot grounds its decisions in the plan you already agreed on.

## Chore 5 — Deploy the workload infrastructure

Scenario: the Bicep from Chore 4 is reviewed and merged. Time to actually land it in the subscription.

Goals:

- Sign in (`az login`) and confirm the target subscription with `az account show`. This must be the **same subscription** as the hub from the prep step and the spoke from Chore 2 — single-subscription is a workshop invariant.
- Run **`azure-deployment-preflight`** end-to-end against the template: ARM/Bicep validation, what-if, and a permission check on the deployment principal. Do not proceed until all three are clean.
- Execute `./infra/workload-01/Deploy-Workload.ps1` (or whatever the script from Chore 4 is called). Capture the output; the deployment name and any module-level failures land here.
- Re-run the script immediately after a successful deploy. The second run's what-if must be a **no-op** — that's the idempotency check.
- Verify the result against the Chore 3 design's promises:
  - Public surface = **only** the frontend container app FQDN and the container registry login server.
  - `nslookup <sqlserver>.database.windows.net` from inside the spoke resolves to a **private IP** in `snet-private-endpoints`; from anywhere else it should resolve to the public CNAME but **refuse connections** (`publicNetworkAccess: Disabled`).
  - The two container apps exist with `minReplicas = 0`, the registry has `adminUserEnabled = false`, and both UAMIs hold `AcrPull` on the registry scope.
  - The SQL database is in **paused** state shortly after deploy (auto-pause delay = 60 min, but with no traffic it pauses on schedule). Cost on the resting subscription matches the estimate from Chore 3 §6.
- **Don't deploy the container images yet.** The container apps will spin up with their default placeholder image (the `mcr.microsoft.com/k8se/quickstart` ACA bootstrap image is typical) — that's expected and the next chore fixes it.

> **Safety note.** Even with preflight green, a real deploy is the first irreversible step in the workshop. If anything in the what-if output surprises you (resource deletions, role-assignment changes on resources you didn't expect to touch, edits to the hub resource group), stop and reconcile with the design before continuing.

## Chore 6 — Build the container images and roll them out

Scenario: the infrastructure is up but the container apps are still serving placeholder content. You're a **platform engineer, not a developer** — you don't know the .NET/React stack the workload team uses. That's fine: Copilot can read [workload-app/](workload-app/), produce the two `Dockerfile`s, and write the build/push/rollout script. You stay in your lane and review the output.

Goals:

- Two Dockerfiles live under [`dockerfiles/`](dockerfiles/) at the repo root, named exactly:
  - `dockerfiles/Dockerfile.backend` — multi-stage build of the .NET 10 minimal API in [workload-app/backend/HotelBooking.Api/](workload-app/backend/HotelBooking.Api/).
  - `dockerfiles/Dockerfile.frontend` — multi-stage build of the Vite/React SPA in [workload-app/frontend/](workload-app/frontend/), served by nginx. The nginx config does SPA fallback (`try_files $uri /index.html`) and reverse-proxies `/api/` to the backend container app's **internal** ingress FQDN, so the frontend stays the only public surface.
- Every `FROM` line uses **`mcr.microsoft.com`** — per [.github/copilot-instructions.md](.github/copilot-instructions.md) rule 5. No Docker Hub, no Quay, no GHCR base images.
- A PowerShell script — `dockerfiles/Build-And-Deploy.ps1` — does the rollout end-to-end:
  - Reads the ACR login server and the two container app names from the **Chore 5 deployment outputs** (or accepts them as parameters with sensible defaults). Do not hard-code resource names.
  - Authenticates against the registry with **`az acr login`** (Entra-based, no admin user — the registry has `adminUserEnabled = false`).
  - Builds both images with **`az acr build`** (server-side build, so the workshop participant doesn't need a local Docker daemon). Tags each image with both `:latest` and a short Git SHA (`git rev-parse --short HEAD`) for traceability.
  - Updates each container app to the new image tag with **`az containerapp update --image <acr>/<repo>:<sha>`**, which triggers a new ACA **revision**. The script waits for the new revision to report `Healthy` before moving on.
  - Prints the public frontend FQDN at the end so the participant can curl it and see real hotel data.
- The script is **idempotent and re-runnable**: running it twice produces a second revision with the same digest, which ACA collapses into a no-op rollout.
- After the rollout, verify end-to-end:
  - `curl https://<frontend-fqdn>/` returns the SPA's `index.html`.
  - `curl https://<frontend-fqdn>/api/hotels` returns JSON from the backend (proves the nginx reverse-proxy and the internal ACA ingress are wired correctly).
  - A booking POST works (the backend touches Azure SQL through the private endpoint with its managed identity — end-to-end proof of Chore 3 §3.3 and §3.5).
  - Open Application Insights and confirm a **single distributed trace** spans browser → frontend → backend → Azure SQL, courtesy of the OpenTelemetry wiring the app team already shipped.

> **Note for the platform engineer.** You are explicitly **not** expected to debug `.cs` or `.tsx` files. If the image fails to build, your job is to read the build log, decide whether it's an infrastructure issue (missing base image tag, registry auth, network) or an application issue, and either fix the infra or hand it back to the app team. **Do not edit anything under [workload-app/](workload-app/)** — that rule from [.github/copilot-instructions.md](.github/copilot-instructions.md) still applies.

---

More chores will be added as the workshop grows. Each one follows the same pattern: a realistic platform-team goal, delivered as IaC, with Copilot doing the heavy lifting.
