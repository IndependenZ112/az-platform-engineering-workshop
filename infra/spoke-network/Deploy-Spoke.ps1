#requires -Version 7.0
<#
.SYNOPSIS
    Deploys a workload spoke VNet (test or prod) and peers it to the hub.

.DESCRIPTION
    Deploys infra/spoke-network/main.bicep at subscription scope using the Azure CLI, with the
    parameter file selected by `-Environment`. Creates the workload resource group, the spoke
    VNet, and the bidirectional peering to the existing hub VNet in rg-platform. `test` and
    `prod` use the same template; only the parameter file (and therefore region, address space,
    and resource group name) differs. Run from PowerShell 7+ with az CLI already logged in
    (`az login`) and the correct subscription selected (`az account set`).

.PARAMETER Environment
    Which environment to deploy: `test` or `prod`. Selects `main.<Environment>.bicepparam`.

.PARAMETER Location
    Azure region for the deployment (subscription-level `--location` used for the deployment
    metadata; resource location comes from the Bicep parameters). Defaults per environment:
    `test` → `belgiumcentral`, `prod` → `swedencentral`.

.PARAMETER DeploymentName
    Name of the subscription-scoped deployment.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('test', 'prod')]
    [string]$Environment,

    [string]$Location,

    [string]$DeploymentName = "spoke-network-$Environment-$(Get-Date -Format 'yyyyMMddHHmmss')"
)

$ErrorActionPreference = 'Stop'

$defaultLocations = @{
    test = 'belgiumcentral'
    prod = 'belgiumcentral'
}
if (-not $Location) {
    $Location = $defaultLocations[$Environment]
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptRoot 'main.bicep'
$parameterFile = Join-Path $scriptRoot "main.$Environment.bicepparam"

Write-Host "Using subscription:" -ForegroundColor Cyan
az account show --query '{name:name, id:id}' -o table

Write-Host "Deploying spoke network for '$Environment' ($DeploymentName)..." -ForegroundColor Cyan
az deployment sub create `
    --location $Location `
    --name $DeploymentName `
    --template-file $templateFile `
    --parameters $parameterFile `
    --output table

Write-Host "Done." -ForegroundColor Green
