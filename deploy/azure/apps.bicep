@description('Short lowercase environment name used in Azure resource names.')
param environmentName string = 'stayflow-dev'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Azure Container Registry name created by main.bicep.')
param acrName string

@description('Azure Container Apps managed environment name.')
param containerEnvName string

@description('Azure Container Apps managed environment resource group.')
param containerEnvResourceGroup string = 'rg-app-container'

@description('User-assigned managed identity name with AcrPull rights.')
param identityName string

@secure()
@description('Neon PostgreSQL connection string.')
param neonConnectionString string

@description('Container image for the ASP.NET Core API.')
param apiImage string

@description('Container image for the database migration/seed job.')
param migrationImage string

@description('Container image for the Next.js frontend.')
param webImage string

@description('Runtime public URL of the frontend.')
param siteUrl string

@description('Runtime public URL of the API/OIDC authority.')
param oidcAuthority string

@minValue(0)
@maxValue(10)
@description('Minimum replicas per Container App. Use 0 for Azure Students/dev cost control; use 1+ for production.')
param minReplicas int = 0

@minValue(1)
@maxValue(20)
@description('Maximum replicas per Container App.')
param maxReplicas int = 2

@secure()
@description('Initial platform administrator email used by the idempotent data seeder.')
param adminEmail string

@secure()
@description('Initial platform administrator password used by the idempotent data seeder.')
param adminPassword string

@secure()
@description('Confidential client secret used for service-to-service OAuth clients seeded in production.')
param serviceClientSecret string

var normalizedName = toLower(replace(environmentName, '_', '-'))
var apiAppName = '${normalizedName}-api'
var webAppName = '${normalizedName}-web'
var postgresConnectionString = neonConnectionString

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

resource containerEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerEnvName
  scope: resourceGroup(containerEnvResourceGroup)
}

resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}


resource api 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${pullIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        allowInsecure: false
        targetPort: 8080
        transport: 'auto'
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: pullIdentity.id
        }
      ]
      secrets: [
        {
          name: 'postgres-connection-string'
          value: postgresConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'Authentication__FrontendLoginUrl'
              value: '${siteUrl}/signin'
            }
            {
              name: 'Authentication__Issuer'
              value: oidcAuthority
            }
            {
              name: 'ConnectionStrings__Default'
              secretRef: 'postgres-connection-string'
            }
            {
              name: 'Authentication__SpaRedirectUris__0'
              value: '${siteUrl}/api/auth/callback'
            }
            {
              name: 'Authentication__SpaPostLogoutRedirectUris__0'
              value: '${siteUrl}/'
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health/live'
                port: 8080
              }
              initialDelaySeconds: 30
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
              }
              initialDelaySeconds: 45
              periodSeconds: 30
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

resource migrationJob 'Microsoft.App/jobs@2024-03-01' = {
  name: '${normalizedName}-migrations'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${pullIdentity.id}': {}
    }
  }
  properties: {
    environmentId: containerEnv.id
    configuration: {
      triggerType: 'Manual'
      replicaTimeout: 1800
      replicaRetryLimit: 1
      manualTriggerConfig: {
        parallelism: 1
        replicaCompletionCount: 1
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: pullIdentity.id
        }
      ]
      secrets: [
        {
          name: 'postgres-connection-string'
          value: postgresConnectionString
        }
        {
          name: 'seed-admin-email'
          value: adminEmail
        }
        {
          name: 'seed-admin-password'
          value: adminPassword
        }
        {
          name: 'service-client-secret'
          value: serviceClientSecret
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'migrations'
          image: migrationImage
          command: [
            '/bin/sh'
            '-c'
          ]
          args: [
            'dotnet StayFlow.MigrationHost.dll migrate && dotnet StayFlow.MigrationHost.dll seed'
          ]
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ConnectionStrings__Default'
              secretRef: 'postgres-connection-string'
            }
            {
              name: 'Seeding__AdminEmail'
              secretRef: 'seed-admin-email'
            }
            {
              name: 'Seeding__AdminPassword'
              secretRef: 'seed-admin-password'
            }
            {
              name: 'Authentication__ServiceClientSecret'
              secretRef: 'service-client-secret'
            }
            {
              name: 'Authentication__Issuer'
              value: oidcAuthority
            }
            {
              name: 'Authentication__SpaRedirectUris__0'
              value: '${siteUrl}/api/auth/callback'
            }
            {
              name: 'Authentication__SpaPostLogoutRedirectUris__0'
              value: '${siteUrl}/'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
    }
  }
}

resource web 'Microsoft.App/containerApps@2024-03-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${pullIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        allowInsecure: false
        targetPort: 3000
        transport: 'auto'
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: pullIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'web'
          image: webImage
          env: [
            {
              name: 'NODE_ENV'
              value: 'production'
            }
            {
              name: 'API_INTERNAL_URL'
              value: oidcAuthority
            }
            {
              name: 'NEXT_PUBLIC_SITE_URL'
              value: siteUrl
            }
            {
              name: 'NEXT_PUBLIC_OIDC_AUTHORITY'
              value: oidcAuthority
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/'
                port: 3000
              }
              initialDelaySeconds: 30
              periodSeconds: 30
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output apiUrl string = oidcAuthority
output webUrl string = siteUrl
