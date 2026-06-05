# Chore 11 — Commit and push the workflows, leave a clean working tree

The two workflow files only take effect once they are on `main` of your remote. Land them
cleanly and confirm the working tree is clean.

## Requirements

- The staged diff is **inspected before committing** — only the workflow YAMLs and related docs
  land. No application source, no container build assets, no parameter files with real
  subscription IDs, no local-only files.
- The work is committed cleanly (one commit per workflow, plus docs) and pushed to `main` so
  the workflows take effect.

## Success criteria

**Done when**
- Both workflows exist on `main` and the working tree is clean.

**Verify**
- `git status` reports `nothing to commit, working tree clean`.
- On the **Actions** tab, both workflows are listed and dispatchable.
- On **Settings → Environments**, `test` and `prod` exist with federated credentials,
  variables, and (for `prod`) required reviewers.

**Enough to move on**
- The workflows are live on the remote and the environments are correctly configured.

---
Background, the inspection commands, and why this is its own chore: [details-11.md](details-11.md).
