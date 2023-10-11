@description('Specifies the name prefix of all resources.')
param namePrefix string

@description('Specifies the location to deploy to.')
param location string

@description('Specifies the name of the blob container for images and thumbnails.')
param imageContainerName string

@description('Specifies the name of the request queue.')
param requestQueueName string

@description('Specifies the name of the result queue.')
param resultQueueName string

@description('Specifies the tags for all resources.')
param tags object = {}

var storageAccountName = '${length(namePrefix) <= 11 ? namePrefix : substring(namePrefix, 0, 11)}${uniqueString(resourceGroup().id)}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: true
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Enabled'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: imageContainerName
  parent: blobService
  properties: {
    publicAccess: 'Blob'
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
}

resource requestQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: requestQueueName
  parent: queueService
}

resource resultQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: resultQueueName
  parent: queueService
}

output storageAccoutName string = storageAccount.name
