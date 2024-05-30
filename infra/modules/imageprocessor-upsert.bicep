@description('Specifies the location to deploy to.')
param location string

@description('Specifies the name of Azure Container Apps environment to deploy to.')
param environmentId string

@description('Specifies the Application Insights connection string.')
param aiConnectionString string

@description('Specifies the Azure Container registry name to pull from.')
param containerRegistryName string

@description('Specifies whether the app has been previously deployed.')
param exists bool

@description('Specifies the tags for all resources.')
param tags object = {}

var appName = 'contosoads-imageprocessor'
var defaultImage = 'joergjo/contosoads-imageprocessor:dotnet6'

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

resource existingContainerApp 'Microsoft.App/containerApps@2024-03-01' existing = if (exists) {
  name: appName
}

module imageprocessor 'imageprocessor.bicep' = {
  name: '${deployment().name}-update'
  params: {
    name: appName
    location: location
    tags: tags
    environmentId: environmentId
    image: exists ? existingContainerApp.properties.template.containers[0].image : defaultImage
    secrets: secrets
    envVars: envVars
    containerRegistryName: containerRegistryName
  }
}

output name string = imageprocessor.outputs.name
