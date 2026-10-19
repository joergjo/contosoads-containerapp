#!/bin/bash
set -e

# Simple script to enable HTTPS mode for Docker Compose
# This addresses Safari 18's Mixed Content Level 2 requirements for Docker Compose scenarios

echo "Setting up HTTPS mode for Docker Compose..."

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

echo "Configuring Dapr components for Docker Compose with HTTPS..."

# For Docker Compose mode, we need to use host.docker.internal and HTTPS
# This requires manually editing the component files to uncomment the Docker lines and change them to HTTPS

cat > components/web-storage.yaml << 'EOF'
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: web-storage
spec:
  type: bindings.azure.blobstorage
  version: v1
  metadata:
    - name: storageAccount
      secretKeyRef:
        name: storageAccount
    - name: storageAccessKey
      secretKeyRef:
        name: storageAccountKey
    - name: container
      value: "images"
    - name: decodeBase64
      value: true
    - name: endpoint
      value: "https://host.docker.internal:10000"
auth:
  secretStore: dev-secrets
scopes:
  - contosoads-web
EOF

cat > components/imageprocessor-storage.yaml << 'EOF'
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: imageprocessor-storage
spec:
  type: bindings.azure.blobstorage
  version: v1
  metadata:
    - name: storageAccount
      secretKeyRef:
        name: storageAccount
    - name: storageAccessKey
      secretKeyRef:
        name: storageAccountKey
    - name: container
      value: "images"
    - name: decodeBase64
      value: true
    - name: endpoint
      value: "https://host.docker.internal:10000"
auth:
  secretStore: dev-secrets
scopes:
  - contosoads-imageprocessor
EOF

# Queue components
cat > components/thumbnail-request-receiver.yaml << 'EOF'
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: thumbnail-request-receiver
spec:
  type: bindings.azure.storagequeues
  version: v1
  metadata:
    - name: storageAccount
      secretKeyRef:
        name: storageAccount
    - name: storageAccessKey
      secretKeyRef:
        name: storageAccountKey
    - name: queue
      value: "thumbnail-request"
    - name: route
      value: "/thumbnail-request"
    - name: endpoint
      value: "https://host.docker.internal:10001"
auth:
  secretStore: dev-secrets
scopes:
  - contosoads-imageprocessor
EOF

cat > components/thumbnail-request-sender.yaml << 'EOF'
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: thumbnail-request-sender
spec:
  type: bindings.azure.storagequeues
  version: v1
  metadata:
    - name: storageAccount
      secretKeyRef:
        name: storageAccount
    - name: storageAccessKey
      secretKeyRef:
        name: storageAccountKey
    - name: queue
      value: "thumbnail-request"
    - name: endpoint
      value: "https://host.docker.internal:10001"
auth:
  secretStore: dev-secrets
scopes:
  - contosoads-web
EOF

cat > components/thumbnail-result-receiver.yaml << 'EOF'
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: thumbnail-result-receiver
spec:
  type: bindings.azure.storagequeues
  version: v1
  metadata:
    - name: storageAccount
      secretKeyRef:
        name: storageAccount
    - name: storageAccessKey
      secretKeyRef:
        name: storageAccountKey
    - name: queue
      value: "thumbnail-result"
    - name: route
      value: "/thumbnail-result"
    - name: endpoint
      value: "https://host.docker.internal:10001"
auth:
  secretStore: dev-secrets
scopes:
  - contosoads-web
EOF

cat > components/thumbnail-result-sender.yaml << 'EOF'
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: thumbnail-result-sender
spec:
  type: bindings.azure.storagequeues
  version: v1
  metadata:
    - name: storageAccount
      secretKeyRef:
        name: storageAccount
    - name: storageAccessKey
      secretKeyRef:
        name: storageAccountKey
    - name: queue
      value: "thumbnail-result"
    - name: endpoint
      value: "https://host.docker.internal:10001"
auth:
  secretStore: dev-secrets
scopes:
  - contosoads-imageprocessor
EOF

# Copy HTTPS environment configuration
cp .env.https .env

echo "HTTPS mode enabled for Docker Compose!"
echo ""
echo "To start the application with HTTPS support:"
echo "  docker compose up -d"
echo ""
echo "To revert to HTTP mode, run:"
echo "  ./scripts/disable-https.sh"