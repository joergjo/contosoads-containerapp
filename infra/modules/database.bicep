@description('Specifies the name prefix of all resources.')
param namePrefix string

@description('Specifies the name of PostgreSQL database used by the application.')
param databaseName string

@description('Specifies the location to deploy to.')
param location string

@description('Specifies the PostgreSQL version.')
@allowed([
  '12'
  '13'
  '14'
  '15'
  '16'
])
param version string

@description('Specifies the PostgreSQL administrator login name.')
@secure()
param administratorLogin string

@description('Specifies the PostgreSQL administrator login password.')
@secure()
param administratorLoginPassword string

@description('Specifies the Entra ID PostgreSQL administrator user principal name.')
param entraIdAdmin string

@description('Specifies the Entra ID PostgreSQL administrator user\'s object ID.')
param entraIdAdminObjectId string

@description('Specifies the User Assigned Managed Identity for database migrations.')
param migrationIdentityName string

@description('Specifies the User Assigned Managed Identity for database migrations.')
param migrationIdentityObjectId string

@description('Specifies the subnet resource ID of the delegated subnet for Azure Database for PostgreSQL server.')
param postgresSubnetId string

@description('Specifies the subnet resource ID of the delegated subnet for Azure Container Instances.')
param aciSubnetId string

@description('Specifies the resource ID of the private DNS zone name.')
param privateDnsZoneId string

@description('Specifies the public Git repo that hosts the database migration script.')
param repository string

@description('Specifies the tags for all resources.')
param tags object = {}

var uid = uniqueString(resourceGroup().id)
var serverName = '${namePrefix}${uid}'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: migrationIdentityName
}

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: serverName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  tags: tags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
      tenantId: subscription().tenantId
    }
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      delegatedSubnetResourceId: postgresSubnetId
      privateDnsZoneArmResourceId: privateDnsZoneId
    }
    version: version
    createMode: 'Default'
  }
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  name: databaseName
  parent: postgresServer
  properties: {
    charset: 'utf8'
    collation: 'en_US.utf8'
  }
}

resource postgresEntraIdAdmin 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2023-06-01-preview' = {
  name: entraIdAdminObjectId
  parent: postgresServer
  properties: {
    principalName: entraIdAdmin
    principalType: 'User'
    tenantId: subscription().tenantId
  }
}

resource postgresMigrationIdentity 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2023-06-01-preview' = {
  name: migrationIdentityObjectId
  parent: postgresServer
  properties: {
    principalName: migrationIdentityName
    principalType: 'ServicePrincipal'
    tenantId: subscription().tenantId
  }
}

resource containerInstance 'Microsoft.ContainerInstance/containerGroups@2023-05-01' = {
  name: '${serverName}-migration'
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    containers: [
      {
        name: 'psql'
        properties: {
          image: 'postgres:15-alpine'
          command: [ 'sh', '/mnt/repo/deploy/migrate.sh' ]
          environmentVariables: [
            {
              name: 'CLIENT_ID'
              value: managedIdentity.properties.clientId
            }
            {
              name: 'PGHOST'
              value: postgresServer.properties.fullyQualifiedDomainName
            }
            {
              name: 'PGUSER'
              value: migrationIdentityName
            }
            {
              name: 'PGDATABASE'
              value: postgresDatabase.name
            }
          ]
          volumeMounts: [
            {
              name: 'github'
              mountPath: '/mnt/repo'
            }
          ]
          resources: {
            requests: {
              cpu: 1
              memoryInGB: 1
            }
          }
        }
      }
    ]
    osType: 'Linux'
    volumes: [
      {
        name: 'github'
        gitRepo: {
          repository: repository
          directory: '.'
        }
      }
    ]
    subnetIds: [
      {
        id: aciSubnetId
      }
    ]
    restartPolicy: 'Never'
  }
}

output serverFqdn string = postgresServer.properties.fullyQualifiedDomainName
output serverName string = postgresServer.name
output databaseName string = postgresDatabase.name
