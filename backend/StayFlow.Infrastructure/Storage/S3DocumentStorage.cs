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

    public async Task<IEnumerable<DocumentMetadata>> ListAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = prefix
        };

        var response = await client.ListObjectsV2Async(request, cancellationToken);
        var result = new List<DocumentMetadata>();

        foreach (var s3Object in response.S3Objects)
        {
            var key = s3Object.Key;
            var name = key.Substring(prefix.Length).TrimStart('/');
            
            // Skip directory markers if any
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            var size = s3Object.Size;
            var uploadedOn = s3Object.LastModified;

            var extension = Path.GetExtension(name).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".csv" => "text/csv",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };

            result.Add(new DocumentMetadata(key, name, size ?? 0, contentType, uploadedOn ?? DateTime.UtcNow));
        }

        return result;
    }
}
