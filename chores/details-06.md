# Chore 6 — Parameterise the workload template and add a `prod` environment

### Background

Chore 3 stood up the workload **for the team's `test` environment** with `test` baked into every resource name. Now the application team needs a **production** environment alongside it. The principle of this chore is non-negotiable: **one Bicep template, two parameter files**. Every difference between `test` and `prod` (scaling, SKUs, zone redundancy, address space, auto-pause, replica count) is a parameter value, **not** a code branch.

Prod must run **at least 3 replicas spread across availability zones**, no scale-to-zero. Both environments coexist in the same subscription, in separate resource groups, on separate spokes.

### Hints

**Introduce the `environmentName` parameter.** In `main.bicep`:

```bicep
@allowed([ 'test', 'prod' ])
param environmentName string
```

Use it wherever a name was hardcoded to `test` in Chore 3. The `test` deployment keeps every existing name byte-for-byte (`rg-workload-01-test`, `ca-hotelapi-test-<region>-001`, `cae-hotelapi-test-<region>-001`, `sql-hotelapi-test-<region>-001`, …). A `what-if` against the `test` RG **must show zero changes** — if it shows renames or deletes, you've broken the contract and the test environment will be torn down on the next deploy.

**Parameterise every environment-specific value.** Replace each prod-vs-test difference with a parameter. Suggested shape:

```bicep
// Networking
param spokeAddressSpace string                 // test: '10.10.0.0/22'  prod: '10.20.0.0/22'

// Container Apps
@minValue(0)
param minReplicas int                          // test: 0              prod: 3
@minValue(1)
param maxReplicas int                          // test: 3              prod: 10
param acaEnvZoneRedundant bool                 // test: false          prod: true

// Azure SQL
param sqlSkuName string                        // test: 'GP_S_Gen5_1'  prod: 'GP_S_Gen5_2' (or BC_Gen5_2)
param sqlZoneRedundant bool                    // test: false          prod: true
param sqlAutoPauseDelay int                    // test: 60             prod: -1 (disabled)
```

…then pass them through to the AVM modules. **No `if (environmentName == 'prod') { ... } else { ... }` branches in the template.** If you find yourself writing one, the value belongs in a parameter.

Two parameter files alongside `main.bicep`:

- `workload.test.bicepparam` — exact values that match the existing `test` deployment.
- `workload.prod.bicepparam` — prod values (zone-redundant, ≥ 3 replicas, no auto-pause, non-overlapping CIDR).

`Deploy-Workload.ps1` gains an `-Environment` switch and selects:

| `-Environment` | Resource group         | Parameter file              |
| -------------- | ---------------------- | --------------------------- |
| `test`         | `rg-workload-01-test`  | `workload.test.bicepparam`  |
| `prod`         | `rg-workload-01-prod`  | `workload.prod.bicepparam`  |

Preflight (what-if + permission check) runs before every `az deployment group create`, regardless of environment.

**One thing genuinely can't be flipped by a parameter on an existing resource:** the Container Apps environment's `zoneRedundant` flag is **set at creation and cannot be changed later**. The existing test ACA environment stays as it is (not zone-redundant); the prod ACA environment is created fresh **as** zone-redundant. The parameter still drives the value — there's just no in-place migration path for that property, so what-if on test continues to show no change.

**Address spaces.** Suggest test = `10.10.0.0/22`, prod = `10.20.0.0/22`. Write the convention into the design doc **before** changing the Bicep. Each spoke peers to the hub in both directions; spokes do **not** peer to each other.

**Distributed Private DNS per environment.** Each env owns its own `privatelink.*` zones in its own RG, linked to its own spoke (registration) and to the hub (resolution). Do not share zones between environments — DNS resolution must be isolated.

### Outcome

- `Deploy-Workload.ps1 -Environment test` produces a clean **no-op** what-if against `rg-workload-01-test`.
- `Deploy-Workload.ps1 -Environment prod` deploys `rg-workload-01-prod` end-to-end with zone-redundant ACA, ≥ 3 replicas, zone-redundant SQL, and its own spoke + DNS.
- Both environments are healthy on their own frontend FQDNs.
- Hub shows two workload peerings (`...-test`, `...-prod`), both `Connected`, no orphans.
- Design doc + diagram updated to show both environments, the address-space split, and the parameter-driven model.
- The template has **zero** `if (environmentName == ...)` branches.

### Reliability note

Zone redundancy on Container Apps and Azure SQL is **only available in regions with 3 availability zones**. Confirm your region qualifies (e.g. `westeurope`, `northeurope`, `eastus2`, `swedencentral`). If your test region doesn't, either move prod to one that does or call out the limitation in the updated design — don't fake `zoneRedundant: true` in a region that can't support it; the deploy will fail at provisioning time.

### Why "no code forks"

A template with `if (env == 'prod')` branches looks fine until the first prod-only bug: you can't reproduce it in test, because test runs a different code path. Parameterising every difference means **the same lines of Bicep deploy both environments**, so a working test deployment is real evidence that prod will work too — only the values differ.

### Info — multi-subscription topology (out of scope for this workshop)

This workshop runs **everything in one subscription** — the hub VNet, the test workload, and the prod workload all live together. In a real landing zone, those typically split across subscriptions for blast-radius and billing isolation:

```
┌──────────────────────────────────┐
│  connectivity-subscription       │
│  ┌────────────────────────────┐  │
│  │ rg-platform                │  │
│  │   └── vnet-hub             │  │
│  └────────────────────────────┘  │
└──────────────▲───────▲───────────┘
               │       │
       peering │       │ peering
               │       │
┌──────────────┼───┐ ┌─┼──────────────────┐
│ workload-    │   │ │ │ workload-        │
│ test-sub     │   │ │ │ prod-sub         │
│ ┌────────────┴─┐ │ │ ┌┴─────────────┐   │
│ │ rg-workload- │ │ │ │ rg-workload- │   │
│ │  01-test     │ │ │ │  01-prod     │   │
│ │   └── vnet-  │ │ │ │   └── vnet-  │   │
│ │   workload-  │ │ │ │   workload-  │   │
│ │   test       │ │ │ │   prod       │   │
│ └──────────────┘ │ │ └──────────────┘   │
└──────────────────┘ └────────────────────┘
```

Two practical consequences if you ever do this:

- The deploy identity (or your interactive `az login` context) needs to be **set to the workload subscription** for the workload deployment, but the peering operation **also writes to the hub VNet**, which is in a different subscription. You either give the identity `Network Contributor` cross-sub on the hub VNet resource, or you split the peering into a separate deployment that runs in the hub subscription.
- The hub VNet's **subscription ID** becomes a real parameter — you can no longer rely on `subscription().subscriptionId` for the hub side of the peering. The parameter file for each workload env carries the hub subscription ID and the hub VNet resource ID explicitly.

We're not doing this in the workshop — calling it out only so you know what the parameterised template would need to grow into when you take it back to your own landing zone.
