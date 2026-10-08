targetScope = 'resourceGroup'

// Single, resource-group-scoped entry point for the CI/CD pipeline. It deploys one complete
// environment — spoke VNet + hub peering, then the whole workload (Container Apps, Azure SQL,
// managed identities, private endpoint, Private DNS, Monitor) — by composing the AVM spoke VNet
// module with the existing infra/workload/main.bicep, unchanged.
//
// The workload resource group itself is created once by infra/spoke-network (subscription
// scope); the per-environment deploy identity only holds Contributor on that existing group
// (plus Network Contributor on the hub group), so this template deliberately stays at
// resource-group scope. `test` and `prod` deploy from this exact file — every difference is a
// value in main.<environment>.bicepparam, never a branch here.

@description('Azure region for the spoke VNet and all workload resources.')
param location string = 'belgiumcentral'

@description('Azure region for Application Insights (Belgium Central does not support `Microsoft.Insights/components` yet).')
param monitorLocation string = 'swedencentral'

@minLength(3)
@maxLength(20)
@description('Short, descriptive name of the workload (used in resource names).')
param workloadName string = 'hotelbooking'

@minLength(2)
@maxLength(10)
@allowed(['test', 'prod'])
@description('Environment token embedded in resource names.')
param environment string

@minLength(1)
@maxLength(90)
@description('Name of the resource group containing the existing hub VNet (read-only reference).')
param hubResourceGroupName string = 'rg-platform'

@minLength(2)
@maxLength(64)
@description('Name of the existing hub VNet.')
param hubVnetName string = 'vnet-hub'

@description('Address space for the spoke VNet. Must not overlap the hub or any other spoke.')
param spokeAddressPrefix string

@description('Address prefix for the private endpoint subnet, carved out of the spoke address space.')
param privateEndpointSubnetPrefix string

@description('Name of the spoke-side peering resource (on the spoke VNet).')
param spokeSidePeeringName string

@description('Name of the hub-side (remote) peering resource. Must be unique per spoke on the shared hub VNet.')
param hubSidePeeringName string

@description('Address prefix for the Container Apps infrastructure subnet, carved out of the spoke address space.')
param containerAppsSubnetAddressPrefix string

@description('Whether the Container Apps environment and the SQL database are zone-redundant.')
param zoneRedundant bool

@description('Whether this environment\'s SQL Private DNS zone also links to the hub VNet (only one environment can, per Azure\'s same-namespace zone-link rule).')
param linkPrivateDnsZoneToHub bool

@description('Container image tag for both apps.')
param imageTag string = 'latest'

@description('Backend container image repository (public GHCR, anonymous pull).')
param backendImageRepository string

@description('Frontend container image repository (public GHCR, anonymous pull).')
param frontendImageRepository string

@description('Minimum replica count for both container apps (0 = scale to zero).')
param containerAppMinReplicas int

@description('Maximum replica count for both container apps.')
param containerAppMaxReplicas int

@description('CPU cores allocated to each container.')
param containerCpu string = '0.25'

@description('Memory allocated to each container.')
param containerMemory string = '0.5Gi'

@description('SQL Database SKU name.')
param sqlDatabaseSkuName string = 'GP_S_Gen5'

@description('SQL Database SKU tier.')
param sqlDatabaseSkuTier string = 'GeneralPurpose'

@description('SQL Database SKU family.')
param sqlDatabaseSkuFamily string = 'Gen5'

@description('SQL Database vCore capacity (serverless max vCores).')
param sqlDatabaseCapacity int

@description('SQL Database serverless minimum capacity (vCores, fractional allowed).')
param sqlDatabaseMinCapacity string

@description('Minutes of inactivity before the serverless database auto-pauses (-1 disables auto-pause).')
param sqlDatabaseAutoPauseDelay int

@description('Log Analytics workspace retention, in days.')
param logAnalyticsRetentionDays int

@description('Tags applied to the spoke VNet and all workload resources.')
param tags object = {
  workload: workloadName
  environment: environment
  role: 'workload'
}

var spokeVnetName = 'vnet-${workloadName}-${environment}-${location}-001'

resource hubVnet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: hubVnetName
  scope: resourceGroup(hubResourceGroupName)
}

// Subnet layout inside the spoke address space:
//   snet-private-endpoints   first /26   - private endpoints for PaaS services
//   snet-containerapps       /27         - added by the workload template (delegated)
module spokeVnet 'br/public:avm/res/network/virtual-network:0.10.2' = {
  name: 'spoke-vnet-${location}'
  params: {
    name: spokeVnetName
    location: location
    addressPrefixes: [
      spokeAddressPrefix
    ]
    subnets: [
      {
        name: 'snet-private-endpoints'
        addressPrefix: privateEndpointSubnetPrefix
        privateEndpointNetworkPolicies: 'Disabled'
      }
    ]
    peerings: [
      {
        name: spokeSidePeeringName
        remoteVirtualNetworkResourceId: hubVnet.id
        allowVirtualNetworkAccess: true
        allowForwardedTraffic: true
        allowGatewayTransit: false
        useRemoteGateways: false
        remotePeeringEnabled: true
        remotePeeringName: hubSidePeeringName
        remotePeeringAllowVirtualNetworkAccess: true
        remotePeeringAllowForwardedTraffic: true
        remotePeeringAllowGatewayTransit: false
        remotePeeringUseRemoteGateways: false
      }
    ]
    tags: tags
    enableTelemetry: false
  }
}

module workload 'workload/main.bicep' = {
  name: 'workload-${environment}-${location}'
  params: {
    location: location
    monitorLocation: monitorLocation
    workloadName: workloadName
    environment: environment
    zoneRedundant: zoneRedundant
    tags: tags
    // Consuming the spoke module's output makes the workload deployment wait for the VNet.
    spokeVnetName: spokeVnet.outputs.name
    hubResourceGroupName: hubResourceGroupName
    hubVnetName: hubVnetName
    containerAppsSubnetAddressPrefix: containerAppsSubnetAddressPrefix
    imageTag: imageTag
    backendImageRepository: backendImageRepository
    frontendImageRepository: frontendImageRepository
    containerAppMinReplicas: containerAppMinReplicas
    containerAppMaxReplicas: containerAppMaxReplicas
    containerCpu: containerCpu
    containerMemory: containerMemory
    sqlDatabaseSkuName: sqlDatabaseSkuName
    sqlDatabaseSkuTier: sqlDatabaseSkuTier
    sqlDatabaseSkuFamily: sqlDatabaseSkuFamily
    sqlDatabaseCapacity: sqlDatabaseCapacity
    sqlDatabaseMinCapacity: sqlDatabaseMinCapacity
    sqlDatabaseAutoPauseDelay: sqlDatabaseAutoPauseDelay
    linkPrivateDnsZoneToHub: linkPrivateDnsZoneToHub
    logAnalyticsRetentionDays: logAnalyticsRetentionDays
  }
}

@description('Name of the spoke VNet.')
output spokeVnetName string = spokeVnet.outputs.name
