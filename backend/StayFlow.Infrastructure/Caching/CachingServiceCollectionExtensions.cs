using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Infrastructure.Caching;

public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the distributed cache. Uses Redis when a connection string is supplied (and also
    /// persists Data Protection keys there so tokens survive across instances); otherwise falls
    /// back to an in-memory distributed cache so the app still runs locally without Redis.
    /// </summary>
    public static IServiceCollection AddCaching(this IServiceCollection services, string? redisConnectionString)
    {
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            var configuration = ConfigurationOptions.Parse(redisConnectionString);
            configuration.AbortOnConnectFail = false;
            var multiplexer = ConnectionMultiplexer.Connect(configuration);

            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(multiplexer);
                options.InstanceName = "stayflow:";
            });

            services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(multiplexer, "stayflow:dataprotection-keys")
                .SetApplicationName("StayFlow");
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, DistributedCacheService>();
        return services;
    }
}
