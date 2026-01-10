@description('Specifies the Container App\'s name.')
param name string

@description('Specifies the location to deploy to.')
param location string

@description('Specifies the name of Azure Container Apps environment to deploy to.')
param environmentId string

@description('Specifies the Application Insights connection string.')
param aiConnectionString string

@description('Specifies the Azure Container registry name to pull from.')
param containerRegistryName string

@description('Specifies the name of the User-Assigned Managed Identity for the Container App.')
param identityName string

@description('Specifies whether the app has been previously deployed.')
param exists bool

@description('Specifies the tags for all resources.')
param tags object = {}

var defaultImage = 'joergjo/contosoads-imageprocessor:latest'

var secrets = [
  {
    name: 'ai-connection-string'
    value: aiConnectionString
  }
]

var envVars = [
  {
    name: 'ApplicationInsights__ConnectionString'
    secretRef: 'ai-connection-string'
  }
  {
    name: 'Logging__ApplicationInsights__LogLevel__ContosoAds'
    value: 'Debug'
  }
]

resource existingContainerApp 'Microsoft.App/containerApps@2025-10-02-preview' existing = if (exists) {
  name: name
}

module containerApp 'imageprocessor.bicep' = {
  name: '${deployment().name}-update'
  params: {
    name: name
    location: location
    tags: tags
    environmentId: environmentId
    image: exists ? existingContainerApp!.properties.template.containers[0].image : defaultImage
    secrets: secrets
    envVars: envVars
    containerRegistryName: containerRegistryName
    identityName: identityName
  }
}

output name string = containerApp.outputs.name
