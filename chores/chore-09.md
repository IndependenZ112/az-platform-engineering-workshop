# Chore 9 — Commit and push the workflows, leave a clean working tree

The infrastructure and image-publishing workflows only take effect once they are on your
remote. Land them cleanly and confirm the working tree is clean.

## Requirements

- The staged diff is **inspected before committing** — workflow YAMLs, related docs, and any
  pending container build assets land. No application source, no parameter files with real
  subscription IDs, no local-only files.
- The work is committed cleanly and pushed to `main` so the workflows take effect.

## Success criteria

**Done when**
- Both workflows exist on `main` and the working tree is clean.

**Verify**
- `git status` reports `nothing to commit, working tree clean`.
- On the **Actions** tab, the infrastructure workflow is listed and dispatchable; the
  image-publishing workflow is listed and triggers on pushes that change `workload-app/`.
- On **Settings → Environments**, `test` and `prod` exist with federated credentials,
  variables, and (for `prod`) required reviewers.

**Enough to move on**
- The workflows are live on the remote and the environments are correctly configured.

---
Background, the inspection commands, and why this is its own chore: [details-09.md](details-09.md).
