# Chore 13 — Prove the app pipeline by rebranding the frontend

Prove the app pipeline end-to-end with the smallest visible change: the page title. This
chore edits **application source**, so you make the edit **by hand** — the platform tooling
never writes to application source under `workload-app/`.

## Requirements

- The `<title>` in [workload-app/frontend/index.html](../workload-app/frontend/index.html) is
  changed **by hand** to something with your own name/team in it. Nothing else in
  `workload-app/` changes.
- The change is committed and pushed to `main`.

## Success criteria

**Done when**
- The `app-deploy` workflow runs and promotes one image through test to prod.

**Verify**
- The run goes `build` → `deploy-test` → `smoke-test` → `deploy-prod` (waiting for approval).
- After approval, the test and prod frontend URLs both show the new title in the browser tab.
- `az containerapp show` on test and prod returns the **same image digest** — prod ran the
  exact artifact that passed test.
- `infra-deploy` does **not** run (its filter is `infra/**`).

**Enough to move on**
- You watched the same built image flow test → smoke → prod, with the change visible in the
  browser.

---
Background and how to find the title safely (without editing app source for you): [details-13.md](details-13.md).
