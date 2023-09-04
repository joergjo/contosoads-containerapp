# Setting up a GitHub Actions workflow

## Prerequisites

* [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) 
* A [GitHub account](https://github.com/join) 

You can either install the Azure CLI locally, or use it through the 
[Azure Cloud Shell](https://shell.azure.com).

## Setup

1. Use the Azure CLI to create an Azure Service Principal, then store that principal's JSON output to a GitHub secret so the GitHub Actions CI/CD process can log into your Azure subscription and deploy the code.
2. Edit the `deploy.yml` workflow file and push the changes into a new `deploy` branch, triggering GitHub Actions to build container images and push those into a new Azure Container Apps Environment.

These steps are describe in more detail in the following sections.

### Authenticate to Azure and configure the repository with a secret

1. Fork this repository to your own GitHub organization.
2. Create an Azure Service Principal [using the Azure CLI](https://docs.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Clinux#use-the-azure-login-action-with-a-service-principal-secret).

   ```bash
   az login
   subscription_id=$(az account show --query id --output tsv)
   az ad sp create-for-rbac \
     --name ContosoAds-CICD \
     --role contributor \
     --sdk-auth \
     --scopes "/subscriptions/$subscription_id"
   ```

   > The output of that last command will include a deprecation warning for the `-sdk-auth`
   > flag. This is expected at the time of writing using Azure CLI 2.50.

3. Copy the JSON written to the screen to your clipboard.

   ```json
   {
     "clientId": "...",
     "clientSecret": "...",
     "subscriptionId": "...",
     "tenantId": "...",
     "activeDirectoryEndpointUrl": "https://login.microsoftonline.com/",
     "resourceManagerEndpointUrl": "https://brazilus.management.azure.com",
     "activeDirectoryGraphResourceId": "https://graph.windows.net/",
     "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
     "galleryEndpointUrl": "https://gallery.azure.com",
     "managementEndpointUrl": "https://management.core.windows.net"
   }
   ```

4. Create a new GitHub secret in your fork of this repository named `AZURE_SPN`. Paste the JSON returned from the Azure CLI into this new secret. Once you've done this you'll see the secret in your fork of the repository.
   > Note: Never save the JSON to disk, for it will enable anyone who obtains this file to create or edit resources in your Azure subscription.

5. Create a new GitHub secret in your fork of this repository named `DB_PWD` and set it to a secure password to be used for PostgreSQL.

   ![Secrets in GitHub](media/secrets.png)

### Deploy the code using GitHub Actions

The easiest way to deploy the code is to make a commit directly to the `deploy` branch. Do this by navigating to the `deploy.yml` file in your browser and
clicking the `Edit` button.

![Edit the deployment workflow file.](media/edit-the-deploy-file.png)

Provide a custom resource group name for the app, set the name of your GitHub repository and then commit the change to a new branch named `deploy`.
If you want to deploy to a different region, make sure to pick one that [offers Azure Container Apps, Azure Database for PostgreSQL Flexible Server
and Application Insights](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=monitor,postgresql,container-apps).

![Create the deploy branch.](media/deploy.png)

Once you click the `Propose changes` button, you'll be in "create a pull request" mode. Don't worry about creating the pull request yet, just click on the `Actions` tab, and you'll see that the deployment CI/CD process has already started.

![Build started.](media/deploy-started.png)

When you click into the workflow, you'll see the `deploy` job the CI/CD will run through:

![Deployment details.](media/deploy-details.png)

After a few minutes, the workflow will be completed and the workflow diagram will reflect success. If anything fails, you can click into the
individual process step to see the detailed log output.

> Note: if you do see any failures or issues, please submit an Issue so we can update the sample. Likewise, if you have ideas that could make
> it better, feel free to submit a pull request.

![Deployment success.](media/success.png)

With the projects deployed to Azure, you can now test the app to make sure it works.

