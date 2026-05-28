targetScope = 'resourceGroup'

@description('Azure region for the hub resources.')
param location string = resourceGroup().location

@description('Name of the hub virtual network.')
param hubVnetName string = 'vnet-hub'

@description('Address space for the hub VNet.')
param hubVnetAddressPrefix string = '192.168.100.0/24'

@description('Tags applied to all resources.')
param tags object = {
  workload: 'platform'
  environment: 'workshop'
  role: 'hub'
}

// Subnet layout inside 192.168.100.0/24:
//   AzureFirewallSubnet   192.168.100.0/26    (.0   - .63)   - /26 is the required minimum
//   GatewaySubnet         192.168.100.64/27   (.64  - .95)   - /27 fits VPN/ER gateways comfortably
//   snet-shared-services  192.168.100.96/27   (.96  - .127)  - placeholder for shared platform services
//   (192.168.100.128/25 left free for growth)

resource hubVnet 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: hubVnetName
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        hubVnetAddressPrefix
      ]
    }
    subnets: [
      {
        name: 'AzureFirewallSubnet'
        properties: {
          addressPrefix: '192.168.100.0/26'
        }
      }
      {
        name: 'GatewaySubnet'
        properties: {
          addressPrefix: '192.168.100.64/27'
        }
      }
      {
        name: 'snet-shared-services'
        properties: {
          addressPrefix: '192.168.100.96/27'
        }
      }
    ]
  }
}

output hubVnetId string = hubVnet.id
output hubVnetName string = hubVnet.name
output hubVnetAddressSpace string = hubVnetAddressPrefix
