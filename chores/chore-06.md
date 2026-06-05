# Chore 6 — Parameterise the workload template and add a `prod` environment

- The application team now needs a **production** environment alongside `test`. Both environments coexist in the same subscription.
- Prod must run **at least 3 replicas spread across availability zones**, no scale-to-zero, and the data tier must be **zone-redundant**.
- The workload Bicep deploys **both `test` and `prod` from the same template** — every difference between environments is expressed as a parameter value, not as a code branch.
- The existing `test` deployment is **preserved**: re-deploying `test` after this chore is a clean **no-op** (`what-if` shows no changes, no resource is dropped or recreated).
- Each environment lives in its **own resource group** and on its **own spoke** (non-overlapping address spaces). Spokes peer to the hub independently and **do not peer to each other**.
- The **distributed Private DNS** pattern still applies per environment.
- The deploy script takes an `-Environment test|prod` switch and runs preflight before every deploy.
- Design doc and diagram are updated to show both environments and the parameter-driven model **before** any Bicep changes.
- **Actually deploy `prod`** at the end of the chore. After the deploy: `rg-workload-01-prod` exists, both spokes are peered to the hub, and a `what-if` against `test` still reports zero changes.

Stuck or want to check your work? See [details-06.md](details-06.md).
