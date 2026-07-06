@description('Short lowercase environment name used in Azure resource names.')
param environmentName string = 'stayflow-dev'

@description('Azure region for all resources.')
param location string = resourceGroup().location


var normalizedName = toLower(replace(environmentName, '_', '-'))
var unique = uniqueString(resourceGroup().id, normalizedName)
var acrName = 'sf${unique}'
var apiAppName = '${normalizedName}-api'
var webAppName = '${normalizedName}-web'
var identityName = '${normalizedName}-aca-pull'

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




output acrName string = acr.name
output acrLoginServer string = acr.properties.loginServer
output apiAppName string = apiAppName
output identityName string = pullIdentity.name
output webAppName string = webAppName
