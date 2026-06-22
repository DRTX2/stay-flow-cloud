using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Persistence.Interceptors;

namespace StayFlow.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<StayFlowDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(StayFlowDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure();
            });
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
                serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<StayFlowDbContext>());

        return services;
    }
}
