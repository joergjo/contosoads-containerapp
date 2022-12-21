# Running the sample app on your PC or Mac using Docker

## Prerequisites

* [Docker Desktop](https://docs.docker.com/docker-desktop/install/)
* macOS, Linux, or Windows 10/11 with the [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/) set up
* An Azure storage account or [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=visual-studio)

> If you want to use Azurite, you should add it as service to the included
> Docker Compose file. 

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
  "storageAccount": "${name}",
  "storageAccountKey": "${account_key}"
}
EOF

# Copy secrets file to every project
cp secrets.json src/ContosoAds.Web/
mv secrets.json src/ContosoAds.ImageProcessor/ 
```

### Run application containers and dependencies

The sample includes a Docker Compose file that launches the application containers, Dapr sidecars, and a PostgreSQL
database. Run the following commands in a shell.

If your Docker Desktop setup supports Docker Compose v2, run:

```bash
cd contosoads-containerapp
docker compose up -d
```

Otherwise, run:

```bash
cd contosoads-containerapp
docker-compose up -d
```

Now, open http://localhost:8080 in your favorite browser to use the Contoso Ads web application.

To shut the application down again, run

```bash
cd contosoads-containerapp
docker compose down
```

or

```bash
cd contosoads-containerapp
docker-compose down
```
