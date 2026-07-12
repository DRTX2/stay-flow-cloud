@description('Short lowercase environment name used in Azure resource names.')
param environmentName string = 'stayflow-dev'

@description('Azure region for all resources.')
param location string = resourceGroup().location


var normalizedName = toLower(replace(environmentName, '_', '-'))
var unique = uniqueString(resourceGroup().id, normalizedName)
var apiAppName = '${normalizedName}-api'
var webAppName = '${normalizedName}-web'
var workspaceName = '${normalizedName}-logs-${unique}'
var containerEnvName = '${normalizedName}-cae'

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource containerEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: workspace.properties.customerId
        sharedKey: workspace.listKeys().primarySharedKey
      }
    }
  }
}

output apiAppName string = apiAppName
output containerEnvName string = containerEnv.name
output containerEnvResourceGroup string = resourceGroup().name
output logAnalyticsWorkspaceName string = workspace.name
output webAppName string = webAppName
