@description('Specifies the name of the Container App.')
param name string

@description('Specifies the location to deploy to.')
param location string

@description('Specifies the name of Azure Container Apps environment to deploy to.')
param environmentId string

@description('Specifies the container image.')
param image string

@description('Specifies the application\'s secrets.')
param secrets array

@description('Specifies the application\'s environment.')
param envVars array

@description('Specifies the Azure Container registry name to pull from.')
param containerRegistryName string

@description('Specifies the name of the User-Assigned Managed Identity for the Container App.')
param identityName string

@description('Specifies the tags for all resources.')
param tags object = {}

var containerPort = 8080
var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-11-01' existing = {
  name: containerRegistryName
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: identityName
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(containerRegistry.id, managedIdentity.id, acrPullRoleId)
  properties: {
    roleDefinitionId: acrPullRoleId
    principalType: 'ServicePrincipal'
    principalId: managedIdentity.properties.principalId
  }
}

resource containerApp 'Microsoft.App/containerApps@2025-10-02-preview' = {
  name: name
  location: location
  tags: union(tags, { 'azd-service-name': 'webapp' })
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  dependsOn: [
    acrPullAssignment
  ]
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      dapr: {
        appId: name
        appPort: containerPort
        enabled: true
        logLevel: 'info'
      }
      ingress: {
        external: true
        targetPort: containerPort
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: managedIdentity.id
        }
      ]
      secrets: secrets
      runtime:{
        dotnet: {
          autoConfigureDataProtection: true
        }
      }
    }
    template: {
      containers: [
        {
          image: image
          name: name
          env: envVars
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          probes: [
            {
              type: 'liveness'
              httpGet: {
                scheme: 'HTTP'
                path: '/healthz/live'
                port: containerPort
              }
            }
            {
              type: 'readiness'
              httpGet: {
                scheme: 'HTTP'
                path: '/healthz/ready'
                port: containerPort
              }
              initialDelaySeconds: 5
            }
          ]
        }
      ]
      scale: {
        minReplicas: 2
        maxReplicas: 10
        rules: [
          {
            name: 'httpscale'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
    workloadProfileName: 'Consumption'
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
output name string = containerApp.name
