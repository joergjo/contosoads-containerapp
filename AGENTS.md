# Agent instructions

## Build and test

Run commands from the repository root. Both applications target .NET 11 / C# 15;
`global.json` selects the SDK and opts into Microsoft.Testing.Platform.

Use the checked-in `ContosoAds.slnx` and project files as the source of truth
when older documentation names a `.sln` file or separate unit/integration test
projects. The current two test projects each contain both kinds of tests.

```sh
dotnet build ContosoAds.slnx
dotnet test --solution ContosoAds.slnx

# One test project
dotnet test --project test/ContosoAds.ImageProcessor.Tests/ContosoAds.ImageProcessor.Tests.csproj

# One test method
dotnet test --project test/ContosoAds.ImageProcessor.Tests/ContosoAds.ImageProcessor.Tests.csproj --filter-method 'ContosoAds.ImageProcessor.Tests.ImageBlobTest.Can_Serialize_Json'

# One test class
dotnet test --project test/ContosoAds.Web.Tests/ContosoAds.Web.Tests.csproj --filter-class 'ContosoAds.Web.Tests.Commands.ReadAdTest'
```

Pass runner options directly to `dotnet test` with this configuration. Add
`--coverlet` to collect coverage using the existing `coverlet.MTP` integration.

Both test projects combine unit and HTTP integration tests. They use xUnit v3,
FakeItEasy, and `WebApplicationFactory`; the web page tests also use AngleSharp.
Web tests use real PostgreSQL through `PostgreSqlContainerFixture`
(`postgres:17-alpine`), so Docker must be running. Reuse the fixture and
`TestSupport` migration/seeding helpers. Both factories replace `DaprClient`
with a fake, so tests do not require live Dapr or Azure Storage.

The Npgsql EF provider `11.0.0-rc.3` requires exactly EF Core
`11.0.0-rc.3.26522.148`. Keep EF Core, its design/tools packages, and its ASP.NET
Core integration packages on that matching release candidate. The underlying
Npgsql driver stays on 10.x; its version is independent of the EF provider's.
Dapr versions are unchanged.

## Running locally

Follow [the native setup guide](docs/build-and-run.md) first to create the
Azurite `images` container, `thumbnail-request` and `thumbnail-result` queues,
and ignored per-application `secrets.json` files. Then run:

```sh
docker compose -f compose.deps.yaml --profile all up -d
dapr run -f .
```

The web UI is at `https://localhost:7125`. `dapr.yaml` coordinates both apps and
their sidecars; keep its app IDs and ports consistent with component scopes
and each application's `Properties/launchSettings.json`.

For the full container stack, follow [the Docker guide](docs/docker.md), then
use `docker compose up -d` and browse to `http://localhost:8080`.
Local Dapr component endpoints default to `127.0.0.1`; sidecars in Compose need
`host.docker.internal` instead. `ImageSource:Host` / `ImageSource:Port` configure
the web app's image tag helper to rewrite storage URLs for the browser.

`docker compose build` builds local application images. Docker build contexts
are the individual application directories, not the repository root; the
Dockerfiles, `compose.yaml`, `azure.yaml`, and image-build workflow depend on
this layout.

## Architecture

`ContosoAds.Web` is a Razor Pages frontend with an MVC callback controller.
Page handlers delegate database and storage operations to concrete classes in
`ContosoAds.Web.Commands`, each exposing `ExecuteAsync`. `AdsContext` uses EF
Core with Npgsql/PostgreSQL and owns the ad metadata and image URLs.

The thumbnail pipeline is asynchronous and uses Dapr **bindings**, not pub/sub:

1. `CreateOrEditAd` uploads the original through `web-storage`, saves the ad to
   obtain its ID, then sends an `ImageBlob` through `thumbnail-request-sender`.
