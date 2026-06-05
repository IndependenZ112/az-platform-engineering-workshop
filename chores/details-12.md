# Chore 12 — Prove the infra pipeline by retagging the workload

### Background

The infra pipeline you built earlier is wired up but you haven't seen it react to a real change. The cheapest, safest proof is a **trivial, observable** edit — change or add a `tags` value. Tags don't move resources, don't restart anything, and `what-if` makes the diff easy to spot.

### Hints

Good candidates:

- Bump an existing `CostCenter` / `Owner` / `Environment` value.
- Add a new tag (`tags: { ManagedBy: 'platform-team', ChangeTicket: '<id>' }`) via the standard AVM `tags` parameter on the RG / resources.
- If your Bicep hoists a single `tags` object at the top of `main.bicep` and fans it out via `tags: tags` on every module, change it in **one** place.

Run preflight locally first so you don't burn a CI run on a typo:

```powershell
az bicep build --file infra/workload-01/main.bicep
az deployment group what-if `
    --resource-group rg-workload-01-test `
    --template-file infra/workload-01/main.bicep `
    --parameters infra/workload-01/main.test.bicepparam
```

Confirm the only changes are tag deltas. If anything else lights up, stop and figure out why before pushing.

```powershell
git add infra/workload-01/
git commit -m "infra(workload-01): add ManagedBy tag to prove CI pipeline"
git push origin main
```

If the workflow does **not** trigger:

1. The commit only touched files outside the `paths:` filter (`git show --stat HEAD`).
2. You pushed to a branch, not `main` (`git log origin/main -1`).
3. The workflow file didn't reach `main` in the previous chore — confirm on github.com.

### Outcome

- New run on the Actions tab tied to your commit SHA.
- `lint` green; `deploy-test` runs `what-if` against `rg-workload-01-test` and the job summary shows **only the tag change**.
- `deploy-test` finishes green; tag updated on `rg-workload-01-test` (`az group show -n rg-workload-01-test --query tags`).
- `deploy-prod` is in **Waiting**. Approve. Same shape against `rg-workload-01-prod`. Tag matches across both.

### Why a tag

A tag change is the smallest possible "real" infra edit: it exercises lint → OIDC login → what-if → deploy → environment approval, but the underlying resources don't change state. If you ever need to prove the pipeline still works after a credential rotation, workflow refactor, or quiet period, do this chore again.
