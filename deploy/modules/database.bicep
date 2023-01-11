@description('Specifies the name of the Azure Database for PostgreSQL flexible server.')
param serverName string

@description('Specifies the name of PostgreSQL database used by the application.')
param databaseName string

@description('Specifies the location to deploy to.')
param location string

@description('Specifies the PostgreSQL version.')
@allowed([
  '12'
  '13'
  '14'
])
param version string

@description('Specifies the PostgreSQL administrator login name.')
@secure()
param administratorLogin string

@description('Specifies the PostgreSQL administrator login password.')
@secure()
param administratorLoginPassword string

@description('Specifies the subnet resource ID of the delegated subnet for Azure Database.')
param postgresSubnetId string

@description('Specifies the subnet resource ID of the delegated subnet for Azure Container Instances.')
param aciSubnetId string

@description('Specifies the resource ID of the private DNS zone name.')
param privateDnsZoneId string

@description('Specifies the public Git repo that hosts the database migration script.')
param repository string

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2022-03-08-preview' = {
  name: serverName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
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

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-03-08-preview' = {
  name: databaseName
  parent: postgres
  properties: {
    charset: 'utf8'
    collation: 'en_US.utf8'
  }
}

var command = [
  'psql'
  '-h'
  '${serverName}.postgres.database.azure.com'
  '-U'
  administratorLogin
  '-d'
  databaseName
  '-f'
  '/mnt/repo/deploy/migrate.sql'
]

resource migration 'Microsoft.ContainerInstance/containerGroups@2021-10-01' = {
  name: 'dbmigration'
  location: location
  dependsOn: [
    database
  ]
  properties: {
    containers: [
      {
        name: 'psql'
        properties: {
          image: 'postgres:14-alpine'
          command: command
          ports: [
            {
              port: 5432
              protocol: 'TCP'
            }
          ]
          environmentVariables: [
            {
              name: 'PGPASSWORD'
              value: administratorLoginPassword
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
