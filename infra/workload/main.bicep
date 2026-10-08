targetScope = 'resourceGroup'

// Deploys the Hotel Booking workload into the existing workload spoke, created in the prior
// network chore. This template only adds workload resources — it does not recreate the
// resource group or the spoke VNet.

@description('Azure region for all workload resources.')
param location string = 'belgiumcentral'

@description('Azure region for Application Insights. Kept separate from `location` because not every region supports `Microsoft.Insights/components` yet (e.g. Belgium Central does not) — Monitor resources have no VNet/private-networking coupling to the rest of the workload, so placing them in a region known to support the resource type (here, alongside the hub in Sweden Central) is a safe, well-architected fallback rather than an architectural compromise.')
param monitorLocation string = 'swedencentral'

@minLength(3)
@maxLength(20)
@description('Short, descriptive name of the workload (used in resource names).')
param workloadName string = 'hotelbooking'

@minLength(2)
@maxLength(10)
@allowed(['test', 'prod'])
@description('Environment token embedded in resource names. One template, two parameter files — every test/prod difference is a parameter value, never a branch in this template.')
param environment string = 'test'

@description('Whether the Container Apps environment and the SQL database are zone-redundant. `test` runs `false` (cost); `prod` runs `true` (reliability) — one flag drives both resources identically. A Container Apps environment\'s zone redundancy is set at creation and cannot change in place, so this only ever applies to a fresh environment.')
param zoneRedundant bool = false

@description('Tags applied to all workload resources.')
param tags object = {
  workload: workloadName
  environment: environment
  role: 'workload'
  managedBy: 'infra-pipeline'
}

@minLength(2)
@maxLength(64)
@description('Name of the existing spoke VNet (deployed by infra/spoke-network).')
param spokeVnetName string = 'vnet-hotelbooking-test-belgiumcentral-001'

@minLength(1)
@maxLength(90)
@description('Name of the resource group containing the existing hub VNet.')
param hubResourceGroupName string = 'rg-platform'

@minLength(2)
@maxLength(64)
@description('Name of the existing hub VNet.')
param hubVnetName string = 'vnet-hub'

@description('Address prefix for the new Container Apps infrastructure subnet, carved out of the spoke address space.')
param containerAppsSubnetAddressPrefix string = '192.168.101.64/27'

@description('Container image tag for both apps. Defaults to the public workshop images\' `latest` tag for the first deployment; pin to an immutable commit SHA for subsequent deployments.')
param imageTag string = 'latest'

@description('Backend container image repository (public, anonymous pull).')
param backendImageRepository string = 'ghcr.io/azureholic/az-platform-engineering-workshop/backend'

@description('Frontend container image repository (public, anonymous pull).')
param frontendImageRepository string = 'ghcr.io/azureholic/az-platform-engineering-workshop/frontend'

@description('Minimum replica count for both container apps (0 = scale to zero).')
param containerAppMinReplicas int = 0

@description('Maximum replica count for both container apps.')
param containerAppMaxReplicas int = 3

@description('CPU cores allocated to each container (Consumption workload profile minimum is 0.25).')
param containerCpu string = '0.25'

@description('Memory allocated to each container (Consumption workload profile minimum is 0.5Gi).')
param containerMemory string = '0.5Gi'

@description('SQL Database SKU name (serverless General Purpose by default — matches the scale-to-zero philosophy at the data tier for `test`).')
param sqlDatabaseSkuName string = 'GP_S_Gen5'

@description('SQL Database SKU tier.')
param sqlDatabaseSkuTier string = 'GeneralPurpose'

@description('SQL Database SKU family.')
param sqlDatabaseSkuFamily string = 'Gen5'

@description('SQL Database vCore capacity (serverless max vCores).')
param sqlDatabaseCapacity int = 1

@description('SQL Database serverless minimum capacity (vCores, fractional allowed).')
param sqlDatabaseMinCapacity string = '0.5'

@description('Minutes of inactivity before the serverless database auto-pauses (-1 disables auto-pause).')
param sqlDatabaseAutoPauseDelay int = 60

