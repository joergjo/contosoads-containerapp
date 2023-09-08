@description('Specifies the name prefix of all resources.')
param namePrefix string

@description('Specifies the location to deploy to.')
param location string

@description('Specifies the tags for all resources.')
param tags object = {}

var uid = uniqueString(resourceGroup().id)
var workspaceName = '${namePrefix}${uid}-logs'
var appInsightsName = '${namePrefix}${uid}-insights'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  tags: tags
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

output workspaceId string = logAnalyticsWorkspace.id
output workspaceName string = logAnalyticsWorkspace.name
output aiConnectionString string = appInsights.properties.ConnectionString
