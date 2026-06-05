# Chore 8 — Bootstrap GitHub Actions OIDC federation per environment

### Background

The design from Chore 2 named a **dedicated GitHub Actions deploy identity per environment**, and Chore 6 left you with two workload RGs (`rg-workload-01-test`, `rg-workload-01-prod`) and a published GitHub repo from Chore 7. Time to wire those together.

Doing this once in a script (not by clicking) means:

- A future fork/re-run only re-executes the script — no portal trail.
- The federated-credential subject (`repo:<owner>/<repo>:environment:<env>`) is generated, not typed — this is the single most common one-shot failure when participants set OIDC up by hand.
- Subsequent chores (the staged infra and app workflows) become pure YAML authoring — federation already works.

### Prereqs

- `az login` against the workshop subscription.
- `gh auth login` with **`repo` + `workflow` + `admin:repo_hook` + `read:org` scopes** (the GitHub Environments API is gated by `repo` scope on a personal repo, `admin:org` on an org repo).
- Your repo from Chore 7 set as `origin`.

### Hints

The script's outline:

```powershell
#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$WorkloadName = 'workload01',
    [string[]]$Environments = @('test','prod'),
    [string]$HubResourceGroup = 'rg-platform',
    [string]$HubVnetName = 'vnet-hub',
    [string]$AcrResourceGroup,           # default: per-env workload RG
    [string]$AcrName,                    # default: looked up from workload RG
    [string]$Location = 'swedencentral',
    [string]$RepoOwner,                  # default: inferred from `gh repo view`
    [string]$RepoName                    # default: inferred from `gh repo view`
)
```

Per-environment loop (idempotent — every `az` / `gh` call checks-then-writes):

1. Resolve the workload RG (`rg-$WorkloadName-$env`), the ACR (lookup if not provided), and the subscription/tenant from `az account show`.
2. **Create the deploy identity** if missing:

   ```powershell
   $miName = "id-github-$WorkloadName-$env-$Location-001"
   az identity create -g $workloadRg -n $miName -l $Location -o json
   $mi = az identity show -g $workloadRg -n $miName -o json | ConvertFrom-Json
   ```

3. **Assign roles** (each `az role assignment create` is naturally idempotent — re-running with the same scope+principal+role is a 200):

   - `Owner` on the workload RG (needed to create role assignments for the runtime managed identities during a deploy).
   - `Network Contributor` on the hub VNet *resource* (not the whole hub RG):

     ```powershell
     $hubVnetId = az network vnet show -g $HubResourceGroup -n $HubVnetName --query id -o tsv
     az role assignment create --assignee-object-id $mi.principalId --assignee-principal-type ServicePrincipal `
         --role 'Network Contributor' --scope $hubVnetId
     ```

   - `AcrPush` on the ACR resource ID (only the app workflow uses this, but the infra workflow shares the identity for simplicity).

4. **Federated credential** with the GitHub Environment subject:

   ```powershell
   $fic = @{
       name      = "gh-$RepoOwner-$RepoName-env-$env"
       issuer    = 'https://token.actions.githubusercontent.com'
       subject   = "repo:$RepoOwner/$($RepoName):environment:$env"
       audiences = @('api://AzureADTokenExchange')
   } | ConvertTo-Json -Compress
   # Use --parameters @- so PowerShell stdin carries the JSON safely.
   $fic | az identity federated-credential create -g $workloadRg --identity-name $miName --parameters '@-'
   ```

   Re-running with the same `name` returns 409 — catch and fall back to `az identity federated-credential update`, or just `az identity federated-credential delete` then create. (`update` is preferred; it preserves the credential's object ID.)

5. **GitHub Environment + protection rules** via `gh api` (no native `gh env` create command yet):

   ```powershell
   gh api -X PUT "repos/$RepoOwner/$RepoName/environments/$env" --silent
   if ($env -eq 'prod') {
       $reviewerId = gh api user --jq .id
       $body = @{
           reviewers = @(@{ type = 'User'; id = $reviewerId })
           deployment_branch_policy = @{ protected_branches = $true; custom_branch_policies = $false }
       } | ConvertTo-Json -Depth 5
       $body | gh api -X PUT "repos/$RepoOwner/$RepoName/environments/$env" --input -
   }
   ```

6. **Environment variables** (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP`) via `gh variable set --env $env`. These are **variables, not secrets** — they're not sensitive and can show up in logs without consequence.

### Verification

```powershell
az identity federated-credential list -g rg-workload-01-test --identity-name id-github-workload01-test-swedencentral-001 -o table
az identity federated-credential list -g rg-workload-01-prod --identity-name id-github-workload01-prod-swedencentral-001 -o table
gh variable list --env test
gh variable list --env prod
gh api repos/:owner/:repo/environments | ConvertFrom-Json | Select-Object -ExpandProperty environments | Format-Table name, protection_rules
```

You should see exactly one federated credential per identity, four variables per environment, and `prod` carrying a `required_reviewers` and `branch_policy` rule.

### Outcome

Two deploy identities exist with the right scopes; both can mint Azure tokens from the matching GitHub Environment; both environments carry the four variables the workflows in the next chores will consume. **No app registrations**, **no client secrets**, **no manual portal steps** — and re-running the script is a clean no-op.

### Why this is its own chore

OIDC bootstrap fails silently in obvious-looking ways: a typo in the subject string returns `AADSTS70021: No matching federated identity record found`, which surfaces only inside the workflow run an hour later. Splitting the wiring (this chore) from the workflow YAML (next two chores) means you debug each in isolation — and once it works, this chore is a single script you re-run for any new fork/clone.

### Safety note

The `Owner` role on the workload RG is intentional but **narrowly scoped**: the deploy identity needs to grant `AcrPull` to runtime managed identities during a deploy, which requires write access on `Microsoft.Authorization/roleAssignments` at that scope. It has **no rights** outside the workload RG except `Network Contributor` on the single hub VNet resource. Do not widen the scope to the subscription — `User Access Administrator` on the workload RG is an acceptable alternative if you want to split create-resources from grant-roles, but the simpler `Owner` is fine for the workshop.
