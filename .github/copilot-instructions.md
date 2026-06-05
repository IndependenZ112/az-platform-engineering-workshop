# Workshop repository — Copilot instructions

This repo is the **Azure Platform Engineering workshop**. It is intentionally split into two parts:

| Folder | Owner | Copilot scope |
| --- | --- | --- |
| `workload-app/` — application source | The "application team" (out of scope for this workshop) | **Application source is read-only.** No edits to `.cs`, `.csproj`, `.tsx`, `.ts`, `package.json`, `appsettings.json`, `index.html`, build configs, or any other code/config the app team owns. |
| `workload-app/<service>/` — containerization assets | Platform team | **Allowed.** You may create and edit `Dockerfile`, `.dockerignore`, nginx config (`nginx.conf`, `default.conf`), entrypoint scripts, and similar container build assets directly inside the matching service folder (e.g. `workload-app/backend/HotelBooking.Api/Dockerfile`, `workload-app/frontend/Dockerfile` + `workload-app/frontend/nginx/default.conf`). |
| `mock-alz/` | Workshop hub / shared landing zone | Read-only reference. Modify only when the chore explicitly says so. |
| Everything else (`infra/`, `bicep/`, `.github/`, READMEs, etc.) | The platform team (you + the workshop participant) | Editable. This is where the chores happen. |

## Non-negotiable rules

1. **Never modify application source code under `workload-app/`.** Not the `.cs`, `.csproj`, `.tsx`, `.ts`, `package.json`, `appsettings.json`, `index.html`, `vite.config.ts`, `tsconfig.json`, or anything else the app team owns. The application is treated as a finished, immutable artifact the platform team has been handed. If a chore appears to require a code change inside `workload-app/`, the participant does that edit by hand — Copilot does not. **Exception:** container build assets (see rule 3) — those are platform-team files and Copilot may create/edit them inside `workload-app/`.
2. **Reading `workload-app/` is encouraged.** Read the code to understand what the app needs (runtime, ports, env vars, database, egress, etc.) so the platform design is grounded in reality.
3. **Dockerfiles and supporting container assets live next to the service they build**, inside the matching `workload-app/<service>/` folder. Layout:
   - `workload-app/backend/HotelBooking.Api/Dockerfile` (for the .NET 10 API)
   - `workload-app/backend/HotelBooking.Api/.dockerignore`
   - `workload-app/frontend/Dockerfile` (for the Vite/React SPA, served by nginx)
   - `workload-app/frontend/.dockerignore`
   - `workload-app/frontend/nginx/default.conf` (or `nginx.conf`) — nginx config for the SPA, including SPA fallback (`try_files $uri /index.html`) and any reverse-proxy rules
   - Any other supporting assets (entrypoint scripts, etc.) live in the same service folder.
   Each Dockerfile's build context is **its own folder** (e.g. `az acr build --file workload-app/frontend/Dockerfile workload-app/frontend`). Application source files are still off-limits to edit — only the container build assets listed here are platform-team property.
   **Auxiliary config (nginx `nginx.conf` / `default.conf` / `entrypoint.sh`, etc.) must be authored as real files on disk and `COPY`-ed into the image.** Do not inline multi-line config via heredocs, `RUN echo`, or `printf` chains inside the Dockerfile — keep config reviewable, diff-able, and editable outside the build.
4. **No application-level instructions.** Do not propose or follow guidance about C#, .NET, EF Core, React, Vite, Tailwind, TypeScript, or test frameworks. Those are the application team's concern.
5. **Container base images must come from `mcr.microsoft.com` (Microsoft Container Registry), not Docker Hub.** This applies to **every** `FROM` line in **every** `Dockerfile` you author in this repo — backend, frontend (nginx), and any platform-team images. Prefer the first-party Microsoft image; if no MCR equivalent exists for a base image you need, stop and ask the user before falling back to Docker Hub. **Use the pinned tags below — do not invent your own, do not use `:latest`, do not float to a higher major version without checking with the user first** (these have been verified to exist on MCR and to be compatible with the workshop app):

   | Stage | Image |
   | --- | --- |
   | .NET 10 build (backend SDK stage) | `mcr.microsoft.com/dotnet/sdk:10.0` |
   | .NET 10 runtime (backend runtime stage) | `mcr.microsoft.com/dotnet/aspnet:10.0` |
   | Node build (frontend build stage) | `mcr.microsoft.com/azurelinux/base/nodejs:24` |
   | Nginx runtime (frontend runtime stage) | `mcr.microsoft.com/azurelinux/base/nginx:1.28` |

   Vite 7 (used by the frontend) requires Node ≥ 20.19, so `nodejs:24` is the right pick — the older Azure Linux `nodejs:20` tag is too old. If you need a Python base image, prefer `mcr.microsoft.com/azurelinux/base/python:<tag>` over the older `cbl-mariner` family.
