#!/bin/sh
principal_type=$(az account show --query "user.type" --output tsv)
if [ "$principal_type" = "user" ]; then
  # A user's UPN cannot be reliably obtained from 'az account show' in the case the 
  # currently logged in user is a guest account. Therefore, we we obtain the UPN from the 
  # /me endpoint.  
  current_user_upn=$(az ad signed-in-user show --query userPrincipalName --output tsv)
  current_user_object_id=$(az ad signed-in-user show --query id --output tsv)
  current_user_type="User"
else
  # A service principal's UPN is the same as its appId. 
  current_user_upn=$(az account show --query "user.name" --output tsv)
  current_user_object_id=$(az ad sp show --id "$current_user_upn" --query "id" --output tsv)
  current_user_type="ServicePrincipal"
fi
azd env set CURRENT_USER_UPN "$current_user_upn"
azd env set CURRENT_USER_OBJECT_ID "$current_user_object_id"
azd env set CURRENT_USER_PRINCIPAL_TYPE "$current_user_type"
