#!/bin/sh
export PGPASSWORD=$(wget -q -O- -t 10 --waitretry=3 --header "Metadata: true" "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://ossrdbms-aad.database.windows.net&client_id=$CLIENT_ID" \
| awk -F'"' '/access_token/ {print $4}')
psql -f /mnt/repo/deploy/migrate.sql
unset PGPASSWORD
