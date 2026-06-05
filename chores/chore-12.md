# Chore 12 — Prove the infra pipeline by retagging the workload

- Pick one tag to change or add on the workload Bicep under `infra/workload-01/`.
- The change is **minimal and reversible** — `what-if` shows only tag deltas.
- Run preflight locally first; confirm only tag deltas appear.
- Commit and push to `main`.
- On the Actions tab, the `infra-deploy` workflow triggers: `lint` and `deploy-test` go green; `deploy-prod` waits for approval; after approval, both resource groups have the new tag.

Stuck or want to check your work? See [details-12.md](details-12.md).
