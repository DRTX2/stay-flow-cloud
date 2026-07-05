@description('Short lowercase environment name used in Azure resource names.')
param environmentName string = 'stayflow-dev'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('PostgreSQL administrator login.')
param postgresAdminLogin string = 'stayflowadmin'

@secure()
@description('PostgreSQL administrator password. Store this as a GitHub Actions secret.')
param postgresAdminPassword string

@description('PostgreSQL database name.')
param postgresDatabaseName string = 'stayflow'

var normalizedName = toLower(replace(environmentName, '_', '-'))
var unique = uniqueString(resourceGroup().id, normalizedName)
var acrName = 'sf${unique}'
var logAnalyticsName = '${normalizedName}-logs'
var containerEnvName = '${normalizedName}-cae'
var apiAppName = '${normalizedName}-api'
var webAppName = '${normalizedName}-web'
var identityName = '${normalizedName}-aca-pull'
var postgresName = take('${normalizedName}-pg-${unique}', 63)

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, pullIdentity.id, 'AcrPull')
  scope: acr
  properties: {
    principalId: pullIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: postgresName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    version: '16'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  name: postgresDatabaseName
  parent: postgres
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-06-01-preview' = {
  name: 'AllowAzureServices'
  parent: postgres
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource containerEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
  }
}

output acrName string = acr.name
output acrLoginServer string = acr.properties.loginServer
output apiAppName string = apiAppName
output apiUrl string = 'https://${apiAppName}.${containerEnv.properties.defaultDomain}'
output containerEnvName string = containerEnv.name
output identityName string = pullIdentity.name
output postgresServerName string = postgres.name
output webAppName string = webAppName
output webUrl string = 'https://${webAppName}.${containerEnv.properties.defaultDomain}'
