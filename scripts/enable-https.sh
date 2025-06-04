#!/bin/bash
set -e

# Script to enable HTTPS mode for Azurite and the Contoso Ads application
# This addresses Safari 18's Mixed Content Level 2 requirements

echo "Setting up HTTPS mode for Azurite..."

# Create a backup directory for original components
mkdir -p .components-backup

# Check if backup already exists
if [ ! -f .components-backup/web-storage.yaml ]; then
    echo "Backing up original HTTP component configurations..."
    cp components/web-storage.yaml .components-backup/
    cp components/imageprocessor-storage.yaml .components-backup/
    cp components/thumbnail-request-receiver.yaml .components-backup/
    cp components/thumbnail-request-sender.yaml .components-backup/
    cp components/thumbnail-result-receiver.yaml .components-backup/
    cp components/thumbnail-result-sender.yaml .components-backup/
fi

# Replace HTTP endpoints with HTTPS endpoints in component files
echo "Updating Dapr component configurations for HTTPS..."

# Update blob storage components
sed -i 's|http://127\.0\.0\.1:10000|https://127.0.0.1:10000|g' components/web-storage.yaml
sed -i 's|http://127\.0\.0\.1:10000|https://127.0.0.1:10000|g' components/imageprocessor-storage.yaml

# Update queue components  
sed -i 's|http://127\.0\.0\.1:10001|https://127.0.0.1:10001|g' components/thumbnail-request-receiver.yaml
sed -i 's|http://127\.0\.0\.1:10001|https://127.0.0.1:10001|g' components/thumbnail-request-sender.yaml
sed -i 's|http://127\.0\.0\.1:10001|https://127.0.0.1:10001|g' components/thumbnail-result-receiver.yaml
sed -i 's|http://127\.0\.0\.1:10001|https://127.0.0.1:10001|g' components/thumbnail-result-sender.yaml

# Copy HTTPS environment configuration
cp .env.https .env

echo "HTTPS mode enabled!"
echo ""
echo "To start the application with HTTPS support:"
echo "  docker compose up -d"
echo ""
echo "To revert to HTTP mode, run:"
echo "  ./scripts/disable-https.sh"