# Chore 1 — Onboard a workload spoke

### Background

A new application team needs network space. As the platform team, you add a **spoke VNet** that hosts their workload and connect it to the hub.

### Hints

- Spoke VNet needs at least a subnet for **private endpoints** (with `privateEndpointNetworkPolicies` configured appropriately) and room to grow for app/data subnets.
- For a hub without a gateway: `allowGatewayTransit` / `useRemoteGateways` stay **off**; `allowVirtualNetworkAccess` **on**; `allowForwardedTraffic` typically **on**.
- Peering must be created on **both** sides — hub→spoke and spoke→hub.

### Outcome

- Resource group `rg-workload-01` exists.
- Spoke VNet deployed in a non-overlapping range.
- `az network vnet peering list` shows `Connected` on both sides.
- A resource in the spoke can reach the hub address space.
