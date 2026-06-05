# Chore 6 — Parameterise the workload template and add a `prod` environment

The application team now needs a **production** environment alongside `test`. Both coexist in
the same subscription. The principle is non-negotiable: **one template, two parameter files** —
every environment difference is a parameter value, not a code branch.

## Requirements

- The design doc and diagram are **updated first** to show both environments, the address-space
  split, and the parameter-driven model — before any Bicep changes.
- The workload Bicep deploys **both `test` and `prod` from the same template**. Every
  difference (scaling, SKUs, zone redundancy, address space, auto-pause, replica count) is a
  **parameter value**. There are **no** environment `if` branches in the template.
- The existing `test` deployment is **preserved byte-for-byte**: re-deploying `test` is a
  clean no-op (what-if shows zero changes; nothing is renamed, dropped, or recreated).
- **Prod** runs **at least 3 replicas spread across availability zones**, no scale-to-zero, and
  a **zone-redundant** data tier.
- Each environment lives in its **own resource group** and on its **own spoke**
  (non-overlapping address spaces). Spokes peer to the hub independently and **not** to each
  other. The **distributed Private DNS** pattern applies per environment, with isolated zones.
- The deploy script takes an `-Environment test|prod` switch and runs preflight before every
  deploy.
- **Prod is actually deployed** at the end of the chore.

## Success criteria

**Done when**
- `prod` is deployed with zone-redundant compute and data, ≥ 3 replicas, its own spoke and
  DNS; `test` is unchanged.

**Verify**
- Deploying `test` reports a clean no-op what-if against its resource group.
- The hub shows two workload peerings (`...-test`, `...-prod`), both `Connected`, no orphans.
- The template contains **zero** `if (environmentName == ...)` branches.

**Enough to move on**
- Both environments are healthy on their own frontend FQDNs and only parameter values differ
  between them.

---
Background, the parameter shape, and the zone-redundancy caveat: [details-06.md](details-06.md).
