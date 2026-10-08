using 'main.bicep'

// Parameter set for the pipeline's `prod` stage. Values mirror — and must stay in sync with —
// infra/spoke-network/main.prod.bicepparam and infra/workload/main.prod.bicepparam, which
// describe the same already-deployed environment.

param location = 'belgiumcentral'
param monitorLocation = 'swedencentral'
param workloadName = 'hotelbooking'
param environment = 'prod'
param hubResourceGroupName = 'rg-platform'
param hubVnetName = 'vnet-hub'

param spokeAddressPrefix = '192.168.102.0/24'
param privateEndpointSubnetPrefix = '192.168.102.0/26'
param containerAppsSubnetAddressPrefix = '192.168.102.64/27'
// Must be unique on the hub side across every spoke — `test` already owns 'peer-hub-to-spoke'.
param spokeSidePeeringName = 'peer-spoke-prod-to-hub'
param hubSidePeeringName = 'peer-hub-to-spoke-prod'

param zoneRedundant = true
// The hub is already linked to test's privatelink.database.windows.net zone, and Azure
// forbids linking one VNet to two zones sharing the same namespace.
param linkPrivateDnsZoneToHub = false

param imageTag = 'latest'
param backendImageRepository = 'ghcr.io/azureholic/az-platform-engineering-workshop/backend'
param frontendImageRepository = 'ghcr.io/azureholic/az-platform-engineering-workshop/frontend'

// Never scales to zero; at least 3 replicas, spread across zones by the zone-redundant environment.
param containerAppMinReplicas = 3
param containerAppMaxReplicas = 10
param containerCpu = '0.25'
param containerMemory = '0.5Gi'

param sqlDatabaseSkuName = 'GP_S_Gen5'
param sqlDatabaseSkuTier = 'GeneralPurpose'
param sqlDatabaseSkuFamily = 'Gen5'
param sqlDatabaseCapacity = 2
param sqlDatabaseMinCapacity = '1'
param sqlDatabaseAutoPauseDelay = -1

param logAnalyticsRetentionDays = 90
