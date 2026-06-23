using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayFlow.Application.Common.Storage;

namespace StayFlow.Infrastructure.Storage;

public static class StorageServiceCollectionExtensions
{
    /// <summary>
    /// Registers document storage. Uses Amazon S3 when a bucket is configured (with an optional
    /// custom endpoint for MinIO/LocalStack); otherwise falls back to local-filesystem storage so
    /// the system runs without cloud credentials.
    /// </summary>
    public static IServiceCollection AddDocumentStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var bucket = configuration["Storage:S3:Bucket"];

        if (string.IsNullOrWhiteSpace(bucket))
        {
            var localPath = configuration["Storage:LocalPath"]
                ?? Path.Combine(Path.GetTempPath(), "stayflow-documents");

            services.AddSingleton<IDocumentStorage>(new LocalFileDocumentStorage(localPath));
            return services;
        }

        var s3Config = new AmazonS3Config();

        var serviceUrl = configuration["Storage:S3:ServiceUrl"];
        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            // S3-compatible endpoint (MinIO/LocalStack) needs path-style addressing.
            s3Config.ServiceURL = serviceUrl;
            s3Config.ForcePathStyle = true;
        }
        else if (configuration["Storage:S3:Region"] is { Length: > 0 } region)
        {
            s3Config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
        }

        // Explicit keys support MinIO/LocalStack; otherwise rely on the default AWS credential chain
        // (env vars, profile, IAM role) — never hard-code credentials in deployed environments.
        var accessKey = configuration["Storage:S3:AccessKey"];
        var secretKey = configuration["Storage:S3:SecretKey"];
        IAmazonS3 client = !string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey)
            ? new AmazonS3Client(accessKey, secretKey, s3Config)
            : new AmazonS3Client(s3Config);

        services.AddSingleton(client);
        services.AddSingleton<IDocumentStorage>(sp => new S3DocumentStorage(sp.GetRequiredService<IAmazonS3>(), bucket));
        return services;
    }
}
