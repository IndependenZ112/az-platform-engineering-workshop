# Chore 11 — Commit and push the workflows

### Background

The two previous chores produced two workflow files (`.github/workflows/infra-deploy.yml`, `.github/workflows/app-deploy.yml`) sitting as **uncommitted local changes**. They can't run until they're on `main` of your remote — GitHub Actions reads workflows from the repo, not your laptop.

### Hints

Inspect before committing:

```powershell
git status
git diff -- .github/workflows/
git diff -- README.md
```

Only workflow YAMLs and related docs land in this commit. No application source or container build assets under `workload-app/` (those belong to their own chores). No `*.bicepparam` with real subscription IDs. No local-only test files.

Commit cleanly — one commit per workflow:

```powershell
git add .github/workflows/infra-deploy.yml
git commit -m "ci: add staged infra deploy workflow"

git add .github/workflows/app-deploy.yml
git commit -m "ci: add build-once app deploy workflow"

git add README.md docs/
git commit -m "docs: document CI/CD workflows"

git push origin main
```

If you've been on a feature branch, **open a PR and merge to `main`** — the `paths:` filters only fire on `push` to `main`.

Sanity-check **Settings → Environments**: both `test` and `prod` exist with federated credentials, secrets/variables, and (for `prod`) required reviewers. If they're missing, the next run fails with `Error: No subscription found` or `Error: environment 'prod' not configured`.

### Outcome

```text
On branch main
Your branch is up to date with 'origin/main'.

nothing to commit, working tree clean
```

Both `infra-deploy` and `app-deploy` appear on the Actions tab with status `active` and can be dispatched manually.

### Why this is its own chore

"Add a workflow file" and "make the workflow runnable" are not the same thing. A workflow that only exists on your laptop is just YAML — it doesn't gate anything, deploy anything, or show up on the Actions tab. This chore is the bridge: it turns the two workflow files into actual CI/CD.
