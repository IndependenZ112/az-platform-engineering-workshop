# Azure Platform Engineering Workshop

A hands-on workshop that walks through the day-to-day work of a platform team on Azure: extending a centralized network, onboarding a new workload, wiring up secure identity-based access, and automating deploys with GitHub Actions — all using Infrastructure as Code and GitHub Copilot.

## Goal

By the end of the workshop you will have, with GitHub Copilot doing the heavy lifting:

- Stood up a mock landing zone (hub VNet) and onboarded a workload **spoke** in a hub-and-spoke topology.
- Read an unfamiliar app, designed its Azure footprint, and implemented it as **Bicep using Azure Verified Modules**, with **private endpoints**, **managed identities**, and the **distributed Private DNS** pattern.
- Split the deployment into **test and prod environments** with isolated spokes and zone-redundant prod.
- Published the work to your own GitHub repo and wired up **two staged GitHub Actions pipelines** — one for infra, one for the app — using OIDC and environment-gated approvals.
- Proved both pipelines end-to-end with trivial, observable changes.

## Workshop structure

- [README.md](README.md) — this file. Goal and prerequisites.
- [CHORES.md](CHORES.md) — the chores. Short, requirements-only. Work through them in order with Copilot.
- [DETAILS.md](DETAILS.md) — hints, expected outcomes, and background per chore. Open this when you get stuck or want to check your work.

## Prerequisites

### Azure

- An **Azure subscription** with **Owner** permissions.
- Signed in via Azure CLI with the target subscription selected:

  ```powershell
  az login
  az account set --subscription <subscription-id>
  ```

### Tooling

- **[Visual Studio Code](https://code.visualstudio.com/)**.
- A **GitHub Copilot** license, with the Azure and Bicep MCP servers enabled.
- **[PowerShell 7+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)** (deployment scripts use `#requires -Version 7.0`).
- **[Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)**.
- **[GitHub CLI (`gh`)](https://cli.github.com/)**:

  ```powershell
  winget install --id GitHub.cli -e
  ```

  macOS: `brew install gh`. Linux: see the [official instructions](https://github.com/cli/cli/blob/trunk/docs/install_linux.md).
- **[draw.io Desktop](https://www.drawio.com/)**:

  ```powershell
  winget install --id JGraph.Draw -e --accept-source-agreements --accept-package-agreements
  ```

  macOS: `brew install --cask drawio`. Linux: see the [releases page](https://github.com/jgraph/drawio-desktop/releases).

### Accounts

- A **GitHub account** that lets you create a new repository (personal account, or corporate GHEC with `Create repository` rights). Sign in to GitHub from VS Code or via `gh auth login`.

## Prep the environment — deploy the mock landing zone

The "platform" side of the landing zone is simulated by a single hub VNet in [mock-alz/](mock-alz/). Deploy it before starting Chore 1:

```powershell
cd mock-alz
./Deploy-Hub.ps1            # optional: -Location westeurope -ResourceGroupName rg-platform
```

This creates `rg-platform` with a hub VNet (`vnet-hub`, `192.168.100.0/24`) and placeholder subnets for firewall, gateway, and shared services. It is the "given" for every chore. Do not edit or delete it.

## How to work through the chores

Each chore in [CHORES.md](CHORES.md) is meant to be solved **with Copilot in agent mode**, not by copy-pasting a finished solution. The loop is always the same: share the chore as the prompt, let Copilot draft IaC and scripts using the MCP servers, review the diff, deploy, verify, iterate. If you get stuck or want to check your work, open [DETAILS.md](DETAILS.md) for hints, expected outcomes, and background.

Two non-negotiables that come from [.github/copilot-instructions.md](.github/copilot-instructions.md):

1. **Never edit anything under `workload-app/`** — it represents the application team's code, and the platform team treats it as immutable. (Chore 15 is the single, deliberate exception.)
2. **All container base images come from `mcr.microsoft.com`**, not Docker Hub.
