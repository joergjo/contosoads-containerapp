@description('Specifies the common name prefix for all resources.')
@minLength(5)
@maxLength(14)
param baseName string

@description('Specifies the name of the private DNS zone.')
param privateDnsZoneName string = '${baseName}.postgres.database.azure.com'

@description('Specifies the location to deploy to.')
param location string

resource vnet 'Microsoft.Network/virtualNetworks@2022-09-01' = {
  name: '${baseName}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.150.0.0/22'
      ]
    }
    subnets: [
      {
        name: 'infrastructure'
        properties: {
          addressPrefix: '10.150.0.0/23'
          networkSecurityGroup: {
            id: networkSecurityGroup.id
          }
        }
      }
      {
        name: 'postgres-delegated'
        properties: {
          addressPrefix: '10.150.2.0/24'
          delegations: [
            {
              name: 'Microsoft.DBforPostgreSQL/flexibleServers'
              properties: {
                serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
              }
            }
          ]
        }
      }
      {
        name: 'aci-delegated'
        properties: {
          addressPrefix: '10.150.3.0/24'
          delegations: [
            {
              name: 'Microsoft.ContainerInstance/containerGroups'
              properties: {
                serviceName: 'Microsoft.ContainerInstance/containerGroups'
              }
            }
          ]
        }
      }
    ]
  }
}

resource networkSecurityGroup 'Microsoft.Network/networkSecurityGroups@2021-02-01' = {
  name: '${baseName}-infra-nsg'
  location: location
  properties: {
    securityRules: [
      {
        name: 'AllowAnyHTTPSInbound'
        properties: {
          priority: 1000
          access: 'Allow'
          direction: 'Inbound'
          destinationPortRange: '443'
          protocol: 'Tcp'
          sourcePortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
        }
      }
      {
        name: 'AllowAnyHTTPInbound'
        properties: {
          priority: 1001
          access: 'Allow'
          direction: 'Inbound'
          destinationPortRange: '80'
          protocol: 'Tcp'
          sourcePortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
        }
      }
    ]
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: privateDnsZoneName
  location: 'global'
}

resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: '${vnet.name}-link'
  location: 'global'
  properties: {
    registrationEnabled: true
    virtualNetwork: {
      id: vnet.id
    }
  }
}

output vnetId string = vnet.id
output infraSubnetId string = vnet.properties.subnets[0].id
output pgSubnetId string = vnet.properties.subnets[1].id
output aciSubnetId string = vnet.properties.subnets[2].id
output privateDnsZoneId string = privateDnsZone.id
