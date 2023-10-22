namespace CachingExample.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, int expirationInMinutes, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, int expirationInMinutes, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
