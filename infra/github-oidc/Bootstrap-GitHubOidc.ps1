#requires -Version 7.0
<#
.SYNOPSIS
    Bootstraps GitHub Actions OIDC federation to Azure, per environment — no long-lived
    secrets, no manual portal steps.

.DESCRIPTION
    Idempotent and safe to re-run. For each environment (default: test, prod) this script:

      1. Creates a dedicated user-assigned managed identity for GitHub Actions deploys,
         separate from any runtime identity on the container apps.
      2. Grants it `Contributor` on its own workload resource group and `Network Contributor`
         on the hub resource group (see the NOTES section for why the hub *resource group*,
         not just the hub VNet resource).
      3. Creates or updates a federated credential whose subject targets the matching GitHub
         Environment, generated from variables — never hand-typed.
      4. Creates the GitHub Environment (`prod` gets required-reviewer protection; `test`
         stays unprotected) and publishes the four `AZURE_*` variables the deploy workflows
         need.

    Every step checks existing state before writing, so a second run changes nothing.

.PARAMETER WorkloadName
    Short workload token embedded in the deploy identity's name.

.PARAMETER Environments
    Which environments to bootstrap. Each must have an entry in the environment map below.

.PARAMETER HubResourceGroupName
    Resource group containing the existing hub VNet (read-only; never modified by this
    script beyond the role assignment described in NOTES).

.PARAMETER HubVnetName
    Name of the existing hub VNet — only used to validate it exists before granting access
    to it.

.PARAMETER RepoOwner
    GitHub owner (user or org) of the published repo. Inferred from `gh repo view` (the
    `origin` remote) when omitted.

.PARAMETER RepoName
    GitHub repository name. Inferred from `gh repo view` (the `origin` remote) when omitted.

.PARAMETER ProdReviewerLogin
    GitHub login required to approve `prod` deployments. Defaults to the currently
    authenticated `gh` user (reasonable for a single-maintainer workshop fork).

.NOTES
    `Network Contributor` is granted on the hub **resource group**, not just the hub VNet
    resource, per .github/instructions/github-oidc-federation.instructions.md: the AVM
    `virtual-network` module creates the *remote* side of a spoke↔hub peering via a nested
    deployment inside the hub resource group, which requires
    `Microsoft.Resources/deployments/write` at resource-group scope. Scoping only to the VNet
    resource fails with `AuthorizationFailed` on `<deploymentName>-virtualNetworkPeering-remote-0`.
    This deliberately widens the scope very slightly beyond "just the VNet" for a real,
    load-bearing reason — it is still scoped to exactly one resource group, nothing wider.
#>

[CmdletBinding()]
param(
    [string]$WorkloadName = 'hotelbooking',

    [ValidateSet('test', 'prod')]
    [string[]]$Environments = @('test', 'prod'),

    [string]$HubResourceGroupName = 'rg-platform',
    [string]$HubVnetName = 'vnet-hub',

    [string]$RepoOwner,
    [string]$RepoName,

    [string]$ProdReviewerLogin
)

$ErrorActionPreference = 'Stop'

# Per-environment workload resource group + region. Mirrors the mapping already used by
# infra/spoke-network/Deploy-Spoke.ps1 and infra/workload/Deploy-Workload.ps1.
$environmentMap = @{
    test = @{ ResourceGroupName = 'rg-hotelbooking-test-belgiumcentral'; Location = 'belgiumcentral' }
    prod = @{ ResourceGroupName = 'rg-hotelbooking-prod-belgiumcentral'; Location = 'belgiumcentral' }
}

function Grant-RoleAssignmentIfMissing {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$PrincipalId,
        [Parameter(Mandatory)][string]$Role,
        [Parameter(Mandatory)][string]$Scope
    )
    $existing = az role assignment list --assignee-object-id $PrincipalId --role $Role --scope $Scope --query '[0].id' -o tsv
    if ($LASTEXITCODE -ne 0) { throw "az role assignment list failed for role '$Role' at scope '$Scope'." }
    if ([string]::IsNullOrWhiteSpace($existing)) {
        Write-Host "    Granting '$Role' on $Scope..." -ForegroundColor DarkGray
        az role assignment create --assignee-object-id $PrincipalId --assignee-principal-type ServicePrincipal --role $Role --scope $Scope --output none
        if ($LASTEXITCODE -ne 0) { throw "az role assignment create failed for role '$Role' at scope '$Scope'." }
    }
    else {
        Write-Host "    '$Role' on $Scope already granted." -ForegroundColor DarkGray
    }
}

function Set-DeployEnvironmentVariable {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Value,
        [Parameter(Mandatory)][string]$EnvironmentName,
        [Parameter(Mandatory)][string]$Repo
    )
    gh variable set $Name --env $EnvironmentName --body $Value --repo $Repo | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "gh variable set failed for '$Name' in environment '$EnvironmentName'." }
}

# ──────────────────────────────────────────────
#  Resolve repo owner/name from `origin`, unless overridden
# ──────────────────────────────────────────────
if (-not $RepoOwner -or -not $RepoName) {
    $repoInfo = gh repo view --json owner,name | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw "gh repo view failed — is 'origin' set to your published repo?" }
    if (-not $RepoOwner) { $RepoOwner = $repoInfo.owner.login }
    if (-not $RepoName) { $RepoName = $repoInfo.name }
}
$repoFullName = "$RepoOwner/$RepoName"
Write-Host "Repo: $repoFullName" -ForegroundColor Cyan

if (-not $ProdReviewerLogin) {
    $currentUser = gh api user | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw "gh api user failed — run 'gh auth login' first." }
    $ProdReviewerLogin = $currentUser.login
}
$reviewerId = (gh api "users/$ProdReviewerLogin" | ConvertFrom-Json).id
if ($LASTEXITCODE -ne 0) { throw "Could not resolve GitHub user '$ProdReviewerLogin' for the prod required-reviewer rule." }
Write-Host "prod required reviewer: $ProdReviewerLogin (id $reviewerId)" -ForegroundColor Cyan

