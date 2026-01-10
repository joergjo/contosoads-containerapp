@description('Specifies the name prefix of all resources.')
param namePrefix string

@description('Specifies the location to deploy to.')
param location string

@description('Indicates whether admin user is enabled')
param adminUserEnabled bool = true

@description('Specifies the tags for all resources.')
param tags object = {}

var acrName = '${length(namePrefix) <= 11 ? namePrefix : substring(namePrefix, 0, 11)}${uniqueString(resourceGroup().id)}'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: acrName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: adminUserEnabled
    dataEndpointEnabled: false
    encryption: {
      status: 'disabled'
    }
    networkRuleBypassOptions: 'AzureServices'
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: 'Disabled'
  }
}

output loginServer string = containerRegistry.properties.loginServer
output name string = containerRegistry.name
