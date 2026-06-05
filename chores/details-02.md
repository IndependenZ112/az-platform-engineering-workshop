# Chore 2 — Investigate the workload and design its infrastructure

### Background

The application team has dropped a workload in [workload-app/](../workload-app/) — a .NET 10 Minimal API backend and a React/Vite frontend, with data persisted in SQL Server. Before deploying anything, the platform team **understands the app** and **designs the Azure footprint** that will host it. This chore is design only — the output is a plan, not Bicep.

### Hints

The design document should answer:

- What the backend is (runtime, framework, exposed endpoints, dependencies).
- What the frontend is (build tooling, how it talks to the backend, how it's served in production).
- What data store it expects, what authentication it uses, what config it reads at startup.
- Which compute service hosts the containers, and why it fits this workload.
- Where container images live and how that registry is exposed.
- How the workload reaches its data tier privately, reusing the spoke and the distributed Private DNS pattern.
- How identity flows end-to-end — which managed identities exist, what they're allowed to do.
- **Naming.** The design is for a `test` environment, so every resource name embeds `test` as the CAF environment segment from the start — `rg-workload-01-test`, `vnet-spoke-workload01-test-<region>-001`, `ca-hotelapi-test-<region>-001`, `id-hotelapi-test-<region>-001`, etc. (Container Registry has its own no-hyphen pattern — see [.github/instructions/azure-naming.instructions.md](../.github/instructions/azure-naming.instructions.md).) The point is to lock in the well-architected design for the team's test environment and stand it up cleanly with the env token baked into every name from the start.
- **Separation of runtime vs. deploy identities.** The design must name a dedicated **GitHub Actions deploy identity per environment** (e.g. `id-github-workload01-test-<region>-001`, `...-prod-...`), distinct from any runtime UAMI on the container apps. Rationale: a compromised CI runner cannot read the app's data plane, and a compromised app pod cannot redeploy infrastructure. For each deploy identity capture: scope of `Owner` on its own workload RG (it must create role assignments for the runtime UAMIs), `Network Contributor` on the hub VNet (to write the peering), and `AcrPush` on the workload container registry (for the app workflow's build job). The federated-credential subject pattern is `repo:<owner>/<repo>:environment:<env>`. Implementation lives in a follow-up chore — here, name it, scope it, and write it into the diagram.
- Which subnets the workload needs in the spoke; whether the previous chore's address plan still fits.
- Inbound exposure: how users reach the frontend.
- Trade-offs against the [Well-Architected Framework](https://learn.microsoft.com/azure/well-architected/) pillars — Reliability, Security, Cost, Operations, Performance.

**Private endpoints everywhere**, with two explicit exceptions:

- The **container registry** stays publicly reachable (so the workshop's build/push works without a private build agent). **`Basic` SKU is enough** for the workshop — there is no requirement for geo-replication, content trust, customer-managed keys, or private endpoints, so don't pick `Premium`. `Standard` is fine if you want more throughput / storage; `Premium` is overkill and costs ~5× more per month.
- The **frontend app** stays publicly reachable (it's the user-facing entry point).

### Outcome

A short, opinionated design document in `docs/` plus a draw.io diagram, signed off and ready for a follow-up implementation chore without re-litigating decisions.

### Why this chore exists

If you skip straight to Bicep, you end up redesigning halfway through writing it. Writing the design first turns the Bicep chore into pure execution.
