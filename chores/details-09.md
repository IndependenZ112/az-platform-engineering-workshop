# Chore 9 — Commit and push the workflows

### Background

The infrastructure and image-publishing workflows and related documentation are sitting as
**uncommitted local changes**. The infrastructure workflow takes effect from `main`; the image
workflow runs on pushes that change `workload-app/`. GitHub Actions reads workflows from the
repo, not your laptop.

### Hints

Inspect before committing:

```powershell
git status
git diff -- .github/workflows/
git diff -- README.md
```

Only workflow YAMLs and related docs land in these commits. No application source or container build assets under `workload-app/`. No `*.bicepparam` with real subscription IDs. No local-only test files.

Commit the workflow files together, then commit the documentation:

```powershell
git add .github/workflows/infra-deploy.yml
git add .github/workflows/build-and-publish-workload-images.yml
git commit -m "ci: add infrastructure and image publishing workflows"

git add README.md docs/ chores/
git commit -m "docs: document CI/CD workflows"

git push origin main
```

If you've been on a feature branch, **open a PR and merge to `main`** so the infrastructure
workflow runs there. The image-publishing workflow runs on pushes to any branch when
`workload-app/` changes.

Sanity-check **Settings → Environments**: both `test` and `prod` exist with federated credentials, secrets/variables, and (for `prod`) required reviewers. If they're missing, the next run fails with `Error: No subscription found` or `Error: environment 'prod' not configured`.

### Outcome

```text
On branch main
Your branch is up to date with 'origin/main'.

nothing to commit, working tree clean
```

`infra-deploy` appears on the Actions tab with status `active` and can be dispatched manually. The image publishing workflow runs automatically when files under `workload-app/` change.

### Why this is its own chore

"Add a workflow file" and "make the workflow runnable" are not the same thing. A workflow that only exists on your laptop is just YAML — it doesn't gate or deploy anything, or show up on the Actions tab. This chore is the bridge: it publishes the infrastructure and image-publishing workflows to GitHub.
