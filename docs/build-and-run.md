# Building the sample app on your PC or Mac

## Prerequisites

* [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or an IDE like Visual Studio, Visual Studio Code, or Rider
* [Dapr](https://docs.dapr.io/getting-started/install-dapr-cli/)
* [Docker Desktop](https://docs.docker.com/docker-desktop/install/) 
* macOS, Linux, or Windows 10/11 with the [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/) set up
* An Azure storage account

> Note that [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=visual-studio) 
> is currently _not_ supported by Dapr until [this PR](https://github.com/dapr/components-contrib/pull/1692) is merged into a Dapr release. 
> Hence you need a real storage account.

### Create a new Azure storage account

Run the following script to create a new Azure storage account and a secrets file for Dapr. 
Make sure to set `name`, `resource_group_name`, and `location` to the appropriate values.

```bash
name=<your-storage-account-name>
resource_group_name=<your-resource-group-name>
location=<your-storage-account-location>

# Run this from the root directory of the sample app
cd contosoads-containerapp

# Create a resource group
az group create --name $resource_group_name \
  --location $location
  
# Create an Azure storage account
az storage account create --name $name \
  --resource-group $resource_group_name \
  --location $location \
  --sku Standard_LRS \
  --min-tls-version TLS1_2

# Obtain the primary storage account key
account_key=$(az storage account keys list \
  --resource-group "$resource_group_name" \
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

### Build the sample app from source 

Either open [ContosoAds.sln](ContosoAds.sln) in your favorite IDE and build the solution, or run the 
following commands in your system's shell.

```bash
cd contosoads-containerapp
dotnet build
```

### Running the Contoso Ads web application

Run the following commands in a shell:

```bash
docker compose -f compose.db.yaml up -d
cd src/ContosoAds.Web
dapr run --app-id web \
	--app-port 7125 \
	--app-ssl \
	--dapr-grpc-port 3501 \
	--components-path ../../components/ \
	-- dotnet run
```

This also starts the Dapr sidecar and a PostgreSQL database container.

### Running the Contoso Ads image processor API

Run the following commands in shell:

```bash
cd src/ContosoAds.ImageProcessor
dapr run --app-id imageprocessor \
	--app-port 7073 \
	--app-ssl \
	--dapr-grpc-port 63501 \
	--components-path ../../components/ \
	-- dotnet run
```

This also starts the Dapr sidecar.

Now, open http://localhost:7125 in your favorite browser to use the Contoso Ads web application.