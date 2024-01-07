#!/bin/sh
declare -a wait_times=(0 2 6 14 30)

# See https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/how-to-use-vm-token#retry-guidance
for t in "${wait_times[@]}"
do
  export PGPASSWORD=$(wget -q -O- --header "Metadata: true" "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://ossrdbms-aad.database.windows.net&client_id=$CLIENT_ID" \
  | awk -F'"' '/access_token/ {print $4}')
  if [ $? = 0 ]; then break; fi;
  sleep $t
done

psql -f /mnt/repo/deploy/migrate.sql
unset PGPASSWORD
