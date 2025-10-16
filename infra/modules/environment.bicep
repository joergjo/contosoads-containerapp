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

@description('Specifies the web app\'s Dapr app ID.')
param webAppId string

@description('Specifies the image processor\'s Dapr app ID.')
param imageProcessorAppId string

@description('Specifies the name of the User-Assigned Managed Identity for the web app.')
param webAppIdentityName string

@description('Specifies the name of the User-Assigned Managed Identity for the image processor.')
param imageProcessorIdentityName string

@description('Specifies the Application Insights connection string.')
@secure()
param aiConnectionString string

@description('Specifies the tags for all resources.')
param tags object = {}

var uid = uniqueString(resourceGroup().id)
var environmentName = '${namePrefix}${uid}-env'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2025-02-01' existing = {
  name: workspaceName
}

resource webAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: webAppIdentityName
}

resource imageProcessorIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: imageProcessorIdentityName
}

resource environment 'Microsoft.App/managedEnvironments@2025-02-02-preview' = {
  name: environmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        #disable-next-line use-secure-value-for-secure-inputs
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    daprAIConnectionString: aiConnectionString
    vnetConfiguration: {
      infrastructureSubnetId: infrastructureSubnetId
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    infrastructureResourceGroup: '${resourceGroup().name}-deps'
    zoneRedundant: true
  }
}

resource webImageStorageComponent 'Microsoft.App/managedEnvironments/daprComponents@2025-02-02-preview' = {
  name: 'web-storage'
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
        name: 'container'
        value: imageContainerName
      }
      {
        name: 'decodeBase64'
        value: 'true'
      }
      {
        name: 'azureClientId'
        value: webAppIdentity.properties.clientId
      }
    ]
    scopes: [
      webAppId
    ]
  }
}

resource imageProcessorStorageComponent 'Microsoft.App/managedEnvironments/daprComponents@2025-02-02-preview' = {
  name: 'imageprocessor-storage'
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
        name: 'container'
        value: imageContainerName
      }
      {
        name: 'decodeBase64'
        value: 'true'
      }
      {
        name: 'azureClientId'
        value: imageProcessorIdentity.properties.clientId
      }
    ]
    scopes: [
      imageProcessorAppId
    ]
  }
}

resource requestQueueSendComponent 'Microsoft.App/managedEnvironments/daprComponents@2025-02-02-preview' = {
  name: 'thumbnail-request-sender'
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
        name: 'queue'
        value: requestQueueName
      }
      {
        name: 'azureClientId'
        value: webAppIdentity.properties.clientId
      }
    ]
    scopes: [
      webAppId
    ]
  }
}

resource requestQueueReceiveComponent 'Microsoft.App/managedEnvironments/daprComponents@2025-02-02-preview' = {
  name: 'thumbnail-request-receiver'
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
        name: 'queue'
        value: requestQueueName
      }
      {
        name: 'azureClientId'
        value: imageProcessorIdentity.properties.clientId
      }
      {
        name: 'route'
        value: '/thumbnail-request'
      }
    ]
    scopes: [
      imageProcessorAppId
    ]
  }
}

resource resultQueueSendComponent 'Microsoft.App/managedEnvironments/daprComponents@2025-02-02-preview' = {
  name: 'thumbnail-result-sender'
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
        name: 'queue'
        value: resultQueueName
      }
      {
        name: 'azureClientId'
        value: imageProcessorIdentity.properties.clientId
      }
    ]
    scopes: [
      imageProcessorAppId
    ]
  }
}

resource resultQueueReceiveComponent 'Microsoft.App/managedEnvironments/daprComponents@2025-02-02-preview' = {
  name: 'thumbnail-result-receiver'
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
        name: 'queue'
        value: resultQueueName
      }
      {
        name: 'azureClientId'
        value: webAppIdentity.properties.clientId
      }
      {
        name: 'route'
        value: '/thumbnail-result'
      }
    ]
    scopes: [
      webAppId
    ]
  }
}

output id string = environment.id
output name string = environment.name
