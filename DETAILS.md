# Details — hints, outcomes, and background

Open this when you're stuck on a chore, want hints, want to verify what success looks like, or want the background on **why** the chore exists.

Chore headings here match [CHORES.md](CHORES.md) one-for-one.

---

## Workshop background

In a real environment, the baseline for this workshop would be a full [Azure Landing Zone](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/landing-zone/) deployment — either the [Enterprise-scale](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/enterprise-scale/) Accelerator (ALZ), the [SMB landing zone](https://learn.microsoft.com/azure/cloud-adoption-framework/scenarios/small-medium-enterprise/), or the **SMB Ready Foundation**. These accelerators provision a full management-group hierarchy, policy baseline, identity, connectivity, and management subscriptions.

Standing all of that up takes time we don't have. Instead, we **simulate** the centralized "platform" side of a landing zone by deploying a minimal **hub VNet** into a single subscription. The hub stands in for the connectivity subscription, so the workload-onboarding patterns we practice (peering, private endpoints, Private DNS) are the same ones you'd use in production.

Design choices locked in up front:

- **Single subscription.** All resources — platform and workload — live in one subscription. In a real ALZ they'd be split across a connectivity subscription and one or more landing-zone subscriptions.
- **Distributed Private DNS.** Private DNS zones for Private Link are deployed and linked **per workload**, not centrally in the hub. This is the [distributed Private DNS pattern](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/azure-best-practices/private-link-and-dns-integration-at-scale) — simpler for a workshop and a valid ALZ option.
- **Hub-and-spoke topology.** Workload VNets are spokes that [peer](https://learn.microsoft.com/azure/virtual-network/virtual-network-peering-overview) to the hub.

### What the prep step deploys into `rg-platform`

A **hub VNet** (`vnet-hub`, `192.168.100.0/24`) with placeholder subnets — no gateway or firewall is actually deployed, the subnets exist so the topology is realistic:

| Subnet                 | Range                 | Purpose                              |
| ---------------------- | --------------------- | ------------------------------------ |
| `AzureFirewallSubnet`  | `192.168.100.0/26`    | Reserved for Azure Firewall (`/26` minimum) |
| `GatewaySubnet`        | `192.168.100.64/27`   | Reserved for VPN / ExpressRoute gateway |
| `snet-shared-services` | `192.168.100.96/27`   | Shared platform services (DNS forwarders, jumpbox, etc.) |

`192.168.100.128/25` is left free for growth. No central Private DNS zones — each workload owns its own under the distributed pattern.

---

## Chore 1 — Curate the Copilot toolbox

### Background

The community maintains a large catalogue of reusable Copilot customizations at [aka.ms/awesome-copilot](https://aka.ms/awesome-copilot) (the [`github/awesome-copilot`](https://github.com/github/awesome-copilot) repo) — **agents**, **instructions**, and **skills**. Picking the right ones up front means Copilot follows your conventions (CAF naming, AVM, Bicep best practices, draw.io diagram authoring, deployment preflight, container best practices) without you having to re-prompt them every chore.

### Hints

- Browse the [`agents/`](https://github.com/github/awesome-copilot/tree/main/agents), [`instructions/`](https://github.com/github/awesome-copilot/tree/main/instructions), and [`skills/`](https://github.com/github/awesome-copilot/tree/main/skills) folders — or use the [machine-readable index](https://awesome-copilot.github.com/llms.txt) — and let Copilot match items to the chores ahead.
- Install layout:
  - **Instructions** → `.github/instructions/*.instructions.md` (use `applyTo` glob front-matter).
  - **Skills** → `.github/skills/<skill-name>/SKILL.md` (plus any bundled assets).
  - **Agents** → `.github/agents/<agent-name>.agent.md`.
- After installing, **Developer: Reload Window** (`Ctrl+Shift+P`). New customizations are only discovered on window load.
- Smoke test: ask Copilot for a Bicep resource name (should cite naming instructions); ask for a Dockerfile (should pull from `mcr.microsoft.com`).

### Outcome

The repo has a curated set of agents/instructions/skills under `.github/`, and Copilot now follows the workshop's conventions without re-prompting. Every subsequent chore assumes these are loaded.

### Why this chore exists

The exact set drifts as the community publishes new items. Treat this chore as recurring — re-running it every few months keeps the repo's Copilot configuration current.

---

## Chore 2 — Onboard a workload spoke

### Background

A new application team needs network space. As the platform team, you add a **spoke VNet** that hosts their workload and connect it to the hub.

### Hints

- Spoke VNet needs at least a subnet for **private endpoints** (with `privateEndpointNetworkPolicies` configured appropriately) and room to grow for app/data subnets.
- For a hub without a gateway: `allowGatewayTransit` / `useRemoteGateways` stay **off**; `allowVirtualNetworkAccess` **on**; `allowForwardedTraffic` typically **on**.
- Peering must be created on **both** sides — hub→spoke and spoke→hub.

### Outcome

- Resource group `rg-workload-01` exists.
- Spoke VNet deployed in a non-overlapping range.
- `az network vnet peering list` shows `Connected` on both sides.
- A resource in the spoke can reach the hub address space.

---

## Chore 3 — Investigate the workload and design its infrastructure

### Background

The application team has dropped a workload in [workload-app/](workload-app/) — a .NET 10 Minimal API backend and a React/Vite frontend, with data persisted in SQL Server. Before deploying anything, the platform team **understands the app** and **designs the Azure footprint** that will host it. This chore is design only — the output is a plan, not Bicep.

### Hints

The design document should answer:

- What the backend is (runtime, framework, exposed endpoints, dependencies).
- What the frontend is (build tooling, how it talks to the backend, how it's served in production).
- What data store it expects, what authentication it uses, what config it reads at startup.
- Which compute service hosts the containers, and why it fits this workload.
- Where container images live and how that registry is exposed.
- How the workload reaches its data tier privately, reusing the spoke and the distributed Private DNS pattern.
- How identity flows end-to-end — which managed identities exist, what they're allowed to do.
- Which subnets the workload needs in the spoke; whether the Chore 2 address plan still fits.
- Inbound exposure: how users reach the frontend.
- Trade-offs against the [Well-Architected Framework](https://learn.microsoft.com/azure/well-architected/) pillars — Reliability, Security, Cost, Operations, Performance.

**Private endpoints everywhere**, with two explicit exceptions:

- The **container registry** stays publicly reachable (so the workshop's build/push works without a private build agent).
- The **frontend app** stays publicly reachable (it's the user-facing entry point).

### Outcome

A short, opinionated design document in `docs/` plus a draw.io diagram, signed off and ready for Chore 4 to implement without re-litigating decisions.

### Why this chore exists

If you skip straight to Bicep, you end up redesigning halfway through writing it. Writing the design first turns the Bicep chore into pure execution.

---

## Chore 4 — Implement the workload infrastructure in Bicep

### Background

The design from Chore 3 is signed off. Turn it into **deployable Bicep** that lands the workload in the spoke from Chore 2 — without re-opening architectural decisions.

### Hints

- Bicep under `infra/workload-01/`. Use AVM wherever a module exists; raw `Microsoft.*` only with an inline comment explaining the gap.
- CAF naming — see [.github/instructions/azure-naming.instructions.md](.github/instructions/azure-naming.instructions.md).
- If you discover a design gap while writing Bicep, **fix it in the design doc and diagram first**, then change the template.
- Identity wiring:
  - UAMI per container app, with `AcrPull` on the registry.
  - Backend UAMI granted access to the data tier as the design prescribes.
  - **No secrets** — construct connection strings from resource properties at deploy time; auth is MI-based.
- Distributed Private DNS: every zone created in the workload RG, linked to the spoke (registration) and the hub (resolution).
- Parameterise location, naming tokens, address space, SKU sizes — with sensible defaults.
- `Deploy-Workload.ps1` mirrors [mock-alz/Deploy-Hub.ps1](mock-alz/Deploy-Hub.ps1). Run `azure-deployment-preflight` (what-if + permission checks) before every `az deployment group create`.
- Post-deploy steps that can't live in Bicep (e.g. contained DB users for UAMIs) are scripted alongside the template — not done by hand.

### Outcome

- `infra/workload-01/` deploys cleanly end-to-end.
- Second run of the script produces a no-op what-if (idempotent).
- Public surface = frontend FQDN + container registry. Everything else private.
- Resting cost matches the scale-to-zero estimate from Chore 3.

### Hint — workflow

Use the **`bicep-plan`** chat mode to draft file layout and module choices, then switch to **`bicep-implement`** to write the resources. Keep the Chore 3 design open in the chat context.

---

## Chore 5 — Deploy the workload infrastructure

### Background

The Bicep from Chore 4 is reviewed. Time to actually land it in the subscription.

### Hints

- Confirm `az account show` points at the **same subscription** as the hub and the Chore 2 spoke.
- Run **`azure-deployment-preflight`** end-to-end: validate, what-if, permission check. Do not proceed until all three are clean.
- Execute `./infra/workload-01/Deploy-Workload.ps1`.
- Re-run immediately — second run's what-if must be a **no-op**.
- Verifications:
  - Public surface = frontend FQDN + ACR login server only.
  - `nslookup <sqlserver>.database.windows.net` from inside the spoke returns a **private IP** in `snet-private-endpoints`; from elsewhere it resolves but **refuses connections** (`publicNetworkAccess: Disabled`).
  - Container apps exist with `minReplicas = 0`, registry has `adminUserEnabled = false`, both UAMIs hold `AcrPull` on the registry scope.
  - SQL database in **paused** state shortly after deploy (auto-pause = 60 min).
- **Don't deploy container images yet** — container apps will spin up with the placeholder `mcr.microsoft.com/k8se/quickstart` image. That's expected; Chore 6 fixes it.

### Outcome

Workload infrastructure exists in the subscription, fully private (except registry and frontend), idempotent, with a resting cost that matches the design.

### Safety note

This is the first irreversible step. If anything in the what-if surprises you (resource deletions, role-assignment changes on resources you didn't expect to touch, edits to the hub RG), stop and reconcile with the design.

---

## Chore 6 — Build the container images and roll them out

### Background

Infrastructure is up but the container apps are still serving placeholder content. You're a **platform engineer, not a developer** — you don't know the .NET/React stack. Copilot reads [workload-app/](workload-app/), produces two Dockerfiles, and writes the rollout script. You stay in your lane and review.

### Hints

- Backend: multi-stage build of the .NET 10 minimal API in [workload-app/backend/HotelBooking.Api/](workload-app/backend/HotelBooking.Api/).
- Frontend: multi-stage build of the Vite/React SPA in [workload-app/frontend/](workload-app/frontend/), served by nginx. nginx config does SPA fallback (`try_files $uri /index.html`) and reverse-proxies `/api/` to the backend container app's **internal** ingress FQDN, keeping the frontend the only public surface.
- Every `FROM` line uses **`mcr.microsoft.com`** — per [.github/copilot-instructions.md](.github/copilot-instructions.md) rule 5.
- `dockerfiles/Build-And-Deploy.ps1`:
  - Reads ACR login server and container app names from Chore 5 outputs (or accepts them as parameters). Do **not** hard-code names.
  - `az acr login` (Entra-based, no admin user).
  - `az acr build` for both images (server-side build — no local Docker needed). Tag with `:latest` and `git rev-parse --short HEAD`.
  - `az containerapp update --image <acr>/<repo>:<sha>` triggers a new ACA revision. Wait for `Healthy` before moving on.
  - Print the public frontend FQDN at the end.
- Script must be **idempotent**.

### Outcome

- `curl https://<frontend-fqdn>/` returns the SPA's `index.html`.
- `curl https://<frontend-fqdn>/api/hotels` returns JSON.
- A booking POST works (backend reaches SQL via private endpoint with its UAMI).
- Application Insights shows a single distributed trace browser → frontend → backend → SQL.

### Note for the platform engineer

You are **not** expected to debug `.cs` or `.tsx` files. If the image fails to build, decide whether it's infra (base image tag, registry auth, network) or application (hand it back to the app team). **Do not edit anything under [workload-app/](workload-app/)** — that rule still applies.

---

## Chore 7 — Add a production environment alongside test

### Background

The application team needs a real **production** environment alongside test. Prod must run **at least 3 replicas spread across availability zones**, no scale-to-zero. Both environments coexist in the same subscription.

### Hints

- Single `environmentName` parameter with allowed values `test` / `prod`, or two `*.bicepparam` files (`workload.test.bicepparam`, `workload.prod.bicepparam`). **No code fork** — same template produces both.
- Each environment in its own RG (`rg-workload-01-test`, `rg-workload-01-prod`) with **non-overlapping spoke address spaces**. Both spokes peer to the same hub.
- CAF naming carries the environment token (`ca-hotelapi-test-weu-001` vs `ca-hotelapi-prod-weu-001`).
- ACA env **`zoneRedundant: true`** is set at environment creation time and cannot be flipped later — so prod gets its **own** ACA environment.
- SQL: prod uses a SKU that supports **zone redundancy** (e.g. General Purpose serverless with `zoneRedundant: true`, or provisioned Business Critical) and does **not** auto-pause.
- `Deploy-Workload.ps1` takes `-Environment test|prod` and selects the matching RG / parameter file / address space. Preflight runs before every deploy.

### Outcome

- Deploying test then prod (or prod then test) leaves both workloads healthy on their own frontend FQDNs.
- Each environment's what-if shows only the resources for that environment.
- Design doc + diagram are updated to show both environments and the zone-spread prod topology.

### Reliability note

Zone redundancy on Container Apps and Azure SQL is **only available in regions with 3 availability zones**. Confirm your region qualifies (e.g. `westeurope`, `northeurope`, `eastus2`). If not, either move prod to one that does or call out the limitation in the updated design.

---

## Chore 8 — Rebuild the deployment as two fully isolated environments

### Background

Two things came up after Chore 7: (1) the **single spoke from Chore 2 is no longer fit for purpose** — test and prod must not share a VNet; (2) the **existing deployment carries baggage** from before the split (resources named without an environment token, the original `rg-workload-01`, the shared spoke). Tear it down and rebuild cleanly.

### Hints

Teardown order (with what-if previews before each destructive step):

1. Container apps and their environment.
2. SQL server, ACR, Key Vault, other workload PaaS.
3. **Spoke VNet from Chore 2** *and* the hub-side peerings (orphaned peerings cause confusing what-if output later).
4. The `rg-workload-01` resource group itself.

`mock-alz/` and `rg-platform` are **not** touched.

Two new spokes:

- `vnet-spoke-workload01-test-<region>-001` in `rg-workload-01-test`.
- `vnet-spoke-workload01-prod-<region>-001` in `rg-workload-01-prod`.
- Non-overlapping address spaces. Suggested: test = `10.10.0.0/22`, prod = `10.20.0.0/22`. Write the convention into the Chore 3 design doc **before** the Bicep.
- Each spoke carries the same subnet layout as the original (private endpoints subnet, ACA infrastructure subnet sized per Chore 7's zone-redundant requirements, room to grow).
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

---

## Chore 9 — Make sure you have a usable GitHub account

### Background

Chores from here use GitHub for collaboration (repos, branches, PRs, Actions). You have two options:

- **Option A — personal account on [github.com](https://github.com/).** Recommended if you don't have one — it follows you between jobs.
- **Option B — your corporate GHEC account**, *provided* your org allows you to create new repos. If it locks repo creation down to admins, fall back to Option A.

### Hints

- Quick check: open [github.com/new](https://github.com/new) and confirm the form loads with at least one owner in the **Owner** dropdown where you have create rights. Don't actually create the repo yet — that's Chore 10.
- Option A signup: [github.com/signup](https://github.com/signup), use a **personal** email, pick the **Free** plan.
- Enable **2FA** at [github.com/settings/security](https://github.com/settings/security) using an authenticator app or hardware key. Save recovery codes.
- Configure commit identity:

  ```powershell
  git config --global user.name  "<your name>"
  git config --global user.email "<the email or the noreply GitHub address>"
  ```

  Prefer GitHub's [noreply email](https://docs.github.com/account-and-profile/setting-up-and-managing-your-personal-account-on-github/managing-email-preferences/setting-your-commit-email-address) (`<id>+<username>@users.noreply.github.com`).
- Verify which account VS Code / `gh` is using — not a second account that happens to be active in the same browser. `gh auth status`.

### Outcome

You can sign in to GitHub as the intended account, create a new repo, and your commits will be attributed correctly with 2FA in place.

### Workplace tip

If you keep both a personal account and a corporate GHEC identity on the same laptop, they live alongside each other — they're separate logins, not a merge. Use a browser profile or VS Code profile to keep them apart.

---

## Chore 10 — Publish your work to your own GitHub repo

### Background

So far you've been working in a local clone of [azureholic/az-platform-engineering-workshop](https://github.com/azureholic/az-platform-engineering-workshop) with no write access. Time to take ownership: create a repo under the account from Chore 9, swap `origin` over, push everything. Clean break — no `upstream`, no fork relationship.

### Hints

Review before publishing:

```powershell
git status
git diff --stat origin/main
git log --oneline origin/main..HEAD
```

Anything surprising (large generated files, secrets, `.bicepparam` with real subscription IDs, local-only paths) gets fixed or `.gitignore`d **before** the first push.

If your work is one giant uncommitted blob, split it into a handful of logical commits (e.g. `chore-1 toolbox`, `chore-2 spoke`, `chore-4 workload infra`). Conventional Commits is fine but not required.

Create the empty remote — **do not** initialise with README/.gitignore/license:

```powershell
# Option A — GitHub CLI
gh repo create <your-handle-or-org>/az-platform-engineering-workshop `
    --private `
    --description "My run through the Azure Platform Engineering workshop" `
    --disable-wiki

# Option B — https://github.com/new (Owner + Name + Private, no init files)
```

Swap `origin`:

```powershell
git remote set-url origin https://github.com/<your-handle-or-org>/az-platform-engineering-workshop.git
git remote -v
git push -u origin main
```

Verify the round-trip in the browser: commit graph matches `git log --oneline`, last commit's author is the Chore 9 identity, no secrets or local-only artifacts (`*.tfstate`, `*.pem`, `bin/`, `obj/`). If anything leaked, scrub with [`git filter-repo`](https://github.com/newren/git-filter-repo) and force-push **once** before anyone clones the repo.

### Outcome

Your repo is on GitHub under an account you control, with clean history and clean commit identity. Future `git push` / `git pull` are one-liners.

### Heads up

Without `upstream`, you won't see new chores added to [azureholic/az-platform-engineering-workshop](https://github.com/azureholic/az-platform-engineering-workshop). Add it on demand: `git remote add upstream https://github.com/azureholic/az-platform-engineering-workshop.git` and cherry-pick what you need.

---

## Chore 11 — Staged infra deploy workflow

### Background

Every change to the Bicep under `infra/` should flow through CI, not be deployed from a laptop. The platform team wants a **staged pipeline**: lint, then auto-deploy to **test**, then wait for a human to approve **prod**. Same shape a real landing zone uses — test as the safety net, prod gated by a reviewer.

### Hints

Workflow shape:

| Stage | Job          | Runs on             | Purpose |
| ----- | ------------ | ------------------- | ------- |
| 1     | `lint`       | every trigger       | `az bicep build` + `az bicep lint` against `infra/**`. No Azure login. |
| 2     | `deploy-test`| `needs: lint`, env `test` | OIDC login, `az deployment group what-if`, then `az deployment group create` against `rg-workload-01-test`. Auto-approved. |
| 3     | `deploy-prod`| `needs: deploy-test`, env `prod` | Same shape but `rg-workload-01-prod`. **Blocks on required reviewer.** |

OIDC details:

- `azure/login@v2` with `client-id` / `tenant-id` / `subscription-id`, `permissions: id-token: write`.
- **No long-lived secrets** in repo or org secrets.
- Two separate Entra app registrations — one per environment — each federated to the matching GitHub environment.
- Federated credential subject for test: `repo:<owner>/<repo>:environment:test`. Same shape for prod.

GitHub Environments do the gating, not workflow logic:

- `test`: no protection rules. Variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP=rg-workload-01-test`.
- `prod`: **required reviewers** (at least one human), optional wait timer, deployment-branch policy restricted to `main`. Same four variables pointing at the prod app reg and `rg-workload-01-prod`.

Two `bicepparam` files (`main.test.bicepparam`, `main.prod.bicepparam`) are the **only** thing that differs between stages. Template is identical.

Every deploy job runs `what-if` first and writes the output to `$GITHUB_STEP_SUMMARY` so the prod reviewer sees what they're approving.

### Outcome

First end-to-end run: test deploys without prompting; prod sits in **Waiting** on the Actions tab until you approve. After approval, the same commit's prod deploy uses the exact templates and params verified in test — no drift.

### Workshop scope note

You only have one subscription, so test and prod are different **resource groups** in the same subscription. The federated credentials and the workflow are still split per environment so the muscle memory matches a real multi-subscription landing zone — when you later have separate test and prod subscriptions, only `AZURE_SUBSCRIPTION_ID` changes.

---

## Chore 12 — Build-once, promote-everywhere app workflow

### Background

Chore 11 handles infra; this handles the application. The rule is simple: **build each container image exactly once, then deploy that same image — same tag, same digest, byte-for-byte — to test and then to prod.** Prod must never see a freshly-rebuilt image, because a rebuild is a new artifact that hasn't been verified anywhere.

### Hints

Four-job workflow, all chained with `needs:`:

| Stage | Job           | Env  | Purpose |
| ----- | ------------- | ---- | ------- |
| 1     | `build`       | `test` (for ACR push perms) | OIDC login, `az acr login`, `az acr build` for both Dockerfiles. Tag each image `:${{ github.sha }}`. **Emit the `@sha256:...` digests as job outputs** (`backend-digest`, `frontend-digest`). |
| 2     | `deploy-test` | `test` | `az containerapp update --image <registry>/<repo>@${{ needs.build.outputs.backend-digest }}`. Pin to **digest**, not tag. Wait for the new revision to be healthy. Auto-approved. |
| 3     | `smoke-test`  | `test` | `curl --fail` the test frontend FQDN and a couple of `/api/` health checks. If smoke fails, prod is never offered the image. |
| 4     | `deploy-prod` | `prod` | **Blocks on required reviewer.** Same `az containerapp update --image ...@<digest>` against prod. **No build. No `az acr build`. No re-tag.** Same digest that passed test. |

Other essentials:

- **Reuse the per-env OIDC app registrations from Chore 11.**
- Build is gated by `if: github.event.inputs.image_tag == ''` (or equivalent) so a `workflow_dispatch` with an explicit `image_tag` **skips the build entirely** and goes straight to redeploy. Covers two scenarios with one mechanism: redeploying yesterday's good image after a bad one, and rolling prod to an older tag while you investigate.
- Each deploy job writes `backend → <registry>/<repo>@<digest>` and `frontend → <registry>/<repo>@<digest>` to `$GITHUB_STEP_SUMMARY` so the prod reviewer can eyeball it.
- A multi-subscription setup would add `az acr import` between test and prod registries to copy the **same digest**. You still wouldn't rebuild.

### Outcome

A diff of `az containerapp show` outputs across test and prod shows identical `image` fields ending in `@sha256:<same-digest>`. That's the contract.

### Why build once

Every rebuild creates a new artifact: base-image patches, transient package mirrors, build-time clocks and ARGs all change the bytes. If prod has its own `build` job, prod is running an artifact that **no one has ever tested**. Build once, deploy the same image everywhere, and "it worked in test" actually means something in prod.

### Why two pipelines, not one

Infra (Chore 11) and app (this chore) move at different speeds and have different blast radius. Separate workflows mean a typo in Bicep can't accidentally redeploy the app, and a hotfix container build doesn't drag a half-finished infra change along. Both pipelines share the same OIDC app registrations and the same `test` / `prod` environments, so the security model stays consistent.

---

## Chore 13 — Commit and push the workflows

### Background

Chores 11 and 12 produced two workflow files (`.github/workflows/infra-deploy.yml`, `.github/workflows/app-deploy.yml`) sitting as **uncommitted local changes**. They can't run until they're on `main` of your remote — GitHub Actions reads workflows from the repo, not your laptop.

### Hints

Inspect before committing:

```powershell
git status
git diff -- .github/workflows/
git diff -- README.md
```

Only workflow YAMLs and related docs land. Nothing under `workload-app/`. No `*.bicepparam` with real subscription IDs. No local-only test files.

Commit cleanly — one commit per workflow:

```powershell
git add .github/workflows/infra-deploy.yml
git commit -m "ci: add staged infra deploy workflow (chore 11)"

git add .github/workflows/app-deploy.yml
git commit -m "ci: add build-once app deploy workflow (chore 12)"

git add README.md docs/
git commit -m "docs: document chores 11-13"

git push origin main
```

If you've been on a feature branch, **open a PR and merge to `main`** — the `paths:` filters only fire on `push` to `main`.

Sanity-check **Settings → Environments**: both `test` and `prod` exist with federated credentials, secrets/variables, and (for `prod`) required reviewers. If they're missing, the next run fails with `Error: No subscription found` or `Error: environment 'prod' not configured`.

### Outcome

```text
On branch main
Your branch is up to date with 'origin/main'.

nothing to commit, working tree clean
```

Both `infra-deploy` and `app-deploy` appear on the Actions tab with status `active` and can be dispatched manually.

### Why this is its own chore

"Add a workflow file" and "make the workflow runnable" are not the same thing. A workflow that only exists on your laptop is just YAML — it doesn't gate anything, deploy anything, or show up on the Actions tab. This chore is the bridge: it turns the artifacts from Chores 11 and 12 into actual CI/CD.

---

## Chore 14 — Prove the infra pipeline by retagging the workload

### Background

The infra pipeline from Chore 11 is wired up but you haven't seen it react to a real change. The cheapest, safest proof is a **trivial, observable** edit — change or add a `tags` value. Tags don't move resources, don't restart anything, and `what-if` makes the diff easy to spot.

### Hints

Good candidates:

- Bump an existing `CostCenter` / `Owner` / `Environment` value.
- Add a new tag (`tags: { ManagedBy: 'platform-team', ChangeTicket: '<id>' }`) via the standard AVM `tags` parameter on the RG / resources.
- If your Bicep hoists a single `tags` object at the top of `main.bicep` and fans it out via `tags: tags` on every module, change it in **one** place.

Run preflight locally first so you don't burn a CI run on a typo:

```powershell
az bicep build --file infra/workload-01/main.bicep
az deployment group what-if `
    --resource-group rg-workload-01-test `
    --template-file infra/workload-01/main.bicep `
    --parameters infra/workload-01/main.test.bicepparam
```

Confirm the only changes are tag deltas. If anything else lights up, stop and figure out why before pushing.

```powershell
git add infra/workload-01/
git commit -m "infra(workload-01): add ManagedBy tag to prove CI pipeline"
git push origin main
```

If the workflow does **not** trigger:

1. The commit only touched files outside the `paths:` filter (`git show --stat HEAD`).
2. You pushed to a branch, not `main` (`git log origin/main -1`).
3. The workflow file didn't reach `main` in Chore 13 — confirm on github.com.

### Outcome

- New run on the Actions tab tied to your commit SHA.
- `lint` green; `deploy-test` runs `what-if` against `rg-workload-01-test` and the job summary shows **only the tag change**.
- `deploy-test` finishes green; tag updated on `rg-workload-01-test` (`az group show -n rg-workload-01-test --query tags`).
- `deploy-prod` is in **Waiting**. Approve. Same shape against `rg-workload-01-prod`. Tag matches across both.

### Why a tag

A tag change is the smallest possible "real" infra edit: it exercises lint → OIDC login → what-if → deploy → environment approval, but the underlying resources don't change state. If you ever need to prove the pipeline still works after a credential rotation, workflow refactor, or quiet period, do this chore again.

---

## Chore 15 — Prove the app pipeline by rebranding the frontend

### Background

Chore 14 proved the **infra** pipeline. This proves the **app** pipeline — make the smallest possible, visually obvious edit to the frontend, push it, watch `app-deploy` build a new image, deploy to test, smoke test, wait for prod approval, roll out to prod. When you load the site in the browser, your change is staring back at you.

> **The one chore that touches `workload-app/`.** The standing rule is the platform team doesn't edit application code. This is a deliberate, single-line exception so you see an end-to-end app deploy with your own eyes. Treat it as a one-time stunt: change the title, ship it, move on. **Do not start adding features to the app.**

### Hints

Edit one line in [workload-app/frontend/index.html](workload-app/frontend/index.html) — change the `<title>` from `StayBright Hotels` to something with your name/handle/team in it (e.g. `Azureholic Hotels`). Nothing else in the file. Nothing else in `workload-app/`.

```powershell
git add workload-app/frontend/index.html
git commit -m "app(frontend): rebrand title to Azureholic Hotels"
git push origin main
```

`infra-deploy` does **not** run (its filter is `infra/**`) — exactly the separation Chore 12 was built for.

Watch the run (`gh run watch` or the web UI):

1. **`build`** — builds both images, pushes to ACR, emits digests. Note the frontend digest.
2. **`deploy-test`** — `az containerapp update --image <registry>/frontend@<digest>` against test. No rebuild. New revision points at the new digest.
3. **`smoke-test`** — hits the public test URL for HTTP 200.
4. **`deploy-prod`** — sits in **Waiting** for the reviewer. Approve. `az containerapp update` against prod with the **same digest**. No build, no `az acr build`, no re-tag.

Verify byte-for-byte promotion:

```powershell
az containerapp show -g rg-workload-01-test -n ca-hotelweb-test-weu-001 --query "properties.template.containers[0].image"
az containerapp show -g rg-workload-01-prod -n ca-hotelweb-prod-weu-001 --query "properties.template.containers[0].image"
```

Both must end in `@sha256:<same-digest>`. If not, something rebuilt between test and prod — re-read Chore 12.

If the workflow does **not** trigger or fails:

1. Commit only touched files outside `workload-app/**` (`git show --stat HEAD`).
2. `build` failed because the frontend `Dockerfile` `FROM` line points at Docker Hub instead of `mcr.microsoft.com` — fix per the repo's container-image rule.
3. `deploy-test` failed at OIDC — same federated-credential subject check as Chore 14, but for the `test` environment.
4. Smoke test fails because the new revision isn't taking traffic yet — Container Apps takes a few seconds to swap revisions; add a short retry loop if the workflow doesn't already.

### Outcome

Test and prod URLs both show the new title in the browser tab. `az containerapp show` returns the same `@sha256:` digest for both. You've seen, with your own eyes, the same image artifact you built once flow through test → smoke → prod.

### Why a title change

The `<title>` tag is the cheapest possible "real" app edit: one byte of meaningful change, visible to a human without logging into anything, and it forces the full build-once / promote-everywhere pipeline to run end-to-end. That's the whole reason Chore 12 exists.
