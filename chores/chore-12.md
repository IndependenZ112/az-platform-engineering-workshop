# Chore 12 — Commit and push the workflows, leave a clean working tree

- **Inspect** the staged diff before committing — only the workflow YAMLs and related docs land.
- Commit cleanly (one commit per workflow plus docs).
- Push to `main` so the workflows take effect.
- `git status` reports `nothing to commit, working tree clean`.
- On the **Actions tab**, both workflows are listed and dispatchable.
- On **Settings → Environments**, `test` and `prod` exist with federated credentials, variables, and (for `prod`) required reviewers.

Stuck or want to check your work? See [details-12.md](details-12.md).
