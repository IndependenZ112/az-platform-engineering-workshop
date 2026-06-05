# Chore 4 — Deploy the workload infrastructure

The reviewed Bicep gets landed in the subscription. This is the first irreversible step, so
preflight comes first.

## Requirements

- Confirm the active subscription matches the hub and spoke.
- Run **preflight** against the template — validate, what-if, and permission check — and only
  proceed when all three are clean.
- Execute the deploy script, then re-run it and confirm the second run's what-if is a
  **no-op**.
- Container **images are not deployed yet** — the apps come up on the placeholder image; that
  is expected and a follow-up chore replaces it.

## Success criteria

**Done when**
- The workload infrastructure exists in the subscription and is fully private except the
  registry and frontend.

**Verify**
- The second deploy's what-if shows zero changes.
- The private endpoint's A record in the workload's Private DNS zone matches the endpoint's
  private IP, and that IP sits inside the private-endpoint subnet.
- From outside the spoke, the SQL FQDN resolves through a `privatelink.*` CNAME but a direct
  connection refuses (public access disabled).
- Container apps exist with `minReplicas = 0`, ACR admin user disabled, both managed
  identities hold `AcrPull` on the registry.
- The frontend's public URL loads in a browser and serves the placeholder image's default
  page (the real app is not built yet) — confirming public ingress works end to end.

**Enough to move on**
- Infrastructure is up, private, idempotent, and the resting cost matches the design.

---
Background, verification commands, and a safety note: [details-04.md](details-04.md).
