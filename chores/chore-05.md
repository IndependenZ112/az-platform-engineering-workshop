# Chore 5 — Deploy the workload infrastructure

- Run preflight against the template — validate, what-if, permission check — and only proceed when all three are clean.
- Execute the deploy script. Re-run it and confirm the second run's what-if is a **no-op**.
- Verify the public surface is **only** the frontend FQDN and the container registry.
- Verify private DNS resolves to private IPs from inside the spoke.
- Do **not** deploy the container images yet — that is a follow-up chore.

Stuck or want to check your work? See [details-05.md](details-05.md).