# ──────────────────────────────────────────────
#  Resolve Azure context
# ──────────────────────────────────────────────
$account = az account show | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw "az account show failed — run 'az login' and select the workshop subscription first." }
$subscriptionId = $account.id
$tenantId = $account.tenantId
Write-Host "Subscription: $($account.name) ($subscriptionId)" -ForegroundColor Cyan

$hubRgExists = az group exists --name $HubResourceGroupName
if ($hubRgExists -ne 'true') { throw "Hub resource group '$HubResourceGroupName' does not exist." }
$hubRgId = az group show --name $HubResourceGroupName --query id -o tsv

$null = az network vnet show --resource-group $HubResourceGroupName --name $HubVnetName --query id -o tsv
if ($LASTEXITCODE -ne 0) { throw "Hub VNet '$HubVnetName' not found in '$HubResourceGroupName'." }

foreach ($environmentName in $Environments) {
    Write-Host "`n=== Environment: $environmentName ===" -ForegroundColor Yellow
    $cfg = $environmentMap[$environmentName]
    $workloadRgName = $cfg.ResourceGroupName
    $location = $cfg.Location

    $workloadRgExists = az group exists --name $workloadRgName
    if ($workloadRgExists -ne 'true') {
        throw "Workload resource group '$workloadRgName' does not exist. Deploy the spoke and workload for '$environmentName' first."
    }
    $workloadRgId = az group show --name $workloadRgName --query id -o tsv

    # 1. Deploy identity (separate from the runtime identity on the container apps)
    $identityName = "id-cicd-$WorkloadName-$environmentName-$location-001"
    Write-Host "  Identity: $identityName" -ForegroundColor Cyan
    $identityJson = az identity show --name $identityName --resource-group $workloadRgName 2>$null
    if ($LASTEXITCODE -eq 0 -and $identityJson) {
        $identity = $identityJson | ConvertFrom-Json
        Write-Host "    Already exists." -ForegroundColor DarkGray
    }
    else {
        Write-Host "    Creating..." -ForegroundColor DarkGray
        $identity = az identity create --name $identityName --resource-group $workloadRgName --location $location | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0) { throw "az identity create failed for '$identityName'." }
    }
    $principalId = $identity.principalId
    $clientId = $identity.clientId

    # 2. Role assignments — Contributor on its own workload RG, Network Contributor on the
    #    hub RG (see .NOTES above for why the hub RG and not just the VNet resource).
    Grant-RoleAssignmentIfMissing -PrincipalId $principalId -Role 'Contributor' -Scope $workloadRgId
    Grant-RoleAssignmentIfMissing -PrincipalId $principalId -Role 'Network Contributor' -Scope $hubRgId

    # 3. Federated credential — subject generated from variables, never hand-typed.
    $subject = "repo:${repoFullName}:environment:$environmentName"
    $credentialName = "github-actions-$environmentName"
    Write-Host "  Federated credential subject: $subject" -ForegroundColor DarkGray
    $existingCredJson = az identity federated-credential show --identity-name $identityName --resource-group $workloadRgName --name $credentialName 2>$null
    if ($LASTEXITCODE -eq 0 -and $existingCredJson) {
        az identity federated-credential update `
            --identity-name $identityName --resource-group $workloadRgName --name $credentialName `
            --issuer 'https://token.actions.githubusercontent.com' --subject $subject --audiences 'api://AzureADTokenExchange' `
            --output none
    }
    else {
        az identity federated-credential create `
            --identity-name $identityName --resource-group $workloadRgName --name $credentialName `
            --issuer 'https://token.actions.githubusercontent.com' --subject $subject --audiences 'api://AzureADTokenExchange' `
            --output none
    }
    if ($LASTEXITCODE -ne 0) { throw "Federated credential create/update failed for '$credentialName' on '$identityName'." }

    # 4. GitHub Environment — PUT fully replaces protection rules each time, so this is a
    #    true idempotent upsert: prod always ends up with exactly the one required reviewer,
    #    test always ends up with none.
    $envBody = if ($environmentName -eq 'prod') {
        @{ reviewers = @(@{ type = 'User'; id = $reviewerId }) }
    }
    else {
        @{ reviewers = @() }
    }
    $envBodyJson = $envBody | ConvertTo-Json -Depth 5 -Compress
    $envBodyJson | gh api "repos/$repoFullName/environments/$environmentName" -X PUT --input - | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "gh api PUT environments/$environmentName failed." }
    Write-Host "  GitHub Environment '$environmentName' created/updated." -ForegroundColor DarkGray

    # 5. Environment variables — not secrets, safe to appear in logs.
    Set-DeployEnvironmentVariable -Name 'AZURE_CLIENT_ID' -Value $clientId -EnvironmentName $environmentName -Repo $repoFullName
    Set-DeployEnvironmentVariable -Name 'AZURE_TENANT_ID' -Value $tenantId -EnvironmentName $environmentName -Repo $repoFullName
    Set-DeployEnvironmentVariable -Name 'AZURE_SUBSCRIPTION_ID' -Value $subscriptionId -EnvironmentName $environmentName -Repo $repoFullName
    Set-DeployEnvironmentVariable -Name 'AZURE_RESOURCE_GROUP' -Value $workloadRgName -EnvironmentName $environmentName -Repo $repoFullName
    Write-Host "  Variables published." -ForegroundColor DarkGray

    Write-Host "Done: $environmentName." -ForegroundColor Green
}

Write-Host "`nBootstrap complete." -ForegroundColor Green
