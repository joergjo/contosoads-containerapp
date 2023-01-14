#!/bin/bash
if [ -z "$CONTOSOADS_RESOURCE_GROUP_NAME" ]; then
    echo "CONTOSOADS_RESOURCE_GROUP_NAME is not set. Please set it to the name of the resource group to deploy to."
    exit 1
fi

if [ -z "$CONTOSOADS_DB_PWD" ]; then
    echo "CONTOSOADS_POSTGRES_PWD is not set. Please set it to a secure password for the Contoso Ads DB server."
    exit 1
fi

resource_group_name=$CONTOSOADS_RESOURCE_GROUP_NAME
postgres_login_password=$CONTOSOADS_DB_PWD
base_name=${CONTOSOADS_BASE_NAME:-contosoads}
location=${CONTOSOADS_LOCATION:-northeurope}
webapp_tag=${CONTOSOADS_WEBAPP_TAG:-stable}
imageprocessor_tag=${CONTOSOADS_IMAGEPROCESSOR_TAG:-stable}
deployment_name="$base_name-$(date +%s)"
postgres_login=$base_name
postgres_version=${CONTOSOADS_POSTGRES_VERSION:-14}
repository=${CONTOSOADS_REPO:-'https://github.com/joergjo/contosoads-containerapp.git'}

az group create \
  --resource-group "$resource_group_name" \
  --location "$location"

fqdn=$(az deployment group create \
  --resource-group "$resource_group_name" \
  --name "$deployment_name" \
  --template-file main.bicep \
  --parameters postgresLogin="$postgres_login" postgresLoginPassword="$postgres_login_password" \
    webAppTag="$webapp_tag" imageProcessorTag="$imageprocessor_tag" \
    repository="$repository" postgresVersion="$postgres_version" \
  --query properties.outputs.fqdn.value \
  --output tsv)

if [ $? -ne 0 ]; then
    echo "Deployment error. Please check the deployment logs in the Azure portal."
    exit 1
fi

echo "Application has been deployed successfully. You can access it at https://$fqdn"
