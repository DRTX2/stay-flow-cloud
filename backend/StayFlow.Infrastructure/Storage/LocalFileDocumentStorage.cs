using StayFlow.Application.Common.Storage;

namespace StayFlow.Infrastructure.Storage;

/// <summary>
/// Filesystem-backed document storage used when no S3 bucket is configured, so the system runs
/// locally without cloud credentials. The MIME type is preserved in a sidecar file alongside each
/// object; download URLs are <c>file://</c> URIs.
/// </summary>
public sealed class LocalFileDocumentStorage : IDocumentStorage
{
    private readonly string _root;

    public LocalFileDocumentStorage(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var path = PathFor(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using (var file = File.Create(path))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        await File.WriteAllTextAsync(path + ".contenttype", contentType, cancellationToken);
        return key;
    }

    public async Task<StoredDocument?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = PathFor(key);
        if (!File.Exists(path))
        {
            return null;
        }

        var contentType = File.Exists(path + ".contenttype")
            ? await File.ReadAllTextAsync(path + ".contenttype", cancellationToken)
            : "application/octet-stream";

        return new StoredDocument(File.OpenRead(path), contentType);
    }

    public Task<Uri> GetDownloadUrlAsync(string key, TimeSpan expiresIn, CancellationToken cancellationToken = default)
        => Task.FromResult(new Uri(Path.GetFullPath(PathFor(key))));

    // Map a storage key onto a path under the root, guarding against traversal outside it.
    private string PathFor(string key)
    {
        var relative = key.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_root, relative));

        if (!fullPath.StartsWith(Path.GetFullPath(_root), StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Resolved path for key '{key}' escapes the storage root.");
        }

        return fullPath;
    }
}
