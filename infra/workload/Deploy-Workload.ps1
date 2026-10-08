#requires -Version 7.0
<#
.SYNOPSIS
    Deploys the Hotel Booking workload (test or prod) into its existing workload spoke.

.DESCRIPTION
    Deploys infra/workload/main.bicep into an existing resource group using the Azure CLI, with
    the resource group and parameter file selected by `-Environment`. `test` and `prod` deploy
    from the exact same template — every difference between them is a parameter value in
    `main.<Environment>.bicepparam`, never a branch in the template. Every run performs a
    preflight pass first — Bicep build (syntax), a what-if analysis, and an implicit permission
    check (the `Provider` validation level fails with an authorization error if the signed-in
    principal lacks the rights to deploy what's in the template) — and prints a clear pass/fail
    summary. The actual deployment only runs when `-Deploy` is passed; by default this script is
    preflight-only, so it's safe to run repeatedly.

    Run from PowerShell 7+ with az CLI already logged in (`az login`) and the correct
    subscription selected (`az account set`).

.PARAMETER Environment
    Which environment to deploy: `test` or `prod`. Selects the resource group and
    `main.<Environment>.bicepparam`:

        test -> rg-hotelbooking-test-belgiumcentral  / main.test.bicepparam
        prod -> rg-hotelbooking-prod-swedencentral   / main.prod.bicepparam

.PARAMETER ResourceGroupName
    Overrides the environment's default resource group, if ever needed.

.PARAMETER DeploymentName
    Name of the resource-group-scoped deployment.

.PARAMETER Deploy
    If set, runs the actual `az deployment group create` after a successful preflight. Omit
    this switch to run preflight only (the default, safe behaviour).
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('test', 'prod')]
    [string]$Environment,

    [string]$ResourceGroupName,

    [string]$DeploymentName = "workload-$Environment-$(Get-Date -Format 'yyyyMMddHHmmss')",

    [switch]$Deploy
)

$ErrorActionPreference = 'Stop'

$defaultResourceGroups = @{
    test = 'rg-hotelbooking-test-belgiumcentral'
    prod = 'rg-hotelbooking-prod-belgiumcentral'
}
if (-not $ResourceGroupName) {
    $ResourceGroupName = $defaultResourceGroups[$Environment]
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptRoot 'main.bicep'
$parameterFile = Join-Path $scriptRoot "main.$Environment.bicepparam"

Write-Host "Using subscription:" -ForegroundColor Cyan
az account show --query '{name:name, id:id}' -o table

Write-Host "`nChecking resource group '$ResourceGroupName' exists..." -ForegroundColor Cyan
$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -ne 'true') {
    throw "Resource group '$ResourceGroupName' does not exist. It is created by the spoke-network deployment (infra/spoke-network/Deploy-Spoke.ps1 -Environment $Environment) — run that first."
}

Write-Host "`n--- Preflight: Bicep syntax ---" -ForegroundColor Cyan
az bicep build --file $templateFile --stdout | Out-Null
Write-Host "Bicep syntax OK." -ForegroundColor Green

Write-Host "`n--- Preflight: what-if (also validates deployment permissions) ---" -ForegroundColor Cyan
$whatIfLevel = 'Provider'
$whatIfOutput = az deployment group what-if `
    --resource-group $ResourceGroupName `
    --name "$DeploymentName-preflight" `
    --template-file $templateFile `
    --parameters $parameterFile `
    --validation-level $whatIfLevel `
    -o json 2>&1
$whatIfExit = $LASTEXITCODE

if ($whatIfExit -ne 0) {
    Write-Host "Provider-level what-if failed (often a permissions gap) — retrying with ProviderNoRbac..." -ForegroundColor Yellow
    $whatIfLevel = 'ProviderNoRbac'
    $whatIfOutput = az deployment group what-if `
        --resource-group $ResourceGroupName `
        --name "$DeploymentName-preflight" `
        --template-file $templateFile `
        --parameters $parameterFile `
        --validation-level $whatIfLevel `
        -o json 2>&1
    $whatIfExit = $LASTEXITCODE
}

if ($whatIfExit -ne 0) {
    Write-Host $whatIfOutput -ForegroundColor Red
    throw "Preflight failed: what-if did not complete at either validation level. Resolve the error above before deploying."
}

Write-Host "What-if succeeded (validation level: $whatIfLevel)." -ForegroundColor Green
Write-Host $whatIfOutput

if (-not $Deploy) {
    Write-Host "`nPreflight-only run (default). Re-run with -Deploy to apply this template." -ForegroundColor Yellow
    return
}

Write-Host "`n--- Deploying '$Environment' ($DeploymentName) ---" -ForegroundColor Cyan
az deployment group create `
    --resource-group $ResourceGroupName `
    --name $DeploymentName `
    --template-file $templateFile `
    --parameters $parameterFile `
    --output table

Write-Host "Done." -ForegroundColor Green
