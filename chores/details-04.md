# Chore 4 — Implement the workload infrastructure in Bicep

### Background

The design from the previous chore is signed off. Turn it into **deployable Bicep** that lands the workload in the existing spoke — without re-opening architectural decisions.

### Hints

- Bicep under `infra/workload-01/`. Use AVM wherever a module exists; raw `Microsoft.*` only with an inline comment explaining the gap.
- CAF naming — see [.github/instructions/azure-naming.instructions.md](../.github/instructions/azure-naming.instructions.md).
- If you discover a design gap while writing Bicep, **fix it in the design doc and diagram first**, then change the template.
- Identity wiring:
  - UAMI per container app, with `AcrPull` on the registry.
  - Backend UAMI granted access to the data tier as the design prescribes.
  - **No secrets** — construct connection strings from resource properties at deploy time; auth is MI-based.
- Distributed Private DNS: every zone created in the workload RG, linked to the spoke (registration) and the hub (resolution).
- Parameterise location, naming tokens, address space, SKU sizes — with sensible defaults.
- `Deploy-Workload.ps1` mirrors [mock-alz/Deploy-Hub.ps1](../mock-alz/Deploy-Hub.ps1). Run `azure-deployment-preflight` (what-if + permission checks) before every `az deployment group create`.
- Post-deploy steps that can't live in Bicep (e.g. contained DB users for UAMIs) are scripted alongside the template — not done by hand.

### Outcome

- `infra/workload-01/` deploys cleanly end-to-end.
- Second run of the script produces a no-op what-if (idempotent).
- Public surface = frontend FQDN + container registry. Everything else private.
- Resting cost matches the scale-to-zero estimate from the design.

### Hint — workflow

Use the **`bicep-plan`** chat mode to draft file layout and module choices, then switch to **`bicep-implement`** to write the resources. Keep the design open in the chat context.
