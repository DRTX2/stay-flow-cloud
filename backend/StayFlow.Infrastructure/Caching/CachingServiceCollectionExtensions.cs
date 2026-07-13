using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Caching;

public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the distributed cache. Uses Redis when a connection string is supplied (and also
    /// persists Data Protection keys there so tokens survive across instances); otherwise falls
    /// back to an in-memory distributed cache so the app still runs locally without Redis.
    /// </summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        string? redisConnectionString,
        IConfiguration configuration,
        bool isDevelopment)
    {
        IDataProtectionBuilder dataProtection;
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            var redisConfiguration = ConfigurationOptions.Parse(redisConnectionString);
            redisConfiguration.AbortOnConnectFail = false;
            var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration);

            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(multiplexer);
                options.InstanceName = "stayflow:";
            });

            dataProtection = services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(multiplexer, "stayflow:dataprotection-keys")
                .SetApplicationName("StayFlow");
        }
        else
        {
            services.AddDistributedMemoryCache();
            dataProtection = services.AddDataProtection()
                .PersistKeysToDbContext<StayFlowDbContext>()
                .SetApplicationName("StayFlow");
        }

        if (!isDevelopment)
        {
            dataProtection.ProtectKeysWithCertificate(DependencyInjection.LoadOpenIddictCertificate(configuration));
        }

        services.AddScoped<ICacheService, DistributedCacheService>();
        return services;
    }
}
