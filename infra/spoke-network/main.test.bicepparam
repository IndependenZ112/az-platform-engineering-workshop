using 'main.bicep'

param location = 'belgiumcentral'
param workloadName = 'hotelbooking'
param environment = 'test'
param hubResourceGroupName = 'rg-platform'
param hubVnetName = 'vnet-hub'
param spokeAddressPrefix = '192.168.101.0/24'
param privateEndpointSubnetPrefix = '192.168.101.0/26'
// Pinned to the exact names already deployed for `test` — must never change, or redeploying
// `test` would create a second, orphaned peering alongside the existing one.
param spokeSidePeeringName = 'peer-spoke-to-hub'
param hubSidePeeringName = 'peer-hub-to-spoke'
