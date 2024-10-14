# Running the sample app on your PC or Mac using Docker

## Prerequisites

* macOS, Linux, or Windows 10/11 with the [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/) set up.
* [Docker Desktop](https://docs.docker.com/docker-desktop/install/)

### Preparing storage artifacts and secrets

Run the following shell script in bash or zsh to create the required storage artifacts and a secrets file.
You only need to execute this step once, as long as you don't delete the Docker
volume that stores Azurite's workspace (see [Storing and cleaning up](#cleanup)).

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

Next, uncomment the endpoint settings for all Dapr components located in the [`components`](../components) directory.
In `image-store.yaml`, replace the `IP_ADDRESS` placeholder with your computer's 
IP address (_not_ 127.0.0.1).

This way we can expose Azurite's blob endpoint both on your local network and to the
Docker network that the other containers use. Note that this is not required for the queue 
endpoint, since it is accessed only by the .NET backend services.

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
database. Run the following commands to launch the entire stack.

```bash
cd contosoads-containerapp
docker compose up -d
```

Open http://localhost:8080 in your favorite browser to use the Contoso Ads web application.

### Stopping and cleaning up

To shut down the application run

```bash
cd contosoads-containerapp
docker compose down
```

If you want to remove all Docker volumes created for PostgreSQL and Azurite
(e.g., to quickly start from scratch with an empty database and storage), run

```bash
docker compose down -v
```

### Building and using your own container images 

The Compose file uses the prebuilt images which I provide on Docker Hub. You can 
override the image names and tags by creating an `.env` file and setting 
`IMAGEPROCESSOR_IMAGE` and `WEB_IMAGE` to an image that you have built locally or 
that is stored in another repository. 

To build images for your local CPU architecture (e.g., ARM64 on an Apple silicon Mac,
AMD64 on an x64 based Windows PC) run, create an `.env` file as mentioned and run

```bash
cd contosoads-containerapp
docker compose build
````

See the [Compose docs](https://docs.docker.com/compose/environment-variables/set-environment-variables/#substitute-with-an-env-file)
to learn more about using `.env` files. 
