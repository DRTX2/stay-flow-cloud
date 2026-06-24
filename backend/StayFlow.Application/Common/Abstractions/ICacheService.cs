namespace StayFlow.Application.Common.Abstractions;

/// <summary>
/// A small JSON-serialising façade over the distributed cache. Backed by Redis when configured,
/// otherwise an in-memory distributed cache, so application code is agnostic to the provider.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Returns the cached value, or computes it via <paramref name="factory"/>, caches and returns it.</summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);
}
