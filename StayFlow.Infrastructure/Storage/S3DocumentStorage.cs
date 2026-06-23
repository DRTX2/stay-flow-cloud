using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using StayFlow.Application.Common.Storage;

namespace StayFlow.Infrastructure.Storage;

/// <summary>
/// Amazon S3-backed document storage. Works against real S3 or any S3-compatible endpoint
/// (MinIO/LocalStack) depending on how the <see cref="IAmazonS3"/> client is configured.
/// </summary>
public sealed class S3DocumentStorage(IAmazonS3 client, string bucketName) : IDocumentStorage
{
    public async Task<string> SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false,
            },
            cancellationToken);

        return key;
    }

    public async Task<StoredDocument?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetObjectAsync(bucketName, key, cancellationToken);
            return new StoredDocument(response.ResponseStream, response.Headers.ContentType ?? "application/octet-stream");
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Uri> GetDownloadUrlAsync(string key, TimeSpan expiresIn, CancellationToken cancellationToken = default)
    {
        var url = await client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiresIn),
        });

        return new Uri(url);
    }
}
