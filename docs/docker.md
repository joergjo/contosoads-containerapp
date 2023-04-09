# Running the sample app on your PC or Mac using Docker

## Prerequisites

* [Docker Desktop](https://docs.docker.com/docker-desktop/install/)
* macOS, Linux, or Windows 10/11 with the [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/) set up
* [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=visual-studio)

### Preparing the storage artifacts

Run the following script to create the required storage artifacts and a secrets file, then
add the emulator's endpoints in Dapr's component configuration. You only need to execute this step once, as long
as you don't delete the Docker volume that stores Azurite's workspace.

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

Next, uncomment the endpoint settings for all Dapr components located in the `components` directory.
In `image-store.yaml`, replace `IP_ADDRESS` with your computer's IP address (not 127.0.0.1).
By doing so, we can expose Azurite's blob endpoint both to a web browser and the other Contoso Ads 
containers. That is not required for the queue endpoints, since they are not accesses by a 
web browser.

```yaml
# image-store.yaml
- name: endpoint
  value: "http://IP_ADDRESS:10000"

# thumbnail-request.yaml and thumbnail-result.yaml
- name: queueEndpointUrl
  value: "http://host.docker.internal:10001"
```

### Run application containers and dependencies

The sample includes a Docker Compose file that launches the application containers, Dapr sidecars, Azurite and a PostgreSQL
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

To shut down the application (if using Docker Compose v2), run

```bash
cd contosoads-containerapp
docker compose down
```

or 

```bash
cd contosoads-containerapp
docker-compose down
```

:point_right: The Compose files uses the prebuilt images on Docker Hub. You can override
this by creating an `.env` file and setting `IMAGEPROCESSOR_IMAGE` and `WEB_IMAGE` to an image
that you have built locally or that is stored in another repository. See the 
[Compose docs](https://docs.docker.com/compose/environment-variables/set-environment-variables/#substitute-with-an-env-file)
to learn more about using `.env` files. 
