using ContosoAds.Web;
using ContosoAds.Web.Commands;
using ContosoAds.Web.DataAccess;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add monitoring services.
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

#pragma warning disable CA1861
builder.Services.AddHealthChecks().AddDbContextCheck<AdsContext>("AdsContext", tags: ["db_ready"]);
#pragma warning restore CA1861

// Add MVC and Razor Pages with Dapr support.
builder.Services.AddRazorPages().AddDapr();
builder.Services.AddControllers().AddDapr();

// Configure Npgsql data source and Entity Framework.
var useEntraId = builder.Configuration.GetValue("DataSource:UseEntraID", false);
builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")!, 
    useEntraId);
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
app.UseStaticFiles();

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