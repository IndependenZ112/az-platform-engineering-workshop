# Chores

Work through these in order with GitHub Copilot in agent mode. Each chore lists **requirements only** — the *how* is up to you and Copilot. If you get stuck, want hints, or want to check your work, see [DETAILS.md](DETAILS.md).

---

## Chore 1 — Curate the Copilot toolbox for this repo

- Read [README.md](README.md) and [.github/copilot-instructions.md](.github/copilot-instructions.md) **with Copilot**.
- Pick a shortlist of **agents, instructions, and skills** from [aka.ms/awesome-copilot](https://aka.ms/awesome-copilot) that fit this workshop.
- Install them into the repo under `.github/instructions/`, `.github/skills/`, `.github/agents/`.
- Reload the VS Code window so they are picked up.
- Smoke-test that Copilot now follows them.

## Chore 2 — Onboard a workload spoke

- Create a workload resource group (e.g. `rg-workload-01`).
- Deploy a **spoke VNet** in a non-overlapping address space, with subnets sized for private endpoints and room to grow.
- **Peer the spoke to the hub** in both directions.
- Verify peering shows `Connected` on both sides.

## Chore 3 — Investigate the workload and design its infrastructure

- Analyze the application in [workload-app/](workload-app/) end-to-end.
- Produce an **infrastructure design** (Markdown in `docs/`, with a draw.io diagram) for hosting it on Azure on containers.
- The design must be **Well-Architected**, **scale-to-zero where possible**, and use **private endpoints for every PaaS service.**.
- The plan should be detailed enough that a follow-up implementation can build it without re-opening architectural decisions.
- **No Bicep is written in this chore** — output is design only.

## Chore 4 — Implement the workload infrastructure in Bicep

- All Bicep lives under `infra/workload-01/` and **uses Azure Verified Modules** wherever one exists.
- Resource names follow **Microsoft CAF**.
- Identity is wired end-to-end with **user-assigned managed identities**; **no secrets** in parameters, outputs, or config.
- Implements the **distributed Private DNS** pattern: zones live in the workload RG, linked to the spoke (registration) and hub (resolution).
- A `Deploy-Workload.ps1` script wraps the deployment and runs **preflight (what-if + permission check)** before every `az deployment group create`.
- The deployment is **idempotent**.

## Chore 5 — Deploy the workload infrastructure

- Run preflight against the template — validate, what-if, permission check — and only proceed when all three are clean.
- Execute the deploy script. Re-run it and confirm the second run's what-if is a **no-op**.
- Verify the public surface is **only** the frontend FQDN and the container registry.
- Verify private DNS resolves to private IPs from inside the spoke.
- Do **not** deploy the container images yet — that is a follow-up chore.

## Chore 6 — Build the container images and roll them out

- Two Dockerfiles live under `dockerfiles/`:
  - `Dockerfile.backend` for [workload-app/backend/HotelBooking.Api/](workload-app/backend/HotelBooking.Api/).
  - `Dockerfile.frontend` for [workload-app/frontend/](workload-app/frontend/), served by nginx with SPA fallback and `/api/` reverse-proxied to the backend's internal ingress.
- Every `FROM` line uses **`mcr.microsoft.com`**.
- A PowerShell script `dockerfiles/Build-And-Deploy.ps1` does the full rollout: reads outputs from the workload infrastructure deployment, logs in to ACR, builds with **`az acr build`**, tags with `:latest` and the short Git SHA, and updates each container app to the new image.
- The script is **idempotent**.
- After rollout, the frontend FQDN serves the SPA, `/api/hotels` returns JSON, and a booking POST works end-to-end.
- **You do not edit `workload-app/`.**

## Chore 7 — Add a production environment alongside test

- The workload-infrastructure Bicep is **parameterised by environment** (`test`, `prod`).
- Each environment lands in its own resource group (`rg-workload-01-test`, `rg-workload-01-prod`) with **non-overlapping spoke address spaces**.
- **Scaling profile per environment**:
  - **test**: `minReplicas = 0`.
  - **prod**: `minReplicas ≥ 3`, replicas **spread across availability zones**, ACA environment **zone-redundant**.
- **Azure SQL** in prod uses a **zone-redundant** SKU and does **not** auto-pause; test keeps the serverless scale-to-zero settings.
- The deploy script takes an `-Environment test|prod` switch.
- Deploying test and prod in either order leaves both healthy and reachable.
- Design doc and diagram are **updated** to show both environments.

## Chore 8 — Rebuild the deployment as two fully isolated environments

- **Tear down the existing single-environment workload deployment** (container apps → SQL/ACR/etc → spoke VNet + peerings → resource group). `mock-alz` / `rg-platform` are not touched.
- Stand up **two spoke VNets**, one per environment, each in its own resource group, each peered independently to the hub. The two spokes do **not** peer to each other.
- Workload Bicep deploys end-to-end into the **matching spoke** for its environment.
- The **distributed Private DNS** pattern still applies per environment.
- The deploy script can deploy spoke, workload, or both; spoke is deployed first.
- After rebuild, the hub shows exactly two workload peerings (`...-test`, `...-prod`), both `Connected`, no orphans.
- Design doc and diagram are updated **before** any Bicep changes.

## Chore 9 — Make sure you have a usable GitHub account

- You can sign in to GitHub with an account that lets you **create a new repository**.
- **Two-factor authentication is enabled.**
- Your **commit identity** (`git config --global user.name` / `user.email`) is set correctly.
- You can **authenticate to GitHub from VS Code** (or via `gh auth login`) as the intended account.

## Chore 10 — Publish your work to your own GitHub repo

- **Review the local diff** before publishing; nothing surprising or secret gets pushed.
- Stage your work into a handful of **clean commits**.
- Create a new **empty** repo under your account — do **not** initialise it with README/.gitignore/license.
- Swap the existing `origin` remote to point at your new repo (no `upstream`, no fork relationship).
- Push everything to `main` and set the upstream.
- Verify in the browser that the commit graph, author, and contents look right and contain no secrets.

## Chore 11 — Automate infra deployment with a staged GitHub Actions workflow

- A workflow at `.github/workflows/infra-deploy.yml` triggers on `push` to `main` (with a `paths:` filter on `infra/**`) and on `workflow_dispatch`.
- Auth uses **OIDC federation** — no long-lived secrets — with **separate app registrations per environment**.
- Three jobs: **`lint`** → **`deploy-test`** → **`deploy-prod`**, chained with `needs:`.
- **GitHub Environments** do the gating: `test` (no protection), `prod` (required reviewers, branch policy `main`).
- Every deploy job runs `what-if` first and posts the output to the job summary.
- First end-to-end run: test deploys without prompting, prod waits in **Waiting** until you approve.

## Chore 12 — Automate app container deployment with a build-once, promote-everywhere workflow

- A workflow at `.github/workflows/app-deploy.yml` triggers on `push` to `main` (`paths:` filter on `workload-app/**` and `dockerfiles/**`) and on `workflow_dispatch`.
- Four jobs: **`build`** → **`deploy-test`** → **`smoke-test`** → **`deploy-prod`**.
- **The build job runs once per workflow run** and emits image **digests** as job outputs.
- Both deploy jobs pin to the **digest** (not the tag) — prod deploys the **exact same bytes** that passed test.
- **No `build` / `az acr build` runs in the prod stage.**
- Prod is gated by the `prod` GitHub Environment's required reviewer.
- Each deploy job writes the digest it deployed to the job summary.

## Chore 13 — Commit and push the workflows, leave a clean working tree

- **Inspect** the staged diff before committing — only the workflow YAMLs and related docs land.
- Commit cleanly (one commit per workflow plus docs).
- Push to `main` so the workflows take effect.
- `git status` reports `nothing to commit, working tree clean`.
- On the **Actions tab**, both workflows are listed and dispatchable.
- On **Settings → Environments**, `test` and `prod` exist with federated credentials, variables, and (for `prod`) required reviewers.

## Chore 14 — Prove the infra pipeline by retagging the workload

- Pick one tag to change or add on the workload Bicep under `infra/workload-01/`.
- The change is **minimal and reversible** — `what-if` shows only tag deltas.
- Run preflight locally first; confirm only tag deltas appear.
- Commit and push to `main`.
- On the Actions tab, the `infra-deploy` workflow triggers: `lint` and `deploy-test` go green; `deploy-prod` waits for approval; after approval, both resource groups have the new tag.

## Chore 15 — Prove the app pipeline by rebranding the frontend

> **You do this one by hand.** Copilot is not allowed to touch `workload-app/` under any circumstance. Make the edit yourself in the editor, then go back to platform work.

- **Manually** change the `<title>` in [workload-app/frontend/index.html](workload-app/frontend/index.html) to something with your own name in it (e.g. `Azureholic Hotels`). Nothing else in `workload-app/` changes.
- Not sure how to make the edit? **Ask Copilot for guidance** (e.g. "how do I change the page title in this HTML file?") — just don't let it edit the file for you. You type the change.
- Commit and push to `main`.
- On the Actions tab, the `app-deploy` workflow triggers: `build` → `deploy-test` → `smoke-test` → `deploy-prod` (waiting for approval).
- After approval, opening the test and prod frontend URLs shows the new title in the browser tab.
- `az containerapp show` on test and prod returns the **same image digest** — prod ran the byte-for-byte artifact that passed test.

---

More chores will be added as the workshop grows.
