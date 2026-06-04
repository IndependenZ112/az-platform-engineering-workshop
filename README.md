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
- [CHORES.md](CHORES.md) — the chores, as an index of per-chore pages under [chores/](chores/). Short, requirements-only. Work through them in order with Copilot.
- Each chore page links to a matching `chores/details-NN.md` with hints, expected outcomes, and background. Open the detail page when you get stuck or want to check your work.

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

## Prep the environment — deploy the mock landing zone

The "platform" side of the landing zone is simulated by a single hub VNet in [mock-alz/](mock-alz/). Deploy it before starting Chore 1:

```powershell
cd mock-alz
./Deploy-Hub.ps1            # optional: -Location westeurope -ResourceGroupName rg-platform
```

This creates `rg-platform` with a hub VNet (`vnet-hub`, `192.168.100.0/24`) and placeholder subnets for firewall, gateway, and shared services. It is the "given" for every chore. Do not edit or delete it.

## How to work through the chores

Each chore in [CHORES.md](CHORES.md) is meant to be solved **with Copilot in agent mode**, not by copy-pasting a finished solution. The loop is always the same: share the chore as the prompt, let Copilot draft IaC and scripts using the MCP servers, review the diff, deploy, verify, iterate. If you get stuck or want to check your work, open the matching `chores/details-NN.md` page for hints, expected outcomes, and background.

> If you don't know how to approach a chore, ask Copilot to help you! It is an execelent troubleshooter and can run any CLI command for you to achieve your goals. Be explicit in your instructions.

Two non-negotiables that come from [.github/copilot-instructions.md](.github/copilot-instructions.md):

1. **Never create, edit, move, or delete anything under `workload-app/`** — the platform team treats it as immutable. Dockerfiles and other build assets live under `dockerfiles/`. Chore 14 requires a one-line edit inside `workload-app/`, but you make it **by hand** in the editor — Copilot is not allowed to touch the folder.
2. **All container base images come from `mcr.microsoft.com`**, not Docker Hub.