6. **Stay inside the current chore. Read only the current chore page (`CHORES.md` is an index — open the specific `chores/chore-NN.md` you are working on, nothing later). Do not open any `chores/details-NN.md` file (they contain hints, expected outcomes, and spoilers the participant is working through on their own). Do not name, number, or reference future chores in any artifact (docs, diagrams, Bicep comments, scripts, commit messages, chat output). Do not pre-bake parameters, SKUs, file paths, or code branches for requirements that have not been stated in the current chore.** If the current chore says "the plan should be detailed enough that the next chore can implement it", that means write a self-contained design — *not* "Chore 4 will…". The participant drives chores in order; you do not get to peek. If something genuinely must be deferred, say **"out of scope for this design"** or **"a follow-up implementation"** without naming or numbering the future work. The only files where you may write `Chore N` are `CHORES.md`, `README.md`, and files under `chores/` — nowhere else.
7. The container registry **must** be publicly accessible for all networks (not behind a private endpoint) to make folow up chores possible. 

## What this workshop is about

Hub-and-spoke Azure Landing Zone with **distributed Private DNS zones** (each workload resource group owns its own Private DNS zones, linked back to the hub vnet). The participant works through a sequence of chores that cover spoke onboarding, workload design, Bicep implementation with AVM, deployment, container build/rollout, multi-environment support, and CI/CD with OIDC. See [CHORES.md](../CHORES.md) for the authoritative, ordered list of chores; each chore page links to its matching detail page under `chores/` for hints.

Architecture diagrams referenced from Markdown design docs must be authored via the **`drawio` skill** and exported as PNG with embedded XML so the source stays editable.

## How to work in this repo

- Prefer **Azure Verified Modules (AVM)** for any Bicep — see [.github/instructions/azure-verified-modules-bicep.instructions.md](.github/instructions/azure-verified-modules-bicep.instructions.md).
- Follow **Microsoft CAF naming** — see [.github/instructions/azure-naming.instructions.md](.github/instructions/azure-naming.instructions.md).
- Architecture diagrams referenced from Markdown must be authored via the **`drawio` skill** under [.github/skills/drawio/SKILL.md](.github/skills/drawio/SKILL.md) and exported as PNG (with embedded `.drawio` XML) so the source stays editable.
- Use the **`azure-principal-architect`** or **`plan`** chat modes for design conversations, and **`bicep-plan` / `bicep-implement`** for IaC work.
- Run **`azure-deployment-preflight`** (Bicep what-if + permission checks) before any `az deployment` command.

## When in doubt

Never change application source files inside `workload-app/` — the only thing Copilot may create or edit there is container build assets (`Dockerfile`, `.dockerignore`, nginx config, entrypoint scripts; see rule 3). For everything else under that folder, refuse and tell the user to do it by hand. Ask the user before:
- Adding new top-level folders.
- Introducing a new Azure service that wasn't already on the table for the current chore.
- Running anything that touches a real Azure subscription (deployments, role assignments, resource creation).

## other important instructions
- The container registry, the Monitor stack (Log Analytics, Application Insights, any DCE/DCR), and the public frontend are the **only** workload components that may have a public endpoint. Every other service must be locked down with private endpoints and/or service endpoints, and all inter-service communication must stay on the Microsoft backbone via Private DNS and peering. The full rules — including which `privatelink.*` zones are forbidden for ACR/Monitor and the exact AVM modules to watch — live in [.github/instructions/workload-network-exposure.instructions.md](instructions/workload-network-exposure.instructions.md) and are auto-applied to Bicep and design files.
- **ACR must be configured for "Allow All Networks"** (no `networkRuleSet`/`networkAcls` with `defaultAction: 'Deny'`, no `ipRules`, no `virtualNetworkRules`). `az acr build` runs on Microsoft-managed agents outside the workshop spoke and image pulls happen from multiple places during chores — any network restriction breaks the build/deploy flow.
- you must make sure the local az cli principal has RBAC permissions to deploy all the resources in the Bicep templates, including the ability to create role assignments for user-assigned managed identities. If any permission is missing, the deployment will fail. Use `az role assignment create` to grant the necessary permissions before deployment.
- the **Azure SQL Entra admin must be the workload's user-assigned managed identity**, not the local deploying principal. SQL is reachable only through its private endpoint from inside the spoke, so the deploying principal cannot (and should not) be the data-plane admin. Wire the MI as the Entra admin in Bicep and let the application authenticate passwordlessly.

## Self-check before you finish a turn

Before returning your final answer or saving any artifact, run **both** of these grep checks against your output and any files you wrote/edited. If either matches in a disallowed context, fix it before responding — do not ship the turn.

1. Regex `Chore \d` — if it matches **and** the hit is not inside `CHORES.md`, `README.md`, or a file under `chores/`, you have violated rule 6. Replace the reference with "a follow-up implementation" or "out of scope for this design".
2. Regex `privatelink\.(azurecr|monitor|oms\.opinsights|ods\.opinsights|agentsvc|applicationinsights)` **or** a `privateEndpoints` block / `publicNetworkAccess:\s*'Disabled'` attached to ACR, Log Analytics, Application Insights, a Data Collection Endpoint/Rule, or an AMPLS — if it matches, you have violated [workload-network-exposure.instructions.md](instructions/workload-network-exposure.instructions.md). Revert: those services stay public on purpose. Re-design before responding.