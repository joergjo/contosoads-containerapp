@description('Specifies the Container App\'s name.')
param name string

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

@description('Specifies the name of the User-Assigned Managed Identity for the Container App.')
param identityName string

@description('Specifies whether to connect to the database using Entra ID')
param useEntraId bool = true

@description('Specifies whether the app has been previously deployed.')
param exists bool

@description('Specifies the tags for all resources.')
param tags object = {}


resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}

var defaultImage = 'joergjo/contosoads-web:latest'
var dbConnectionString = useEntraId ? 'Host=${postgresFqdn};Database=${postgresDatabase};Username=${managedIdentity.name};' : 'Host=${postgresFqdn};Database=${postgresDatabase};Username=${postgresLogin};Password=${postgresLoginPassword}'

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
  {
    name: 'DataSource__UseEntraId'
    value: useEntraId ? 'true' : 'false'
  }
  {
    name: 'DataSource__ManagedIdentityClientId'
    value: useEntraId ? managedIdentity.properties.clientId : ''
  }
]

resource existingContainerApp 'Microsoft.App/containerApps@2023-05-01' existing = if (exists) {
  name: name
}

module containerApp 'webapp.bicep' = {
  name: '${deployment().name}-update'
  params: {
    name: name
    location: location
    tags: tags
    environmentId: environmentId
    image: exists ? existingContainerApp.properties.template.containers[0].image : defaultImage
    secrets: secrets
    envVars: envVars
    containerRegistryName: containerRegistryName
    identityName: identityName
  }
}

output fqdn string = containerApp.outputs.fqdn
output name string = containerApp.outputs.name
