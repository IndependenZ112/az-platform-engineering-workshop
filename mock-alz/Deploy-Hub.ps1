#requires -Version 7.0
<#
.SYNOPSIS
    Deploys the mock landing zone hub VNet into rg-platform.

.DESCRIPTION
    Creates (or updates) the platform resource group and deploys hub.bicep into it
    using the Azure CLI. Run from PowerShell 7+ with az CLI already logged in
    (`az login`) and the correct subscription selected (`az account set`).
#>

[CmdletBinding()]
param(
    [string]$ResourceGroupName = 'rg-platform',
    [string]$Location = 'swedencentral',
    [string]$DeploymentName = "hub-$(Get-Date -Format 'yyyyMMddHHmmss')"
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptRoot 'hub.bicep'

Write-Host "Using subscription:" -ForegroundColor Cyan
az account show --query '{name:name, id:id}' -o table

Write-Host "Ensuring resource group '$ResourceGroupName' exists in '$Location'..." -ForegroundColor Cyan
az group create --name $ResourceGroupName --location $Location --output none

Write-Host "Deploying hub VNet ($DeploymentName)..." -ForegroundColor Cyan
az deployment group create `
    --resource-group $ResourceGroupName `
    --name $DeploymentName `
    --template-file $templateFile `
    --parameters location=$Location `
    --output table

Write-Host "Done." -ForegroundColor Green
