# Chore 3 — Investigate the workload and design its infrastructure

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
- Which subnets the workload needs in the spoke; whether the previous chore's address plan still fits.
- Inbound exposure: how users reach the frontend.
- Trade-offs against the [Well-Architected Framework](https://learn.microsoft.com/azure/well-architected/) pillars — Reliability, Security, Cost, Operations, Performance.

**Private endpoints everywhere**, with two explicit exceptions:

- The **container registry** stays publicly reachable (so the workshop's build/push works without a private build agent).
- The **frontend app** stays publicly reachable (it's the user-facing entry point).

### Outcome

A short, opinionated design document in `docs/` plus a draw.io diagram, signed off and ready for a follow-up implementation chore without re-litigating decisions.

### Why this chore exists

If you skip straight to Bicep, you end up redesigning halfway through writing it. Writing the design first turns the Bicep chore into pure execution.
