# Chore 2 — Investigate the workload and design its infrastructure

- Analyze the application in [workload-app/](../workload-app/) end-to-end.
- Produce an **infrastructure design** (Markdown in `docs/`, with a draw.io diagram) for hosting it on Azure on containers.
- **Frame the design as a well-architected `test` environment for the team.** Every CAF resource name carries the `test` environment token from day one (e.g. `rg-workload-01-test`, `ca-hotelapi-test-<region>-001`).
- The design must be **Well-Architected**, **scale-to-zero**, and use **private endpoints for every PaaS service**.
- The design must include a **dedicated managed identity for CI/CD**, separate from any runtime managed identity on the container apps.
- The design should contain decision records with rationale for choices that are made.
- The plan should be detailed enough that a follow-up implementation can build it without re-opening architectural decisions.
- Do not peek at future chores, just design with the requirements here.
- **No Bicep is written in this chore** — output is design only.
- Review the design and **challenge things you do not agree with**. Have a conversation when you need to.

Stuck or want to check your work? See [details-02.md](details-02.md).
