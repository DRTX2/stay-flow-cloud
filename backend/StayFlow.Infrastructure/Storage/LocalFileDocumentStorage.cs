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

    public async Task<IEnumerable<DocumentMetadata>> ListAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var tenantDir = Path.Combine(_root, prefix);
        if (!Directory.Exists(tenantDir))
        {
            return Enumerable.Empty<DocumentMetadata>();
        }

        var files = Directory.GetFiles(tenantDir);
        var result = new List<DocumentMetadata>();

        foreach (var file in files)
        {
            if (file.EndsWith(".contenttype", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var key = prefix + "/" + Path.GetFileName(file);
            var fileInfo = new FileInfo(file);
            var contentType = File.Exists(file + ".contenttype")
                ? await File.ReadAllTextAsync(file + ".contenttype", cancellationToken)
                : "application/octet-stream";

            result.Add(new DocumentMetadata(
                Key: key,
                Name: Path.GetFileName(file),
                Size: fileInfo.Length,
                ContentType: contentType,
                UploadedOn: fileInfo.LastWriteTimeUtc
            ));
        }

        return result;
    }

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
