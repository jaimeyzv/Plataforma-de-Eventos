namespace EventService.Application.Abstractions;

/// <summary>
/// Thin abstraction over the distributed cache (Redis) so the application layer
/// stays free of infrastructure concerns. Implementations are resilient: a cache
/// outage must never break the request (fail-open).
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);

    Task SetAsync<T>(string key, T value, TimeSpan? ttl, CancellationToken cancellationToken);

    Task RemoveAsync(string key, CancellationToken cancellationToken);
}