@description('Whether this environment\'s Private DNS zone also links to the hub VNet, in addition to its own spoke. Azure hard-blocks linking one VNet to two zones sharing the same namespace (`privatelink.database.windows.net`) even across resource groups — so once one environment\'s zone links to the shared hub, no other environment\'s same-named zone can. The spoke link alone is sufficient: only the backend container app (which lives in the spoke) ever needs to resolve the private endpoint\'s FQDN; nothing in the hub does. `test` keeps this `true` (its existing, already-deployed state); every environment added afterwards sets it `false`.')
param linkPrivateDnsZoneToHub bool = true

@description('Log Analytics workspace retention, in days (kept short for a `test` environment).')
param logAnalyticsRetentionDays int = 30

// ──────────────────────────────────────────────
//  Naming (CAF, `test` token embedded from day one)
// ──────────────────────────────────────────────
var containerAppsSubnetName = 'snet-containerapps'
var privateEndpointSubnetName = 'snet-private-endpoints'
var containerAppsEnvironmentName = 'cae-${workloadName}-${environment}-${location}-001'
// Container App names have a tight 32-character ARM limit (tighter than any other resource
// in this template). The `-001` instance suffix is dropped here specifically — there is only
// ever one instance of each app per environment — to leave headroom for longer region names
// (e.g. `belgiumcentral`) without truncating the workload/component tokens.
var frontendAppName = 'ca-hotelweb-${environment}-${location}'
var backendAppName = 'ca-hotelapi-${environment}-${location}'
var backendIdentityName = 'id-hotelapi-${environment}-${location}-001'
var sqlServerName = 'sql-${workloadName}-${environment}-${location}-${uniqueString(resourceGroup().id)}'
var sqlDatabaseName = 'sqldb-${workloadName}-${environment}'
var sqlPrivateEndpointName = 'pep-sql-${workloadName}-${environment}-001'
// Private DNS zone names for Private Link are fixed, well-known values (not a regional or
// cloud-specific endpoint to template with `environment()`), so the linter's hardcoded-URL
// check is a known false positive here.
#disable-next-line no-hardcoded-env-urls
var sqlPrivateDnsZoneName = 'privatelink.database.windows.net'
var logAnalyticsName = 'log-${workloadName}-${environment}-${location}-001'
var appInsightsName = 'appi-${workloadName}-${environment}-${monitorLocation}-001'

var backendImage = '${backendImageRepository}:${imageTag}'
var frontendImage = '${frontendImageRepository}:${imageTag}'

// ──────────────────────────────────────────────
//  Existing network (from the prior network chore) — referenced, not recreated
// ──────────────────────────────────────────────
resource spokeVnet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: spokeVnetName
}

resource privateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  parent: spokeVnet
  name: privateEndpointSubnetName
}

resource hubVnet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: hubVnetName
  scope: resourceGroup(hubResourceGroupName)
}

// New subnet for the Container Apps environment, delegated to Microsoft.App/environments.
module containerAppsSubnet 'br/public:avm/res/network/virtual-network/subnet:0.2.0' = {
  name: 'container-apps-subnet'
  params: {
    virtualNetworkName: spokeVnet.name
    name: containerAppsSubnetName
    addressPrefix: containerAppsSubnetAddressPrefix
    delegation: 'Microsoft.App/environments'
  }
}

// ──────────────────────────────────────────────
//  Identity — runtime UAMI for the backend only (separate CI/CD identity is out of scope here)
// ──────────────────────────────────────────────
module backendIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.6.0' = {
  name: 'backend-identity'
  params: {
    name: backendIdentityName
    location: location
    tags: tags
    enableTelemetry: false
    // Explicitly matches what Azure already defaults this to — avoids a benign what-if
    // "modify" line (the resource's actual state never changes either way).
    isolationScope: 'None'
  }
}

// ──────────────────────────────────────────────
//  Distributed Private DNS — zone lives in the workload RG, linked to its own spoke always,
//  and to the hub only when `linkPrivateDnsZoneToHub` allows it (see that parameter's
//  description for the Azure constraint behind this).
// ──────────────────────────────────────────────
var hubPrivateDnsZoneLink = [
  {
    name: 'link-${hubVnet.name}'
    virtualNetworkResourceId: hubVnet.id
    registrationEnabled: false
    resolutionPolicy: 'Default'
  }
]

