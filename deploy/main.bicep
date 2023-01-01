@description('Specifies the location to deploy to.')
param location string = resourceGroup().location

@description('Specifies the common name prefix for all resources.')
@minLength(5)
@maxLength(12)
param baseName string = 'contosoads'

@description('Specifies the name of the blob container.')
param containerName string = 'images'

@description('Specifies the name of the request queue.')
param requestQueueName string = 'thumbnail-request'

@description('Specifies the name of the result queue.')
param resultQueueName string = 'thumbnail-result'

@description('Specifies the PostgreSQL login name.')
@secure()
param postgresLogin string

@description('Specifies the PostgreSQL login password.')
@secure()
param postgresLoginPassword string

@description('Specifies the PostgreSQL version.')
param postgresVersion string = '14'

@description('Specifies the tag for the contosoads-web image.')
param webAppTag string = 'dotnet6'

@description('Specifies the tag for the contosoads-imageprocessor image.')
param imageProcessorTag string = 'dotnet6'

@description('Specifies the public Git repo that hosts the database migration script.')
param repository string

var vnetName = '${baseName}-vnet'
var storageAccountName = '${baseName}${uniqueString(resourceGroup().id)}'
var privateDnsZoneName = '${baseName}.postgres.database.azure.com'
var postgresHostName = 'server${uniqueString(resourceGroup().id)}'
var databaseName = 'contosoads'

module network 'modules/network.bicep' = {
  name: 'network'
  params: {
    location: location
    vnetName: vnetName
    privateDnsZoneName: privateDnsZoneName
  }
}

module environment 'modules/environment.bicep' = {
  name: 'environment'
  params: {
    location: location
    baseName: baseName
    infrastructureSubnetId: network.outputs.infraSubnetId
    storageAccountName: storageAccountName
    containerName: containerName
    requestQueueName: requestQueueName
    resultQueueName: resultQueueName
  }
}

module postgres 'modules/database.bicep' = {
  name: 'postgres'
  params: {
    location: location
    serverName: postgresHostName
    databaseName: databaseName
    postgresSubnetId: network.outputs.pgSubnetId
    aciSubnetId: network.outputs.aciSubnetId
    privateDnsZoneId: network.outputs.privateDnsZoneId
    administratorLogin: postgresLogin
    administratorLoginPassword: postgresLoginPassword
    version: postgresVersion
    repository: repository
  }
}

var dbConnectionString = 'Host=${postgresHostName}.postgres.database.azure.com;Database=${databaseName};Username=${postgresLogin};Password=${postgresLoginPassword}'

module webapp 'modules/webapp.bicep' = {
  name: 'webapp'
  params: {
    location: location
    tag: webAppTag
    environmentId: environment.outputs.environmentId
    dbConnectionString: dbConnectionString
    aiConnectionString: environment.outputs.aiConnectionString
  }
  dependsOn: [ postgres ]
}

module imageprocessor 'modules/imageprocessor.bicep' = {
  name: 'imageprocessor'
  params: {
    location: location
    tag: imageProcessorTag
    environmentId: environment.outputs.environmentId
    aiConnectionString: environment.outputs.aiConnectionString
  }
  dependsOn: [ postgres ]
}

output fqdn string = webapp.outputs.fqdn
