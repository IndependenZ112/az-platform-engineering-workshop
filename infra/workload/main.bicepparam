using 'main.bicep'

param location = 'swedencentral'
param workloadName = 'hotelbooking'
param environment = 'test'
param spokeVnetName = 'vnet-hotelbooking-test-swedencentral-001'
param hubResourceGroupName = 'rg-platform'
param hubVnetName = 'vnet-hub'
param containerAppsSubnetAddressPrefix = '192.168.101.64/27'

// First deployment uses the public workshop images' `latest` tag (per chore requirements).
// Pin to an immutable commit SHA for subsequent deployments.
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
