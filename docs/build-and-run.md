# Building and running the sample app on your PC or Mac

## Prerequisites

* macOS, Linux, or Windows 10/11 with the [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/) set up
* [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* [Dapr](https://docs.dapr.io/getting-started/install-dapr-cli/)
* [Docker Desktop](https://docs.docker.com/docker-desktop/install/)

These instructions allow you to run the Contoso Ads applications "natively" on your development machine 
including their Dapr sidecars. PostgreSQL and Azure Storage, emulated using [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=visual-studio), run in Docker 
Containers. 

### Preparing storage artifacts and secrets

Run the following shell script in bash or zsh to create the required storage artifacts and a secrets file.
You only need to execute this step once, as long as you don't delete the Docker 
volume that stores Azurite's workspace (see [Stopping and cleaning up](#stopping-and-cleaning-up)).

```bash
cd contosoads-containerapp

# Run Azurite
docker compose -f compose.deps.yaml up -d  

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

# Create blob container and queues
export AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
az storage container create -n images --public-access blob
az storage queue create -n thumbnail-request
az storage queue create -n thumbnail-result

docker compose -f compose.deps.yaml down  
```

Next, make sure the endpoint settings for all Dapr components located in the 
[`components`](../components) directory use `127.0.0.1` as endpoint (these are the default
settings).

```yaml
# imageprocessor-storage.yaml and web-storage.yaml
- name: endpoint
  value: "http://127.0.0.1:10000"

# thumbnail-request-receiver.yaml, thumbnail-request-sender.yaml, 
# thumbnail-result-receiver.yaml and thumbnail-result-sender.yaml
- name: endpoint
  value: "http://127.0.0.1:10001"
```

### Running the application

Run the following commands in a shell to compile the source code on the fly and 
start the application and its dependencies:

```bash
cd contosoads-containerapp
docker compose -f compose.deps.yaml --profile all up -d
dapr run -f .
```

Open https://localhost:7125 in your favorite browser to use the Contoso Ads web application.

### HTTPS Support for Safari 18+

If you're using Safari 18 or newer, you may need to enable HTTPS support for Azurite to ensure images load correctly due to Mixed Content Level 2 policies. To enable HTTPS mode:

```bash
cd contosoads-containerapp

# Enable HTTPS mode for Azurite
./scripts/enable-https.sh

# Restart Azurite with HTTPS support
docker compose -f compose.deps.yaml --profile all restart azurite
```

When using HTTPS mode, also update the Azure Storage connection string to use HTTPS endpoints:

```bash
export AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=https://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=https://127.0.0.1:10001/devstoreaccount1;TableEndpoint=https://127.0.0.1:10002/devstoreaccount1;"
az storage container create -n images --public-access blob
az storage queue create -n thumbnail-request
az storage queue create -n thumbnail-result
```

This will configure Azurite to use HTTPS endpoints and update the Dapr components accordingly. See [HTTPS Support documentation](https-support.md) for more details.

To revert back to HTTP mode:

```bash
./scripts/disable-https.sh
```

### Stopping and cleaning up

To stop the application, enter `CTRL-C` in the terminal window where you have run
`dapr run -f .`. 

To shut down the PostgreSQL and Azurite containers run
```bash
docker compose -f compose.deps.yaml --profile all down
```

If you want to remove all Docker volumes created for PostgreSQL and Azurite 
(e.g., to quickly start from scratch with an empty database and storage), run

```bash
docker compose -f compose.deps.yaml --profile all down -v
```

### Editing and building the solution (optional)

Open [ContosoAds.sln](../ContosoAds.sln) in your favorite IDE or code editor to
change the source code, build it, and runs tests.

To build the application and run in the included tests, execute 
the following commands.

```bash
cd contosoads-containerapp
dotnet build
dotnet test
```