module sqlPrivateDnsZone 'br/public:avm/res/network/private-dns-zone:0.8.1' = {
  name: 'sql-private-dns-zone'
  params: {
    name: sqlPrivateDnsZoneName
    tags: tags
    enableTelemetry: false
    virtualNetworkLinks: concat(
      [
        {
          name: 'link-${spokeVnet.name}'
          virtualNetworkResourceId: spokeVnet.id
          registrationEnabled: false
          resolutionPolicy: 'Default'
        }
      ],
      linkPrivateDnsZoneToHub ? hubPrivateDnsZoneLink : []
    )
  }
}

// ──────────────────────────────────────────────
//  Data — Azure SQL, Entra-only auth, backend MI as the declarative Entra admin,
//  private endpoint + distributed Private DNS, no public network access, no secrets.
// ──────────────────────────────────────────────
module sqlServer 'br/public:avm/res/sql/server:0.22.1' = {
  name: 'sql-server'
  params: {
    name: sqlServerName
    location: location
    tags: tags
    enableTelemetry: false
    publicNetworkAccess: 'Disabled'
    // Azure SQL's default connection policy is `Redirect`, which sends clients to a direct
    // node hostname (`*.worker.database.windows.net`) that the private DNS zone has no record
    // for — so it falls back to a public resolution path that's then correctly denied by
    // `publicNetworkAccess: Disabled`. Private Link requires `Proxy`: every packet routes
    // through the gateway, which the private endpoint IP already reaches directly.
    connectionPolicy: 'Proxy'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: backendIdentity.outputs.name
      principalType: 'Application'
      sid: backendIdentity.outputs.principalId
      tenantId: tenant().tenantId
    }
    databases: [
      {
        name: sqlDatabaseName
        availabilityZone: -1
        zoneRedundant: zoneRedundant
        sku: {
          name: sqlDatabaseSkuName
          tier: sqlDatabaseSkuTier
          family: sqlDatabaseSkuFamily
          capacity: sqlDatabaseCapacity
        }
        minCapacity: sqlDatabaseMinCapacity
        autoPauseDelay: sqlDatabaseAutoPauseDelay
        enableTelemetry: false
      }
    ]
    privateEndpoints: [
      {
        name: sqlPrivateEndpointName
        subnetResourceId: privateEndpointSubnet.id
        service: 'sqlServer'
        privateDnsZoneGroup: {
          privateDnsZoneGroupConfigs: [
            {
              privateDnsZoneResourceId: sqlPrivateDnsZone.outputs.resourceId
            }
          ]
        }
      }
    ]
  }
}

