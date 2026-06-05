# Chore 13 — Prove the app pipeline by rebranding the frontend

> **You do this one by hand.** Copilot is not allowed to touch `workload-app/` under any circumstance. Make the edit yourself in the editor, then go back to platform work.

- **Manually** change the `<title>` in [workload-app/frontend/index.html](../workload-app/frontend/index.html) to something with your own name in it (e.g. `Azureholic Hotels`). Nothing else in `workload-app/` changes.
- Not sure how to make the edit? **Ask Copilot for guidance** (e.g. "how do I change the page title in this HTML file?") — just don't let it edit the file for you. You type the change.
- Commit and push to `main`.
- On the Actions tab, the `app-deploy` workflow triggers: `build` → `deploy-test` → `smoke-test` → `deploy-prod` (waiting for approval).
- After approval, opening the test and prod frontend URLs shows the new title in the browser tab.
- `az containerapp show` on test and prod returns the **same image digest** — prod ran the byte-for-byte artifact that passed test.

Stuck or want to check your work? See [details-13.md](details-13.md).
