# Chore 7 — Bootstrap GitHub Actions OIDC federation per environment

### Background

The design from Chore 2 named a **dedicated GitHub Actions deploy identity per environment**, and Chore 5 left you with two workload RGs (`rg-workload-01-test`, `rg-workload-01-prod`) and a published GitHub repo from Chore 6. Time to wire those together.

Doing this once in a script (not by clicking) means:

- A future fork/re-run only re-executes the script — no portal trail.
- The federated-credential subject (`repo:<owner>/<repo>:environment:<env>`) is generated, not typed — this is the single most common one-shot failure when participants set OIDC up by hand.
- The staged infrastructure workflow can then use this federation without any long-lived credentials.

### Prereqs

- `az login` against the workshop subscription.
- `gh auth login` with **`repo` + `workflow` + `admin:repo_hook` + `read:org` scopes** (the GitHub Environments API is gated by `repo` scope on a personal repo, `admin:org` on an org repo).
- Your repo from Chore 6 set as `origin`.

### Hints

Write the bootstrap as a single idempotent PowerShell 7 script (every `az` / `gh` call checks-then-writes, so a re-run is a clean no-op). It should accept parameters for the workload name, the environments to provision (`test`, `prod`), the hub resource group and VNet name, the location, and the repo owner/name (defaulting to values inferred from `gh repo view`).

Per environment, the loop does the following:

1. Resolve the workload RG (`rg-$WorkloadName-$env`) and the subscription/tenant from `az account show`.
2. **Create the deploy identity** if missing — a user-assigned managed identity named `id-github-$WorkloadName-$env-$Location-001` in the workload RG.

3. **Assign roles** (each `az role assignment create` is naturally idempotent — the same scope+principal+role re-run is a 200):

   - `Contributor` on the workload RG.
   - `Network Contributor` on the hub VNet **resource** — look up its resource ID and scope to that, **not** the whole hub RG.

4. **Federated credential** whose `subject` is exactly `repo:<owner>/<repo>:environment:<env>`, with issuer `https://token.actions.githubusercontent.com` and audience `api://AzureADTokenExchange`. This subject string is the single most error-prone value — generate it from variables, don't type it — and pass the JSON over stdin (`--parameters @-`) so PowerShell quoting can't corrupt it. A re-run with the same `name` returns 409: prefer `az identity federated-credential update` over delete-then-create so the credential's object ID is preserved.

5. **GitHub Environment** via `gh api` (there's no native `gh env` create command). Create the environment, and for `prod` add a required-reviewer protection rule; `test` stays unprotected.

6. **Environment variables** (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP`) via `gh variable set --env $env`. These are **variables, not secrets** — they're not sensitive and can show up in logs without consequence.

### Verification

Confirm — with `az identity federated-credential list`, `gh variable list --env`, and `gh api .../environments` — that each deploy identity lists exactly **one** federated credential with the right `repo:<owner>/<repo>:environment:<env>` subject, each environment carries the **four** variables, and `prod` shows a `required_reviewers` protection rule.

### Outcome

Two deploy identities exist with the right scopes; both can mint Azure tokens from the matching GitHub Environment; both environments carry the four variables the workflows in the next chores will consume. **No app registrations**, **no client secrets**, **no manual portal steps** — and re-running the script is a clean no-op.

### Why this is its own chore

OIDC bootstrap fails silently in obvious-looking ways: a typo in the subject string returns `AADSTS70021: No matching federated identity record found`, which surfaces only inside the workflow run an hour later. Keeping federation setup separate from the infrastructure workflow YAML means you can debug each in isolation — and once it works, this chore is a single script you re-run for any new fork/clone.

### Safety note

The `Contributor` role on the workload RG is scoped to deploying that environment's resources. The identity has **no rights** outside the workload RG except `Network Contributor` on the single hub VNet resource. Do not widen either scope to the subscription.
