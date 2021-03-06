@description('Specifies the name prefix of all resources.')
param baseName string

@description('Specifies the location to deploy to.')
param location string 

@description('Specifies the subnet resource ID for the Container App environment.')
param infrastructureSubnetId string

@description('Specifies the subnet resource ID for the Container App pods.')
param runtimeSubnetId string

@description('Specifies the name of the Azure Storage account.')
param storageAccountName string

@description('Specifies the name of the blob container for images and thumbnails.')
param containerName string

@description('Specifies the name of the request queue.')
param requestQueueName string

@description('Specifies the name of the result queue.')
param resultQueueName string

var workspaceName = '${baseName}-logs'
var appInsightsName = '${baseName}-insights'
var environmentName = '${baseName}-env'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
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

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  name: '${storageAccount.name}/default/${containerName}'
  properties: {
    publicAccess: 'Blob'
  }
}

resource requestQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-08-01' = {
  name: '${storageAccount.name}/default/${requestQueueName}'
}

resource resultQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-08-01' = {
  name: '${storageAccount.name}/default/${resultQueueName}'
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
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

resource environment 'Microsoft.App/managedEnvironments@2022-01-01-preview' = {
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
      runtimeSubnetId: runtimeSubnetId
    }
  }
}

resource imageStoreComponent 'Microsoft.App/managedEnvironments/daprComponents@2022-01-01-preview' = {
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

resource requestQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2022-01-01-preview' = {
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

resource resultQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2022-01-01-preview' = {
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

