@description('Specifies the tag of the ContosoAds image processor container.')
param tag string

@description('Specifies the location to deploy to.')
param location string 

@description('Specifies the name of Azure Container Apps environment to deploy to.')
param environmentId string

@description('Specifies the Application Insights connection string.')
param aiConnectionString string

var secrets = [
  {
    name: 'ai-connection-string'
    value: aiConnectionString
  }
]

var containerPort = 8081

resource imageProcessor 'Microsoft.App/containerApps@2022-03-01' = {
  name: 'contosoads-imageprocessor'
  location: location
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      secrets: secrets
      dapr: {
        enabled: true
        appId: 'contosoads-imageprocessor'
        appPort: containerPort
      }

    }
    template: {
      containers: [
        {
          image: 'joergjo/contosoads-imageprocessor:${tag}'
          name: 'contosoads-imageprocessor'
          env: [
            {
              name: 'ApplicationInsights__ConnectionString'
              secretRef: 'ai-connection-string'
            }           
            {
              name: 'Logging__ApplicationInsights__LogLevel__ContosoAds'
              value: 'Debug'
            }           
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        //   rules: [
        //     {
        //       name: 'httpscale2'
        //       http: {
        //         metadata: {
        //           concurrentRequests: '100'
        //         }
        //     }
        //   }
        // ]
      }
    }
  }
}

