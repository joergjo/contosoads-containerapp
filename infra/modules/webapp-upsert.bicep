@description('Specifies the location to deploy to.')
param location string

@description('Specifies the name of Azure Container Apps environment to deploy to.')
param environmentId string

@description('Specifies the PostgreSQL server FQDN.')
param postgresFqdn string

@description('Specifies the PostgreSQL database to use.')
param postgresDatabase string

@description('Specifies the PostgreSQL login name.')
@secure()
param postgresLogin string

@description('Specifies the PostgreSQL login password.')
@secure()
param postgresLoginPassword string

@description('Specifies the Application Insights connection string.')
param aiConnectionString string

@description('Specifies the Azure Container registry name to pull from.')
param containerRegistryName string

@description('Specifies whether the app has been previously deployed.')
param exists bool

@description('Specifies the tags for all resources.')
param tags object = {}

var appName = 'contosoads-web'
var dbConnectionString = 'Host=${postgresFqdn};Database=${postgresDatabase};Username=${postgresLogin};Password=${postgresLoginPassword}'
var defaultImage = 'joergjo/contosoads-web:latest'

var secrets = [
  {
    name: 'db-connection-string'
    value: dbConnectionString
  }
  {
    name: 'ai-connection-string'
    value: aiConnectionString
  }
]

var envVars = [
  {
    name: 'ConnectionStrings__DefaultConnection'
    secretRef: 'db-connection-string'
  }
  {
    name: 'ApplicationInsights__ConnectionString'
    secretRef: 'ai-connection-string'
  }
  {
    name: 'Logging__ApplicationInsights__LogLevel__ContosoAds'
    value: 'Debug'
  }
]

resource existingContainerApp 'Microsoft.App/containerApps@2023-05-01' existing = if (exists) {
  name: appName
}

module containerApp 'webapp.bicep' = {
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

output fqdn string = containerApp.outputs.fqdn
output name string = containerApp.outputs.name
