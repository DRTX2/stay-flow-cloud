namespace StayFlow.Application.Common.Storage;

/// <summary>A retrieved document: its content stream and MIME type.</summary>
public sealed record StoredDocument(Stream Content, string ContentType);

/// <summary>
/// Object storage for tenant documents (invoices, contracts, uploads). Backed by Amazon S3
/// (or an S3-compatible endpoint) in deployed environments, with a local-filesystem fallback so
/// the system runs without cloud credentials. Keys are caller-defined and conventionally prefixed
/// with the tenant id for isolation.
/// </summary>
public interface IDocumentStorage
{
    Task<string> SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task<StoredDocument?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>A time-limited URL for direct download (S3 pre-signed URL; file URI locally).</summary>
    Task<Uri> GetDownloadUrlAsync(string key, TimeSpan expiresIn, CancellationToken cancellationToken = default);
}
