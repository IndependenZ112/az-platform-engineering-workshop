using 'main.bicep'

param location = 'belgiumcentral'
param workloadName = 'hotelbooking'
param environment = 'prod'
param hubResourceGroupName = 'rg-platform'
param hubVnetName = 'vnet-hub'
param spokeAddressPrefix = '192.168.102.0/24'
param privateEndpointSubnetPrefix = '192.168.102.0/26'
// Must be unique on the hub side across every spoke — `test` already owns 'peer-hub-to-spoke'.
param spokeSidePeeringName = 'peer-spoke-prod-to-hub'
param hubSidePeeringName = 'peer-hub-to-spoke-prod'
