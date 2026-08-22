# HTTPS Support for Azurite

## Overview

This project supports running Azurite (Azure Storage Emulator) with HTTPS to address Safari 18's Mixed Content Level 2 requirements. Safari 18 automatically upgrades HTTP requests to HTTPS when the main page is served over HTTPS, which can cause image loading failures if Azurite only serves HTTP endpoints.

## Quick Start

### Enable HTTPS Mode

To enable HTTPS support for Azurite:

```bash
# Enable HTTPS mode
./scripts/enable-https.sh

# Start the application with HTTPS support
docker compose up -d
```

### Disable HTTPS Mode

To revert back to HTTP mode:

```bash
# Disable HTTPS mode  
./scripts/disable-https.sh

# Start the application with HTTP support
docker compose up -d
```

## How It Works

### Certificates

The repository includes self-signed certificates for development use:

- `certs/azurite.crt` - Certificate file
- `certs/azurite.key` - Private key file  
- `certs/azurite.pem` - Combined certificate and key file

These certificates are valid for:
- `localhost`
- `127.0.0.1`
- `host.docker.internal`

### Configuration

When HTTPS mode is enabled:

1. **Environment Variables**: The `USE_HTTPS=true` environment variable is set
2. **Azurite Configuration**: Azurite starts with SSL certificates and binds to `0.0.0.0`
3. **Dapr Components**: All Azure Storage endpoints are updated to use `https://` URLs
4. **Port Mapping**: Same ports (10000, 10001) are used for both HTTP and HTTPS

### Scripts

- `scripts/enable-https.sh`: Configures the system for HTTPS mode
- `scripts/disable-https.sh`: Restores HTTP mode configuration

## When to Use HTTPS Mode

Use HTTPS mode when:

- Your web application is accessed via HTTPS
- You're testing with Safari 18 or newer browsers
- You need to comply with Mixed Content Security policies
- You're developing in an environment that requires encrypted connections

## Browser Compatibility

- **Safari 18+**: Requires HTTPS mode when main site uses HTTPS
- **Chrome/Firefox**: Works with both HTTP and HTTPS modes
- **Edge**: Works with both HTTP and HTTPS modes

## Security Note

The included certificates are self-signed and intended for development use only. For production deployments, use certificates from a trusted certificate authority.

## Troubleshooting

### Certificate Warnings

Browsers will show security warnings for self-signed certificates. This is normal for development environments. You can:

1. Click "Advanced" and "Proceed to localhost (unsafe)" in Chrome
2. Add the certificate to your system's trusted store
3. Use browser flags to ignore certificate errors for localhost

### Port Conflicts

If you encounter port conflicts:

```bash
# Stop all containers
docker compose down

# Remove any stale containers
docker container prune

# Restart with clean state
docker compose up -d
```

### Component Configuration Issues

If Dapr components aren't working:

```bash
# Check if HTTPS mode is properly enabled
cat .env

# Verify component endpoints
grep "endpoint" components/*.yaml

# Restart services after configuration changes
docker compose restart
```