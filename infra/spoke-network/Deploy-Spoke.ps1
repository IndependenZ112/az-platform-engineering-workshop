#requires -Version 7.0
<#
.SYNOPSIS
    Deploys the workload spoke VNet and peers it to the hub.

.DESCRIPTION
    Deploys infra/spoke-network/main.bicep at subscription scope using the Azure CLI. Creates
    the workload resource group, the spoke VNet, and the bidirectional peering to the existing
    hub VNet in rg-platform. Run from PowerShell 7+ with az CLI already logged in
    (`az login`) and the correct subscription selected (`az account set`).

.PARAMETER Location
    Azure region for the deployment (subscription-level `--location` used for the deployment
    metadata; resource location comes from the Bicep parameters).

.PARAMETER DeploymentName
    Name of the subscription-scoped deployment.
#>

[CmdletBinding()]
param(
    [string]$Location = 'swedencentral',
    [string]$DeploymentName = "spoke-network-$(Get-Date -Format 'yyyyMMddHHmmss')"
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptRoot 'main.bicep'
$parameterFile = Join-Path $scriptRoot 'main.bicepparam'

Write-Host "Using subscription:" -ForegroundColor Cyan
az account show --query '{name:name, id:id}' -o table

Write-Host "Deploying spoke network ($DeploymentName)..." -ForegroundColor Cyan
az deployment sub create `
    --location $Location `
    --name $DeploymentName `
    --template-file $templateFile `
    --parameters $parameterFile `
    --output table

Write-Host "Done." -ForegroundColor Green
