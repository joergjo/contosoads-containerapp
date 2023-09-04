targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@minLength(2)
@maxLength(8)
@description('Specifies the name prefix for Azure resources.')
param namePrefix string = 'ca'

@description('Specifies the PostgreSQL login name.')
@secure()
param postgresLogin string

@description('Specifies the PostgreSQL login password.')
@secure()
param postgresLoginPassword string

@description('Specifies the PostgreSQL version.')
@allowed([
  '12'
  '13'
  '14'
  '15'
])
param postgresVersion string = '14'

@description('Specifies the public Git repo that hosts the database migration script.')
param repository string = 'https://github.com/joergjo/contosoads-containerapp.git'

@description('Specifies whether the web app has been previously deployed.')
param webAppExists bool = false

@description('Specifies whether the imageProcessor has been previously deployed.')
param imageProcessorExists bool = false

// Tags that should be applied to all resources.
// 
// Note that 'azd-service-name' tags should be applied separately to service host resources.
// Example usage:
//   tags: union(tags, { 'azd-service-name': <service name in azure.yaml> })
var tags = {
  'azd-env-name': environmentName
}

// var defaultImageProcessorImage = !empty(imageProcessorImage) ? imageProcessorImage : 'joergjo/contosoads-imageprocessor:latest'
var imageContainerName = 'images'
var requestQueueName = 'thumbnail-request'
var resultQueueName = 'thumbnail-result'

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: environmentName
  location: location
  tags: tags
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  scope: rg
  params: {
    location: location
    tags: tags
    namePrefix: namePrefix
    imageContainerName: imageContainerName
    requestQueueName: requestQueueName
    resultQueueName: resultQueueName
  }
}

module network 'modules/network.bicep' = {
  name: 'network'
  scope: rg
  params: {
    location: location
    tags: tags
    namePrefix: namePrefix
  }
}

module registry 'modules/registry.bicep' = {
  name: 'acr'
  scope: rg
  params: {
    location: location
    tags: tags
    namePrefix: namePrefix
  }
}

module postgres 'modules/database.bicep' = {
  name: 'postgres'
  scope: rg
  params: {
    location: location
    tags: tags
    namePrefix: namePrefix
    administratorLogin: postgresLogin
    administratorLoginPassword: postgresLoginPassword
    postgresSubnetId: network.outputs.pgSubnetId
    aciSubnetId: network.outputs.aciSubnetId
    privateDnsZoneId: network.outputs.privateDnsZoneId
    version: postgresVersion
    repository: repository
  }
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    location: location
    tags: tags
    namePrefix: namePrefix
  }
}

module environment 'modules/environment.bicep' = {
  name: 'environment'
  scope: rg
  params: {
    location: location
    tags: tags
    namePrefix: namePrefix
    infrastructureSubnetId: network.outputs.infraSubnetId
    workspaceName: monitoring.outputs.workspaceName
    storageAccountName: storage.outputs.storageAccoutName
    imageContainerName: imageContainerName
    requestQueueName: requestQueueName
    resultQueueName: resultQueueName
  }
}

module webapp 'modules/webapp-upsert.bicep' = {
  name: 'webapp'
  scope: rg
  params: {
    location: location
    tags: tags
    environmentId: environment.outputs.id
    aiConnectionString: monitoring.outputs.aiConnectionString
    containerRegistryName: registry.outputs.name
    postgresFqdn: postgres.outputs.serverFqdn
    postgresDatabase: postgres.outputs.databaseName
    postgresLogin: postgresLogin
    postgresLoginPassword: postgresLoginPassword
    exists: webAppExists
  }
}

module imageprocessor 'modules/imageprocessor-upsert.bicep' = {
  name: 'imageprocessor'
  scope: rg
  params: {
    location: location
    tags: tags
    environmentId: environment.outputs.id
    aiConnectionString: monitoring.outputs.aiConnectionString
    containerRegistryName: registry.outputs.name
    exists: imageProcessorExists
  }
}

output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = registry.outputs.loginServer
output AZURE_CONTAINER_REGISTRY_NAME string = registry.outputs.name
output AZURE_POSTGRESQL_SERVER_FQDN string = postgres.outputs.serverFqdn
output AZURE_POSTGRESQL_SERVER_NAME string = postgres.outputs.serverName
output AZURE_POSTGRESQL_DATABASE_NAME string = postgres.outputs.databaseName
output AZURE_CONTAINER_ENVIRONMENT_NAME string = environment.outputs.name
output SERVICE_WEBAPP_NAME string = webapp.outputs.name
output SERVICE_IMAGEPROCESSOR_NAME string = imageprocessor.outputs.name