// ──────────────────────────────────────────────
//  Monitor — Log Analytics + Application Insights. Public on purpose (workshop requirement);
//  no private endpoints, no privatelink.* zones for these.
// ──────────────────────────────────────────────
module logAnalytics 'br/public:avm/res/operational-insights/workspace:0.16.1' = {
  name: 'log-analytics'
  params: {
    name: logAnalyticsName
    location: location
    tags: tags
    enableTelemetry: false
    dataRetention: logAnalyticsRetentionDays
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

module appInsights 'br/public:avm/res/insights/component:0.8.0' = {
  name: 'app-insights'
  params: {
    name: appInsightsName
    location: monitorLocation
    tags: tags
    enableTelemetry: false
    kind: 'web'
    applicationType: 'web'
    workspaceResourceId: logAnalytics.outputs.resourceId
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// ──────────────────────────────────────────────
//  Compute — Container Apps environment (workload profiles, /27 subnet, Consumption
//  profile only) with per-app ingress split: frontend external (public), backend internal.
// ──────────────────────────────────────────────
module containerAppsEnvironment 'br/public:avm/res/app/managed-environment:0.16.0' = {
  name: 'container-apps-environment'
  params: {
    name: containerAppsEnvironmentName
    location: location
    tags: tags
    enableTelemetry: false
    zoneRedundant: zoneRedundant
    internal: false
    // `publicNetworkAccess` must be explicitly 'Enabled' — the module defaults it to
    // 'Disabled', which would also block the frontend's public ingress on an otherwise
    // "External" environment.
    publicNetworkAccess: 'Enabled'
    // Matches what Azure already defaults peer-to-peer mTLS to — avoids a benign what-if
    // "modify" line (the environment's actual behavior is unchanged either way).
    peerAuthentication: {
      mtls: {
        enabled: false
      }
    }
    infrastructureSubnetResourceId: containerAppsSubnet.outputs.resourceId
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    // No `appLogsConfiguration`: wiring platform log shipping to Log Analytics requires a
    // workspace shared key (`listKeys()`), which — even though it's only used as an inline
    // deployment-time property, never an output — is an admin credential. The app's own
    // telemetry already reaches Application Insights via `APPLICATIONINSIGHTS_CONNECTION_STRING`
    // (see the backend/frontend apps below); container console logs remain available through
    // `az containerapp logs show` without it. Revisit only if a future chore needs an
    // identity-based log-shipping path.
  }
}

module backendApp 'br/public:avm/res/app/container-app:0.23.0' = {
  name: 'backend-app'
  params: {
    name: backendAppName
    location: location
    tags: tags
    enableTelemetry: false
    environmentResourceId: containerAppsEnvironment.outputs.resourceId
    workloadProfileName: 'Consumption'
    ingressExternal: false
    ingressTargetPort: 8080
    managedIdentities: {
      userAssignedResourceIds: [
        backendIdentity.outputs.resourceId
      ]
    }
    scaleSettings: {
      minReplicas: containerAppMinReplicas
      maxReplicas: containerAppMaxReplicas
    }
    containers: [
      {
        name: 'hotelapi'
        image: backendImage
        resources: {
          cpu: json(containerCpu)
          memory: containerMemory
        }
        env: [
          {
            name: 'ConnectionStrings__HotelDb'
            value: 'Server=tcp:${sqlServer.outputs.fullyQualifiedDomainName},1433;Database=${sqlDatabaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;'
          }
          {
            name: 'AZURE_CLIENT_ID'
            value: backendIdentity.outputs.clientId
          }
          {
            name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
            value: appInsights.outputs.connectionString
          }
        ]
      }
    ]
  }
}

module frontendApp 'br/public:avm/res/app/container-app:0.23.0' = {
  name: 'frontend-app'
  params: {
    name: frontendAppName
    location: location
    tags: tags
    enableTelemetry: false
    environmentResourceId: containerAppsEnvironment.outputs.resourceId
    workloadProfileName: 'Consumption'
    ingressExternal: true
    ingressTargetPort: 8080
    scaleSettings: {
      minReplicas: containerAppMinReplicas
      maxReplicas: containerAppMaxReplicas
    }
    containers: [
      {
        name: 'hotelweb'
        image: frontendImage
        resources: {
          cpu: json(containerCpu)
          memory: containerMemory
        }
        env: [
          {
            name: 'BACKEND_URL'
            value: 'https://${backendApp.outputs.fqdn}'
          }
        ]
      }
    ]
  }
}

// ──────────────────────────────────────────────
//  Outputs — no secrets. Resource properties only.
// ──────────────────────────────────────────────
@description('Resource ID of the Container Apps environment.')
output containerAppsEnvironmentId string = containerAppsEnvironment.outputs.resourceId

@description('Public HTTPS URL of the frontend.')
output frontendUrl string = 'https://${frontendApp.outputs.fqdn}'

@description('Internal HTTPS URL of the backend (reachable only from inside the Container Apps environment).')
output backendInternalUrl string = 'https://${backendApp.outputs.fqdn}'

@description('Resource ID of the backend\'s user-assigned managed identity (also the SQL Entra admin).')
output backendIdentityId string = backendIdentity.outputs.resourceId

@description('Fully qualified domain name of the SQL logical server.')
output sqlServerFqdn string = sqlServer.outputs.fullyQualifiedDomainName

@description('Name of the SQL database.')
output sqlDatabaseName string = sqlDatabaseName

@description('Resource ID of the Application Insights component.')
output appInsightsId string = appInsights.outputs.resourceId
