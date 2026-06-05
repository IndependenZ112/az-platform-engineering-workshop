# Chore 12 — Prove the infra pipeline by retagging the workload

Prove the infra pipeline reacts to a real change with the smallest, safest edit possible: a
tag change. Tags don't move or restart resources, and what-if makes the diff obvious.

## Requirements

- One tag is changed or added on the workload Bicep under `infra/workload-01/`.
- The change is **minimal and reversible** — what-if shows only tag deltas.
- Preflight runs locally first and confirms only tag deltas appear.
- The change is committed and pushed to `main`.

## Success criteria

**Done when**
- The `infra-deploy` workflow runs from your commit and applies the tag to both environments.

**Verify**
- `lint` and `deploy-test` go green; the test job summary shows **only** the tag change.
- `deploy-prod` waits for approval, then applies the same tag to the prod resource group.
- The tag matches across both resource groups afterwards.

**Enough to move on**
- A trivial infra change flowed lint → OIDC → what-if → deploy → approval end-to-end.

---
Background, candidate tag edits, and trigger troubleshooting: [details-12.md](details-12.md).
