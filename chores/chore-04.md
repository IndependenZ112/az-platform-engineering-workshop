# Chore 4 — Implement the workload infrastructure in Bicep

- All Bicep lives under `infra/workload-01/` and **uses Azure Verified Modules** wherever one exists.
- Resource names follow **Microsoft CAF**.
- Identity is wired end-to-end with **user-assigned managed identities**; **no secrets** in parameters, outputs, or config.
- Implements the **distributed Private DNS** pattern: zones live in the workload RG, linked to the spoke (registration) and hub (resolution).
- A `Deploy-Workload.ps1` script wraps the deployment and runs **preflight (what-if + permission check)** before every `az deployment group create`.
- The deployment is **idempotent**.

Stuck or want to check your work? See [details-04.md](details-04.md).
