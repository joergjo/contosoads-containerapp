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

@description('Specifies the name of the User-Assigned Managed Identity for the Web app.')
param webAppIdentityName string

@description('Specifies the name of the User-Assigned Managed Identity for the Image Processor.')
param imageProcessorIdentityName string

@description('Specifies the tags for all resources.')
param tags object = {}

var storageAccountName = '${length(namePrefix) <= 11 ? namePrefix : substring(namePrefix, 0, 11)}${uniqueString(resourceGroup().id)}'

var blobDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
var queueDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88')

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' = {
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

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2025-01-01' = {
  name: 'default'
  parent: storageAccount
}

resource imageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-01-01' = {
  name: imageContainerName
  parent: blobService
  properties: {
    publicAccess: 'Blob'
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2025-01-01' = {
  name: 'default'
  parent: storageAccount
}

resource requestQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2025-01-01' = {
  name: requestQueueName
  parent: queueService
}

resource resultQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2025-01-01' = {
  name: resultQueueName
  parent: queueService
}

resource webAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: webAppIdentityName
}

resource imageProcessorIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: imageProcessorIdentityName
}

resource webAppBlobDataContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: imageContainer
  name: guid(imageContainer.id, webAppIdentity.id, blobDataContributorRoleId)
  properties: {
    roleDefinitionId: blobDataContributorRoleId
    principalType: 'ServicePrincipal'
    principalId: webAppIdentity.properties.principalId
  }
}

resource imageProcessorBlobDataContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: imageContainer
  name: guid(imageContainer.id, imageProcessorIdentity.id, blobDataContributorRoleId)
  properties: {
    roleDefinitionId: blobDataContributorRoleId
    principalType: 'ServicePrincipal'
    principalId: imageProcessorIdentity.properties.principalId
  }
}

resource requestQueueMessageSenderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: requestQueue
  name: guid(requestQueue.id, webAppIdentity.id, queueDataContributorRoleId)
  properties: {
    roleDefinitionId: queueDataContributorRoleId
    principalType: 'ServicePrincipal'
    principalId: webAppIdentity.properties.principalId
  }
}

resource requestQueueMessageProcessorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: requestQueue
  name: guid(requestQueue.id, imageProcessorIdentity.id, queueDataContributorRoleId)
  properties: {
    roleDefinitionId: queueDataContributorRoleId
    principalType: 'ServicePrincipal'
    principalId: imageProcessorIdentity.properties.principalId
  }
}

resource resultQueueMessageSenderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resultQueue
  name: guid(resultQueue.id, imageProcessorIdentity.id, queueDataContributorRoleId)
  properties: {
    roleDefinitionId: queueDataContributorRoleId
    principalType: 'ServicePrincipal'
    principalId: imageProcessorIdentity.properties.principalId
  }
}

resource resultQueueMessageProcessorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resultQueue
  name: guid(resultQueue.id, webAppIdentity.id, queueDataContributorRoleId)
  properties: {
    roleDefinitionId: queueDataContributorRoleId
    principalType: 'ServicePrincipal'
    principalId: webAppIdentity.properties.principalId
  }
}


output storageAccoutName string = storageAccount.name
