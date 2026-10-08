targetScope = 'subscription'

@description('Azure region for the workload resource group and spoke VNet.')
param location string = 'belgiumcentral'

@minLength(3)
@maxLength(20)
@description('Short, descriptive name of the workload (used in resource names).')
param workloadName string = 'hotelbooking'

@minLength(2)
@maxLength(10)
@description('Environment token embedded in resource names.')
param environment string = 'test'

@minLength(1)
@maxLength(90)
@description('Name of the resource group containing the existing hub VNet. Deployed by mock-alz/Deploy-Hub.ps1 and treated as read-only.')
param hubResourceGroupName string = 'rg-platform'

@minLength(2)
@maxLength(64)
@description('Name of the existing hub VNet.')
param hubVnetName string = 'vnet-hub'

@description('Address space for the spoke VNet. Must not overlap the hub (192.168.100.0/24).')
param spokeAddressPrefix string = '192.168.101.0/24'

@description('Address prefix for the private endpoint subnet, carved out of the spoke address space.')
param privateEndpointSubnetPrefix string = '192.168.101.0/26'

@description('Name of the spoke-side peering resource (on the spoke VNet). Each spoke peering to the shared hub needs its own unique name on the hub side (see `hubSidePeeringName`) — this is an explicit parameter, not derived from the spoke name, specifically so an already-deployed environment\'s peering name never changes when a new environment is added.')
param spokeSidePeeringName string = 'peer-spoke-to-hub'

@description('Name of the hub-side (remote) peering resource, created on the existing hub VNet. Must be unique per spoke — the hub VNet accumulates one peering per environment, and a literal, non-unique name here would make a second spoke\'s deployment try to change an existing peering\'s remote VNet in place, which Azure rejects (`ChangingRemoteVirtualNetworkNotAllowed`).')
param hubSidePeeringName string = 'peer-hub-to-spoke'

@description('Tags applied to the workload resource group and its resources.')
param tags object = {
  workload: workloadName
  environment: environment
  role: 'workload'
}

// Includes the region token so the resource group name stays unique across regions — this
// lets a spoke be torn down in one region and (re)created in another without name collisions
// while the old resource group is still finishing its (slow) async deletion.
var workloadResourceGroupName = 'rg-${workloadName}-${environment}-${location}'
var spokeVnetName = 'vnet-${workloadName}-${environment}-${location}-001'

// The hub VNet already exists (deployed by mock-alz/Deploy-Hub.ps1); reference it instead of
// reconstructing its resource ID, per repo Bicep conventions.
resource hubVnet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: hubVnetName
  scope: resourceGroup(hubResourceGroupName)
}

module workloadResourceGroup 'br/public:avm/res/resources/resource-group:0.4.4' = {
  // Deployment names are tracked (with their location) in the subscription's deployment
  // history; including `location` keeps re-deploys to a different region from colliding with
  // an old deployment name still on record for a prior region.
  name: 'workload-resource-group-${location}'
  params: {
    name: workloadResourceGroupName
    location: location
    tags: tags
    enableTelemetry: false
  }
}

// Subnet layout inside 192.168.101.0/24:
//   snet-private-endpoints   192.168.101.0/26    (.0  - .63)   - private endpoints for PaaS services
//   (192.168.101.64/26 and 192.168.101.128/25 left free for app/data subnets in later chores)
module spokeVnet 'br/public:avm/res/network/virtual-network:0.10.2' = {
  name: 'spoke-vnet-${location}'
  scope: resourceGroup(workloadResourceGroupName)
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
  // `scope: resourceGroup(...)` targets the RG by name rather than a symbolic reference, so the
  // dependency on its creation must be declared explicitly.
  dependsOn: [
    workloadResourceGroup
  ]
}

@description('Resource ID of the workload resource group.')
output workloadResourceGroupId string = workloadResourceGroup.outputs.resourceId

@description('Name of the workload resource group.')
output workloadResourceGroupName string = workloadResourceGroup.outputs.name

@description('Resource ID of the spoke VNet.')
output spokeVnetId string = spokeVnet.outputs.resourceId

@description('Name of the spoke VNet.')
output spokeVnetName string = spokeVnet.outputs.name

@description('Address space of the spoke VNet.')
output spokeAddressSpace string = spokeAddressPrefix

@description('Resource ID of the private endpoint subnet, for use by later chores.')
output privateEndpointSubnetId string = spokeVnet.outputs.subnetResourceIds[0]