2. The image processor's `/thumbnail-request` Minimal API receives the message,
   reads the original through `imageprocessor-storage`, and uses SkiaSharp to
   produce an aspect-preserving JPEG thumbnail with an 80-pixel longest edge.
   It stores the thumbnail as `tn-<original blob name>` and sends its URI through
   `thumbnail-result-sender`.
3. The web app's `/thumbnail-result` controller updates `Ad.ThumbnailUri`.
   `_Thumbnail.cshtml` uses HTMX to poll the Index page's Image handler every
   ten seconds until the thumbnail is available.

Azure deployment is orchestrated by `azure.yaml` and `infra/main.bicep` through
`azd up` / `azd deploy`. The web app is public, while PostgreSQL is private in
the virtual network. Images are browser-accessible blobs. Azure Dapr bindings
use managed identities; local bindings use the file-backed secret store.
`DataSource:UseEntraId` and `DataSource:ManagedIdentityClientId` control the
web app's PostgreSQL token authentication. Application Insights telemetry is
enabled when `APPLICATIONINSIGHTS_CONNECTION_STRING` is configured.

For deployment automation, use `.github/workflows/azure-dev.yml` as the source
of truth for configuration names: it expects `POSTGRES_LOGIN_PASSWORD`, not
the older `POSTGRES_LOGIN_PWD` name in the workflow setup guide.

## Cross-file conventions

- Scrutor registers commands as scoped concrete services by scanning the
  **exact** `ContosoAds.Web.Commands` namespace. Keep new commands there or
  explicitly adjust registration. Page handlers use `[FromServices]` for
  commands rather than introducing a mediator or generic handler hierarchy.
- The two apps intentionally have separate `ImageBlob` records. Keep their
  serialized `Uri` and `AdId` contract compatible when changing messages.
  Binding names, routes, queue names, and app scopes must agree across C#,
  `components/*.yaml`, and `infra/modules/environment.bicep`.
- Blob uploads use `WriteAzureBlobBase64Async`; the binding must retain
  `decodeBase64: true`, and the helper reads `blobURL` from the JSON response.
  Editing an image creates a new GUID-based blob name to avoid stale caches,
  resets its thumbnail URI, and queues rendering only after saving the ad.
- Keep the callbacks' `OPTIONS` handlers for Dapr binding discovery. Missing
  source blobs and results for already-deleted ads are acknowledged with HTTP
  200; preserve these deletion-race behaviors.
- Create/Edit pages bind the `Ad` entity with an explicit
  `TryUpdateModelAsync` property allowlist to prevent over-posting. Keep that
  allowlist deliberate when adding editable fields. The UI forces `en-US`,
  and posted dates are stored in UTC.
- Schema changes must keep EF migrations/model snapshot and
  `deploy/migrate.sql` aligned. Compose runs that SQL on first database-volume
  initialization; Azure runs it in a migration Container Instance. The web
  app does not apply migrations at startup; test helpers do.

## Further documentation

For additional setup and deployment details, consult:

- [README.md](README.md) for Azure topology and Azure Developer CLI deployment.
- [docs/build-and-run.md](docs/build-and-run.md) for native .NET/Dapr setup.
- [docs/docker.md](docs/docker.md) for the full container stack.
- [docs/github-actions-workflow.md](docs/github-actions-workflow.md) and
  [.github/workflows](.github/workflows) for deployment automation.

`.github/mcp.json` runs Context7 through `npx` for framework documentation.
It requires Node.js/npm and an exported `CONTEXT7_API_KEY` environment variable
before starting Copilot CLI. Keep the key out of checked-in configuration.

## Agent skills

### Issue tracker

Track issues and specs in GitHub Issues. Before ticket operations, read `docs/agents/issue-tracker.md`.

### Triage labels

Use the five default triage labels. Before triaging, read `docs/agents/triage-labels.md`.

### Domain docs

Use a single-context glossary and ADR layout. Before domain exploration or recording decisions, read `docs/agents/domain.md`.
