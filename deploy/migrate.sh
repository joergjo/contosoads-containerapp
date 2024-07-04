#!/bin/sh
# See https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/how-to-use-vm-token#retry-guidance
# We're running in Busybox, so we have to use a "string array"
wait_times="0 2 6 14 30"

for t in $wait_times
do
  export PGPASSWORD="$(wget -q -O- --header "Metadata: true" "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://ossrdbms-aad.database.windows.net&client_id=$CLIENT_ID" \
  | awk -F'"' '/access_token/ {print $4}')"
  if [ $? = 0 ]; then break; fi;
  echo "Error fetching token, retrying after $t seconds"
  sleep "$t"
done

if [ -z "$PGPASSWORD" ]; then
  echo "Failed to fetch token"
  exit 1
fi

psql -w -f /mnt/repo/deploy/migrate.sql
unset PGPASSWORD
