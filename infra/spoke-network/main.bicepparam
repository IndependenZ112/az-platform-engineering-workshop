using 'main.bicep'

param location = 'swedencentral'
param workloadName = 'hotelbooking'
param environment = 'test'
param hubResourceGroupName = 'rg-platform'
param hubVnetName = 'vnet-hub'
param spokeAddressPrefix = '192.168.101.0/24'
param privateEndpointSubnetPrefix = '192.168.101.0/26'
