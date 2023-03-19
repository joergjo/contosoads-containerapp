# Building the sample app on your PC or Mac

## Prerequisites

* [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) or an IDE such as Visual Studio, Visual Studio Code, or Rider
* [Dapr](https://docs.dapr.io/getting-started/install-dapr-cli/)
* [Docker Desktop](https://docs.docker.com/docker-desktop/install/) 
* macOS, Linux, or Windows 10/11 with the [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/) set up
* An Azure storage account or [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=visual-studio)

### Create a new Azure storage account

Run the following script to create a new Azure storage account and a secrets file for Dapr. 
Make sure to set `name`, `resource_group`, and `location` to the appropriate values.

```bash
name=<your-storage-account-name>
resource_group=<your-resource-group>
location=<your-storage-account-location>

# Run this from the root directory of the repository
cd contosoads-containerapp

# Create a resource group
az group create --name $resource_group \
  --location $location
  
# Create an Azure storage account
az storage account create --name $name \
  --resource-group $resource_group \
  --location $location \
  --sku Standard_LRS \
  --min-tls-version TLS1_2

# Obtain the primary storage account key
account_key=$(az storage account keys list \
  --resource-group "$resource_group" \
  --account-name "$name" \
  --query "[0].value" \
  --out tsv)

# Create a secrets.json store for Dapr
# This file is already included in .gitignore
cat << EOF > secrets.json
{
  "storageAccountName": "$name",
  "storageAccountKey": "$account_key"
}
EOF

# Copy secrets file to every project
cp secrets.json src/ContosoAds.Web/
mv secrets.json src/ContosoAds.ImageProcessor/ 
```

### Using Azurite

Using Azurite requires the manual creation of a publicly accessible blob container and adding the
emulator's endpoints in Dapr's component configuration.

```bash
# Run this from the root directory of the repository
cd contosoads-containerapp

# Create a secrets.json store for Dapr and Azurite
# This file is already included in .gitignore
cat << EOF > secrets.json
{
  "storageAccount": "devstoreaccount1",
  "storageAccountKey": "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
}
EOF

# Copy secrets file to every project
cp secrets.json src/ContosoAds.Web/
mv secrets.json src/ContosoAds.ImageProcessor/ 

export AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
az storage container create -n images --public-access blob
```

Next, uncomment the endpoint settings for all Dapr components located in the `components` directory. 
Update the ports and protocol if you are running Azurite on non-default ports or HTTPS.

```yaml
# image-store.yaml
- name: endpoint
  value: "http://localhost:10000"

# thumbnail-request.yaml and thumbnail-result.yaml
- name: queueEndpointUrl
  value: "http://localhost:10001/devstoreaccount1/thumbnail-request"
```


### Build the sample app from source 

Either open [ContosoAds.sln](../ContosoAds.sln) in your favorite IDE and build the solution, or run the 
following commands in your system's shell.

```bash
cd contosoads-containerapp
dotnet build
```

### Running the Contoso Ads web application

Run the following commands in a shell:

```bash
cd contosoads-containerapp
docker compose -f compose.db.yaml up -d
cd src/ContosoAds.Web
dapr run --app-id web \
  --app-port 7125 \
  --app-ssl \
  --dapr-grpc-port 3501 \
  --resources-path ../../components/ \
  -- dotnet run
```

This also starts the Dapr sidecar and a PostgreSQL database container.

### Running the Contoso Ads image processor API

Run the following commands in shell:

```bash
cd contosoads-containerapp/src/ContosoAds.ImageProcessor
dapr run --app-id imageprocessor \
  --app-port 7073 \
  --app-ssl \
  --dapr-grpc-port 63501 \
  --resources-path ../../components/ \
  -- dotnet run
```

This also starts the Dapr sidecar.

Now, open http://localhost:7125 in your favorite browser to use the Contoso Ads web application.