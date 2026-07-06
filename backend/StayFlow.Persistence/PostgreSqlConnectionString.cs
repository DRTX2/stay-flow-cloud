using Npgsql;

namespace StayFlow.Persistence;

public static class PostgreSqlConnectionString
{
    public static string Normalize(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return connectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.IdnHost,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            SslMode = SslMode.Require,
        };

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            builder.Username = Uri.UnescapeDataString(parts[0]);

            if (parts.Length > 1)
            {
                builder.Password = Uri.UnescapeDataString(parts[1]);
            }
        }

        foreach (var parameter in ParseQuery(uri.Query))
        {
            switch (parameter.Key.ToLowerInvariant())
            {
                case "sslmode":
                case "ssl mode":
                    builder.SslMode = ParseSslMode(parameter.Value);
                    break;
                case "application_name":
                case "application name":
                    builder.ApplicationName = parameter.Value;
                    break;
                case "connect_timeout":
                case "connect timeout":
                    if (int.TryParse(parameter.Value, out var timeout))
                    {
                        builder.Timeout = timeout;
                    }
                    break;
            }
        }

        return builder.ConnectionString;
    }

    private static SslMode ParseSslMode(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "disable" => SslMode.Disable,
            "allow" => SslMode.Allow,
            "prefer" => SslMode.Prefer,
            "require" => SslMode.Require,
            "verify-ca" => SslMode.VerifyCA,
            "verify-full" => SslMode.VerifyFull,
            _ => SslMode.Require,
        };
    }

    private static IEnumerable<KeyValuePair<string, string>> ParseQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            yield break;
        }

        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            var key = Uri.UnescapeDataString(pair[0].Replace('+', ' '));
            var value = pair.Length > 1 ? Uri.UnescapeDataString(pair[1].Replace('+', ' ')) : string.Empty;

            yield return new KeyValuePair<string, string>(key, value);
        }
    }
}
