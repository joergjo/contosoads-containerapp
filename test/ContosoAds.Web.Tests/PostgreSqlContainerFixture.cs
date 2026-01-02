using System.Data.Common;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace ContosoAds.Web.Tests;

public class PostgreSqlContainerFixture(IMessageSink sink)
    : DbContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(sink)
{
    public override DbProviderFactory DbProviderFactory => NpgsqlFactory.Instance;

    protected override PostgreSqlBuilder Configure() => new("postgres:17-alpine");
}