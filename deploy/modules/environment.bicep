@description('Specifies the name prefix of all resources.')
param baseName string

@description('Specifies the location to deploy to.')
param location string 

@description('Specifies the subnet resource ID for the Container App environment.')
param infrastructureSubnetId string

@description('Specifies the name of the Azure Storage account.')
param storageAccountName string = '${length(baseName) <=11 ? baseName : substring(baseName, 0, 11)}${uniqueString(resourceGroup().id)}'

@description('Specifies the name of the blob container for images and thumbnails.')
param containerName string

@description('Specifies the name of the request queue.')
param requestQueueName string

@description('Specifies the name of the result queue.')
param resultQueueName string

var workspaceName = '${baseName}-logs'
var appInsightsName = '${baseName}-insights'
var environmentName = '${baseName}-env'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: containerName
  parent: blobService
  properties: {
    publicAccess: 'Blob'
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

resource requestQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = {
  name: requestQueueName
  parent: queueService
}

resource resultQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = {
  name: resultQueueName
  parent: queueService
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: { 
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource environment 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: environmentName
  location: location
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
  }
}

resource imageStoreComponent 'Microsoft.App/managedEnvironments/daprComponents@2022-10-01' = {
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
        value: containerName
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

resource requestQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2022-10-01' = {
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

resource resultQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2022-10-01' = {
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

output aiConnectionString string = appInsights.properties.ConnectionString
output environmentId string = environment.id

