using 'main.bicep'

// Parameter set for the pipeline's `test` stage. Values mirror — and must stay in sync with —
// infra/spoke-network/main.test.bicepparam and infra/workload/main.test.bicepparam, which
// describe the same already-deployed environment.

param location = 'belgiumcentral'
param monitorLocation = 'swedencentral'
param workloadName = 'hotelbooking'
param environment = 'test'
param hubResourceGroupName = 'rg-platform'
param hubVnetName = 'vnet-hub'

param spokeAddressPrefix = '192.168.101.0/24'
param privateEndpointSubnetPrefix = '192.168.101.0/26'
param containerAppsSubnetAddressPrefix = '192.168.101.64/27'
// Pinned to the exact names already deployed for `test` — must never change.
param spokeSidePeeringName = 'peer-spoke-to-hub'
param hubSidePeeringName = 'peer-hub-to-spoke'

param zoneRedundant = false
param linkPrivateDnsZoneToHub = true

param imageTag = 'latest'
param backendImageRepository = 'ghcr.io/azureholic/az-platform-engineering-workshop/backend'
param frontendImageRepository = 'ghcr.io/azureholic/az-platform-engineering-workshop/frontend'

param containerAppMinReplicas = 0
param containerAppMaxReplicas = 3
param containerCpu = '0.25'
param containerMemory = '0.5Gi'

param sqlDatabaseSkuName = 'GP_S_Gen5'
param sqlDatabaseSkuTier = 'GeneralPurpose'
param sqlDatabaseSkuFamily = 'Gen5'
param sqlDatabaseCapacity = 1
param sqlDatabaseMinCapacity = '0.5'
param sqlDatabaseAutoPauseDelay = 60

param logAnalyticsRetentionDays = 30
