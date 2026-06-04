# Chore 9 — Publish your work to your own GitHub repo

### Background

So far you've been working in a local clone of [azureholic/az-platform-engineering-workshop](https://github.com/azureholic/az-platform-engineering-workshop) with no write access. Time to take ownership: create a repo under your own GitHub account, swap `origin` over, push everything. Clean break — no `upstream`, no fork relationship.

### Hints

Review before publishing:

```powershell
git status
git diff --stat origin/main
git log --oneline origin/main..HEAD
```

Anything surprising (large generated files, secrets, `.bicepparam` with real subscription IDs, local-only paths) gets fixed or `.gitignore`d **before** the first push.

If your work is one giant uncommitted blob, split it into a handful of logical commits (e.g. `chore-1 toolbox`, `chore-2 spoke`, `chore-4 workload infra`). Conventional Commits is fine but not required.

Create the empty remote — **do not** initialise with README/.gitignore/license:

```powershell
# Option A — GitHub CLI
gh repo create <your-handle-or-org>/az-platform-engineering-workshop `
    --private `
    --description "My run through the Azure Platform Engineering workshop" `
    --disable-wiki

# Option B — https://github.com/new (Owner + Name + Private, no init files)
```

Swap `origin`:

```powershell
git remote set-url origin https://github.com/<your-handle-or-org>/az-platform-engineering-workshop.git
git remote -v
git push -u origin main
```

Verify the round-trip in the browser: commit graph matches `git log --oneline`, last commit's author is your GitHub identity, no secrets or local-only artifacts (`*.tfstate`, `*.pem`, `bin/`, `obj/`). If anything leaked, scrub with [`git filter-repo`](https://github.com/newren/git-filter-repo) and force-push **once** before anyone clones the repo.

### Outcome

Your repo is on GitHub under an account you control, with clean history and clean commit identity. Future `git push` / `git pull` are one-liners.

### Heads up

Without `upstream`, you won't see new chores added to [azureholic/az-platform-engineering-workshop](https://github.com/azureholic/az-platform-engineering-workshop). Add it on demand: `git remote add upstream https://github.com/azureholic/az-platform-engineering-workshop.git` and cherry-pick what you need.
