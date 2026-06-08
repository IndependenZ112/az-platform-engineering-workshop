# Chore 1 — Onboard a workload spoke

A new application team needs network space for their test environment. As the platform team, add a spoke to the
hub-and-spoke topology and connect it to the existing hub.

## Requirements

- We need a workload resource group.
- A **spoke VNet** is deployed in an address space that does **not** overlap the hub, with at
  least a subnet sized for private endpoints and headroom for app/data subnets to follow.
- The spoke is **peered to the hub in both directions**.
- The network is defined as **Bicep** and deployed to your subscription.

## Success criteria

**Done when**
- The resource group and spoke VNet exist in the subscription.
- Peering exists on both the hub and the spoke side.

**Verify**
- `az network vnet peering list` shows `Connected` on **both** sides.
- Both peerings report `peeringState = Connected` and the spoke's address space appears in the
  hub peering's `remoteAddressSpace` (and vice versa). No live data-path test is expected yet —
  the spoke has no resources in it at this stage.

**Enough to move on**
- Bidirectional peering is `Connected` and the spoke's address space is non-overlapping.

---
Background, hints, and verification details: [details-01.md](details-01.md).
