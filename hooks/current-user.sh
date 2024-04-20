#!/bin/sh

current_user_upn=$(az ad signed-in-user show --query userPrincipalName --output tsv)
azd env set CURRENT_USER_UPN "$current_user_upn"
current_user_objectid=$(az ad signed-in-user show --query id --output tsv)
azd env set CURRENT_USER_OBJECTID "$current_user_objectid"
