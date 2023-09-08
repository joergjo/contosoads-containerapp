@description('Specifies the name prefix of all resources.')
param namePrefix string

@description('Specifies the location to deploy to.')
param location string

@description('Specifies the subnet resource ID for the Container App environment.')
param infrastructureSubnetId string

@description('Specifies the Log Analytics workspace to connect to.')
param workspaceName string

@description('Specifies the name of the application\'s storage account.')
param storageAccountName string

@description('Specifies the name of the blob container.')
param imageContainerName string = 'images'

@description('Specifies the name of the request queue.')
param requestQueueName string = 'thumbnail-request'

@description('Specifies the name of the result queue.')
param resultQueueName string = 'thumbnail-result'

@description('Specifies the tags for all resources.')
param tags object = {}

var uid = uniqueString(resourceGroup().id)
var environmentName = '${namePrefix}${uid}-env'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: workspaceName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource environment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: environmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: infrastructureSubnetId
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    zoneRedundant: true
  }
}

resource imageStoreComponent 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  name: 'image-store'
  parent: environment
  properties: {
    componentType: 'bindings.azure.blobstorage'
    version: 'v1'
    metadata: [
      {
        name: 'storageAccount'
        value: storageAccountName
      }
      {
        name: 'storageAccessKey'
        secretRef: 'storage-key'
      }
      {
        name: 'container'
        value: imageContainerName
      }
      {
        name: 'decodeBase64'
        value: 'true'
      }
    ]
    secrets: [
      {
        name: 'storage-key'
        value: storageAccount.listKeys().keys[0].value
      }
    ]
  }
}

resource requestQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  name: 'thumbnail-request'
  parent: environment
  properties: {
    componentType: 'bindings.azure.storagequeues'
    version: 'v1'
    metadata: [
      {
        name: 'storageAccount'
        value: storageAccountName
      }
      {
        name: 'storageAccessKey'
        secretRef: 'storage-key'
      }
      {
        name: 'queue'
        value: requestQueueName
      }
    ]
    secrets: [
      {
        name: 'storage-key'
        value: storageAccount.listKeys().keys[0].value
      }
    ]
  }
}

resource resultQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  name: 'thumbnail-result'
  parent: environment
  properties: {
    componentType: 'bindings.azure.storagequeues'
    version: 'v1'
    metadata: [
      {
        name: 'storageAccount'
        value: storageAccountName
      }
      {
        name: 'storageAccessKey'
        secretRef: 'storage-key'
      }
      {
        name: 'queue'
        value: resultQueueName
      }
    ]
    secrets: [
      {
        name: 'storage-key'
        value: storageAccount.listKeys().keys[0].value
      }
    ]
  }
}

output id string = environment.id
output name string = environment.name
