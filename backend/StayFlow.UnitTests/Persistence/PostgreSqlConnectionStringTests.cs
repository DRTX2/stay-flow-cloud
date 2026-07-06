using FluentAssertions;
using Npgsql;
using StayFlow.Persistence;

namespace StayFlow.UnitTests.Persistence;

public sealed class PostgreSqlConnectionStringTests
{
    [Fact]
    public void Normalize_keeps_keyword_connection_strings_unchanged()
    {
        const string connectionString = "Host=localhost;Port=5432;Database=stayflow;Username=app;Password=secret";

        PostgreSqlConnectionString.Normalize(connectionString).Should().Be(connectionString);
    }

    [Fact]
    public void Normalize_converts_neon_uri_to_npgsql_keywords()
    {
        const string connectionString =
            "postgresql://stayflow%20user:p%40ss@ep-example.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require";

        var normalized = PostgreSqlConnectionString.Normalize(connectionString);
        var builder = new NpgsqlConnectionStringBuilder(normalized);

        builder.Host.Should().Be("ep-example.us-east-2.aws.neon.tech");
        builder.Port.Should().Be(5432);
        builder.Database.Should().Be("neondb");
        builder.Username.Should().Be("stayflow user");
        builder.Password.Should().Be("p@ss");
        builder.SslMode.Should().Be(SslMode.Require);
        normalized.Should().NotContain("channel_binding");
    }
}
