using Azure.Monitor.OpenTelemetry.AspNetCore;
using ContosoAds.Web;
using ContosoAds.Web.Commands;
using ContosoAds.Web.DataAccess;
using ContosoAds.Web.TagHelpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add monitoring services.
var appInsightsConnectionString = builder.Configuration.GetValue<string?>(
    "APPLICATIONINSIGHTS_CONNECTION_STRING");
if (appInsightsConnectionString is { Length: > 0 })
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options => options.EnableLiveMetrics = true);
    builder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(options => options.RecordException = true);
    builder.Services.ConfigureOpenTelemetryTracerProvider((_, configure) => configure.AddNpgsql());
    builder.Services.ConfigureOpenTelemetryMeterProvider((_, configure) => configure.AddNpgsqlInstrumentation());
}

#pragma warning disable CA1861
builder.Services.AddHealthChecks().AddDbContextCheck<AdsContext>("AdsContext", tags: ["db_ready"]);
#pragma warning restore CA1861

// Add MVC and Razor Pages with Dapr support.
builder.Services.AddRazorPages().AddDapr();
builder.Services.AddControllers().AddDapr();

// Initialize TagHelper to adjust <img src="..."> URLs when running the
// entire application stack in Docker. In this case, URLs must be rewritten
// from "host.docker.internal" to "127.0.0.1".
var imgSrcHost = builder.Configuration.GetValue<string?>("ImageSource:Host");
var imgSrcPort = builder.Configuration.GetValue<int?>("ImageSource:Port");
builder.Services.AddSingleton<ITagHelperInitializer<ImgTagHelper>>(
    new ImgTagHelperInitializer(imgSrcHost, imgSrcPort));

// Configure Npgsql data source and Entity Framework.
var useEntraId = builder.Configuration.GetValue(
    "DataSource:UseEntraId",
    false);
var managedIdentityClientId = builder.Configuration.GetValue(
    "DataSource:ManagedIdentityClientId",
    default(string));

builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")!,
    useEntraId,
    managedIdentityClientId);
builder.Services.AddDbContext<AdsContext>((sp, options) =>
{
    var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
    options.UseNpgsql(dataSource, pgOptions => pgOptions.EnableRetryOnFailure(3));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDataProtection().PersistKeysToDbContext<AdsContext>();

// We're using Scrutor to register all the command handlers.
builder.Services.Scan(scan =>
    scan.FromAssemblyOf<Program>()
        .AddClasses(classes => classes.InExactNamespaceOf<ListAds>())
        .AsSelf()
        .WithScopedLifetime());

// Force en-US for a consistent culture.
string[] supportedCultures = ["en-US"];
var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

var app = builder.Build();
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseRouting();

app.UseAuthorization();

var port = builder.Configuration.GetValue("HealthCheck:Port", 0);
app.UseHealthChecks(port, "db_ready");
app.MapRazorPages();
app.MapControllers();

app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program
{
}