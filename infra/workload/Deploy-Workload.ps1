#requires -Version 7.0
<#
.SYNOPSIS
    Deploys the Hotel Booking workload into the existing workload spoke.

.DESCRIPTION
    Deploys infra/workload/main.bicep into an existing resource group using the Azure CLI.
    Every run performs a preflight pass first — Bicep build (syntax), a what-if analysis, and
    an implicit permission check (the `Provider` validation level fails with an authorization
    error if the signed-in principal lacks the rights to deploy what's in the template) — and
    prints a clear pass/fail summary. The actual deployment only runs when `-Deploy` is passed;
    by default this script is preflight-only, so it's safe to run repeatedly (including as the
    verification step for a design-only chore).

    Run from PowerShell 7+ with az CLI already logged in (`az login`) and the correct
    subscription selected (`az account set`).

.PARAMETER ResourceGroupName
    Name of the existing workload resource group to deploy into.

.PARAMETER Location
    Azure region used for the subscription-level `--location` metadata of the deployment
    record (resource location itself comes from the Bicep parameters).

.PARAMETER DeploymentName
    Name of the resource-group-scoped deployment.

.PARAMETER Deploy
    If set, runs the actual `az deployment group create` after a successful preflight. Omit
    this switch to run preflight only (the default, safe behaviour).
#>

[CmdletBinding()]
param(
    [string]$ResourceGroupName = 'rg-hotelbooking-test',
    [string]$Location = 'swedencentral',
    [string]$DeploymentName = "workload-$(Get-Date -Format 'yyyyMMddHHmmss')",
    [switch]$Deploy
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptRoot 'main.bicep'
$parameterFile = Join-Path $scriptRoot 'main.bicepparam'

Write-Host "Using subscription:" -ForegroundColor Cyan
az account show --query '{name:name, id:id}' -o table

Write-Host "`nChecking resource group '$ResourceGroupName' exists..." -ForegroundColor Cyan
$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -ne 'true') {
    throw "Resource group '$ResourceGroupName' does not exist. It is created by the spoke-network deployment (infra/spoke-network) — run that first."
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

Write-Host "`n--- Deploying ($DeploymentName) ---" -ForegroundColor Cyan
az deployment group create `
    --resource-group $ResourceGroupName `
    --name $DeploymentName `
    --template-file $templateFile `
    --parameters $parameterFile `
    --output table

Write-Host "Done." -ForegroundColor Green
