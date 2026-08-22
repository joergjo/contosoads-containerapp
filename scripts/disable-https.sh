#!/bin/bash
set -e

# Script to disable HTTPS mode and restore HTTP configurations

echo "Restoring HTTP mode for Azurite..."

# Check if backup exists
if [ ! -d .components-backup ]; then
    echo "No backup found. HTTP mode may already be active."
    exit 0
fi

# Restore original component configurations
echo "Restoring original Dapr component configurations..."

if [ -f .components-backup/web-storage.yaml ]; then
    cp .components-backup/web-storage.yaml components/
    cp .components-backup/imageprocessor-storage.yaml components/
    cp .components-backup/thumbnail-request-receiver.yaml components/
    cp .components-backup/thumbnail-request-sender.yaml components/
    cp .components-backup/thumbnail-result-receiver.yaml components/
    cp .components-backup/thumbnail-result-sender.yaml components/
fi

# Remove HTTPS environment file
if [ -f .env ]; then
    rm .env
fi

echo "HTTP mode restored!"
echo ""
echo "To start the application with HTTP support:"
echo "  docker compose up -d"