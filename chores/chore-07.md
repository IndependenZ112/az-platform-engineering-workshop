# Chore 7 — Publish your work to your own GitHub repo

Take ownership of the work: move it from the local clone to a repository under your own
GitHub account. Clean break — no fork relationship, no upstream.

## Requirements

- The local diff is **reviewed before publishing** — nothing surprising, secret, or local-only
  (real subscription IDs in parameter files, build output, certificates) gets pushed.
- The work is staged into a handful of **clean, logical commits**.
- A new **empty** repository is created under your account — **not** initialised with
  README/.gitignore/license.
- `origin` is repointed at the new repo (no `upstream`, no fork link), and everything is
  pushed to `main` with the upstream set.

## Success criteria

**Done when**
- Your repo on GitHub holds the full history under an account you control.

**Verify**
- The commit graph in the browser matches your local `git log`.
- The latest commit's author is your GitHub identity.
- No secrets or local-only artifacts are present in the pushed tree.

**Enough to move on**
- `git push` / `git pull` are one-liners against your own `origin`, and the history is clean.

---
Background, the review commands, and a secret-scrub note: [details-07.md](details-07.md).
