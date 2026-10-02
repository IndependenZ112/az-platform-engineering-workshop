# Chore 2 — Investigate the workload and design its infrastructure

The application team has dropped a workload in [workload-app/](../workload-app/). Before any
infrastructure is written, understand the app and design the Azure footprint that will host
it. This chore produces a **design, not Bicep**.

## Requirements

- The application in [workload-app/](../workload-app/) is analysed end-to-end: runtime,
  framework, endpoints, how the frontend talks to the backend, data store, authentication,
  and what config it reads at startup.
- An **infrastructure design** is produced as Markdown in `docs/`, with an accompanying
  architecture diagram, for hosting the workload on Azure using containers.
- The **container hosting service is chosen to fit the workload's requirements** (scale-to-zero,
  private networking, managed identity, internal vs. public ingress, operational overhead), and
  the choice is justified against the alternatives that were considered.
- The design is framed as a well-architected **`test`** environment: every CAF resource name
  carries the `test` environment token from the start (e.g. `rg-workload-01-test`,
  `ca-hotelapi-test-<region>-001`).
- The design is **Well-Architected**, **scale-to-zero**, and uses **private endpoints for
  private workload PaaS services**. The frontend and Monitor ingestion/query endpoints remain
  public; GHCR is external to the Azure workload.
- The design names a **dedicated CI/CD managed identity per environment**, separate from any
  runtime managed identity on the container apps, and records its intended scope.
- The design records **decisions with rationale**, and is detailed enough that a follow-up
  implementation can build it without re-opening architectural questions.
- The design is **reviewed and challenged** — disagreements are resolved before sign-off.

## Success criteria

**Done when**
- A design document and diagram exist in `docs/` and cover compute, data, networking,
  identity, GHCR image references, DNS, and inbound exposure.
- Naming, the private-endpoint posture, and the runtime-vs-CI identity split are all explicit.

**Verify**
- Private workload PaaS services are private; the frontend and Monitor stack are the only
  public workload endpoints, and GHCR is identified as the external public image source.
- Each major choice has a one-line rationale (Reliability / Security / Cost / Ops / Perf).

**Enough to move on**
- A reviewer could implement the workload from this document without making a new
  architectural decision.

---
Background, hints, and the design checklist: [details-02.md](details-02.md).
