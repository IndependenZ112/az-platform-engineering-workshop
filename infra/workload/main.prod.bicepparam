using 'main.bicep'

param location = 'belgiumcentral'
param monitorLocation = 'swedencentral'
param workloadName = 'hotelbooking'
param environment = 'prod'
param spokeVnetName = 'vnet-hotelbooking-prod-belgiumcentral-001'
param hubResourceGroupName = 'rg-platform'
param hubVnetName = 'vnet-hub'
param containerAppsSubnetAddressPrefix = '192.168.102.64/27'
param zoneRedundant = true
// The hub is already linked to test's privatelink.database.windows.net zone, and Azure
// forbids linking one VNet to two zones sharing the same namespace. prod's spoke link is
// sufficient — nothing in the hub needs to resolve prod's SQL private endpoint.
param linkPrivateDnsZoneToHub = false

// Pinned to the public workshop images' `latest` tag for this first prod deployment, matching
// the chore's bootstrap-deploy convention; pin to an immutable commit SHA for subsequent
// deployments.
param imageTag = 'latest'
param backendImageRepository = 'ghcr.io/azureholic/az-platform-engineering-workshop/backend'
param frontendImageRepository = 'ghcr.io/azureholic/az-platform-engineering-workshop/frontend'

// Never scales to zero; at least 3 replicas, spread across availability zones by the
// zone-redundant Container Apps environment above.
param containerAppMinReplicas = 3
param containerAppMaxReplicas = 10
param containerCpu = '0.25'
param containerMemory = '0.5Gi'

// Still serverless (same AVM code path as test) — just a higher capacity ceiling and no
// auto-pause. Serverless General Purpose supports zone redundancy, so there's no need to fork
// onto a provisioned SKU family for prod.
param sqlDatabaseSkuName = 'GP_S_Gen5'
param sqlDatabaseSkuTier = 'GeneralPurpose'
param sqlDatabaseSkuFamily = 'Gen5'
param sqlDatabaseCapacity = 2
param sqlDatabaseMinCapacity = '1'
param sqlDatabaseAutoPauseDelay = -1

param logAnalyticsRetentionDays = 90
