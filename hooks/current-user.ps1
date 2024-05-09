$principalType = az account show --query "user.type" --output tsv
if ($principalType -eq "user") {
    # A user's UPN cannot be reliably obtained from 'az account show' in the case the
    # currently logged in user is a guest account. Therefore, we we obtain the UPN from the
    # /me endpoint.
    $currentUserUpn = az ad signed-in-user show --query userPrincipalName --output tsv
    $currentUserObjectId = az ad signed-in-user show --query id --output tsv
    $currentUserType = "User"
}
else {
    # A service principal's UPN is the same as its appId.
    $currentUserUpn = az account show --query "user.name" --output tsv
    $currentUserObjectId = az ad sp show --id $current_user_upn --query "id" --output tsv
    $currentUserType = "ServicePrincipal"
}
azd env set CURRENT_USER_UPN $currentUserUpn
azd env set CURRENT_USER_OBJECT_ID $currentUserObjectId
azd env set CURRENT_USER_PRINCIPAL_TYPE $currentUserType