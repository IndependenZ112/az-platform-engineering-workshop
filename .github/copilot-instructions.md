# Workshop repository — Copilot instructions

This repo is the **Azure Platform Engineering workshop**. It is intentionally split into two parts:

| Folder | Owner | Copilot scope |
| --- | --- | --- |
| `workload-app/` | The "application team" (out of scope for this workshop) | **Read-only.** Do not modify. |
| `mock-alz/` | Workshop hub / shared landing zone | Read-only reference. Modify only when the chore explicitly says so. |
| Everything else (`infra/`, `bicep/`, `.github/`, root `Dockerfile`s, READMEs, etc.) | The platform team (you + the workshop participant) | Editable. This is where the chores happen. |

## Non-negotiable rules

1. **Never edit anything under `workload-app/`.** Not the `.cs`, `.csproj`, `.tsx`, `.ts`, `package.json`, `appsettings.json`, or any other file. The application is treated as a finished, immutable artifact the platform team has been handed. If a chore seems to require changing app code, stop and ask the user first.
2. **Reading `workload-app/` is encouraged.** Read the code to understand what the app needs (runtime, ports, env vars, database, egress, etc.) so the platform design is grounded in reality.
3. **Dockerfiles for the app are allowed**, because containerising is part of the platform work, not the app code. Place them as:
   - `workload-app/backend/HotelBooking.Api/Dockerfile` (for the .NET 10 API)
   - `workload-app/frontend/Dockerfile` (for the Vite/React frontend)
   Do not change any source file alongside them. If the build needs an extra file (e.g. `.dockerignore`), that is fine — but no edits to existing app sources.
4. **No application-level instructions.** Do not propose or follow guidance about C#, .NET, EF Core, React, Vite, Tailwind, TypeScript, or test frameworks. Those are the application team's concern.
5. **Container base images must come from `mcr.microsoft.com` (Microsoft Container Registry), not Docker Hub.** This applies to every `FROM` line in any `Dockerfile` you author in this repo (platform-team images and the workload `Dockerfile`s allowed by rule 3). Prefer the first-party Microsoft image; if no MCR equivalent exists for a base image you need (e.g. a community runtime), stop and ask the user before falling back to Docker Hub. Examples: use `mcr.microsoft.com/dotnet/sdk:<tag>` and `mcr.microsoft.com/dotnet/aspnet:<tag>` for .NET, `mcr.microsoft.com/azurelinux/base/nodejs:<tag>` for Node, `mcr.microsoft.com/azurelinux/base/nginx:<tag>` for nginx, `mcr.microsoft.com/cbl-mariner/base/python:<tag>` (or the Azure Linux equivalent) for Python.

## What this workshop is about

Hub-and-spoke Azure Landing Zone with **distributed Private DNS zones** (each workload resource group owns its own Private DNS zones, linked back to the hub vnet). Two chores:

- **Chore 1 — Spoke onboarding:** stand up a new spoke (vnet, peering to hub, route tables, Private DNS zones) using Bicep + AVM.
- **Chore 2 — Workload investigation & design:** read `workload-app/`, then produce a Markdown design document that includes an embedded architecture diagram (use the `drawio` skill — export PNG with embedded XML). Requirements: Well-Architected, scale-to-zero where possible, **private endpoints for all services except the container registry and the public-facing frontend**. The plan is the deliverable; no infra code is required for this chore.

## How to work in this repo

- Prefer **Azure Verified Modules (AVM)** for any Bicep — see [.github/instructions/azure-verified-modules-bicep.instructions.md](.github/instructions/azure-verified-modules-bicep.instructions.md).
- Follow **Microsoft CAF naming** — see [.github/instructions/azure-naming.instructions.md](.github/instructions/azure-naming.instructions.md).
- Architecture diagrams referenced from Markdown must be authored via the **`drawio` skill** under [.github/skills/drawio/SKILL.md](.github/skills/drawio/SKILL.md) and exported as PNG (with embedded `.drawio` XML) so the source stays editable.
- Use the **`azure-principal-architect`** or **`plan`** chat modes for design conversations, and **`bicep-plan` / `bicep-implement`** for IaC work.
- Run **`azure-deployment-preflight`** (Bicep what-if + permission checks) before any `az deployment` command.

## When in doubt

Ask the user before:
- Editing anything inside `workload-app/` (always — see rule 1).
- Adding new top-level folders.
- Introducing a new Azure service that wasn't already on the table for the current chore.
- Running anything that touches a real Azure subscription (deployments, role assignments, resource creation).
