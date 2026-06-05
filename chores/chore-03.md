# Chore 3 — Implement the workload infrastructure in Bicep

- Use the design in the /docs folder
- All Bicep lives under `infra/workload-01/` and **uses Azure Verified Modules** wherever one exists.
- Resource names follow **Microsoft CAF** and embed the `test` environment token from day one (e.g. `rg-workload-01-test`, `ca-hotelapi-test-<region>-001`).
- Identity is wired end-to-end with **user-assigned managed identities**; **no secrets** in parameters, outputs, or config.
- Each container app pulls its image from ACR **using its own UAMI** — the UAMI holds `AcrPull` on the registry scope **and** the container app's `registries[]` entry references that UAMI as its `identity` (admin user stays disabled, no registry secrets).
- The **backend container app's UAMI is set as the SQL server's Microsoft Entra admin declaratively in Bicep** (via the AVM `sql/server` `administrator` parameter), with **Microsoft Entra-only authentication enabled**. No `az sql server ad-admin create` afterwards, no `deploymentScripts`, no jumpbox — schema creation runs on first app startup using that identity.
- Implements the **distributed Private DNS** pattern: zones live in the workload RG, linked to the spoke (registration) and hub (resolution).
- The backend container app receives `ConnectionStrings__HotelDb` as an environment variable, built from the SQL server FQDN at deploy time and using `Authentication=Active Directory Default` so the runtime picks up the UAMI without any secret.
- A `Deploy-Workload.ps1` script wraps the deployment and runs **preflight (what-if + permission check)** before every `az deployment group create`.
- The deployment is **idempotent**.

Stuck or want to check your work? See [details-03.md](details-03.md).
