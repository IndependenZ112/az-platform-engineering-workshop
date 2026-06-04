# Chore 7 — Add a production environment alongside test

- The workload-infrastructure Bicep is **parameterised by environment** (`test`, `prod`).
- Each environment lands in its own resource group (`rg-workload-01-test`, `rg-workload-01-prod`) with **non-overlapping spoke address spaces**.
- **Scaling profile per environment**:
  - **test**: `minReplicas = 0`.
  - **prod**: `minReplicas ≥ 3`, replicas **spread across availability zones**, ACA environment **zone-redundant**.
- **Azure SQL** in prod uses a **zone-redundant** SKU and does **not** auto-pause; test keeps the serverless scale-to-zero settings.
- The deploy script takes an `-Environment test|prod` switch.
- Deploying test and prod in either order leaves both healthy and reachable.
- Design doc and diagram are **updated** to show both environments.

Stuck or want to check your work? See [details-07.md](details-07.md).
