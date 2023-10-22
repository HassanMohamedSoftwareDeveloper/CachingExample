
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CachingExample.Services;

public class InMemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    #region Methods :
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = memoryCache.Get<T>(key);
        return Task.FromResult(cachedValue);
    }
    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, int expirationInMinutes, CancellationToken cancellationToken = default)
    {
        return await memoryCache.GetOrCreateAsync<T>(key,
              entry =>
              {
                  entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationInMinutes));
                  return factory();
              });
    }
    public Task SetAsync<T>(string key, T value, int expirationInMinutes, CancellationToken cancellationToken = default)
    {
        if (value is not null)
            memoryCache.Set(key, value, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationInMinutes) });

        return Task.CompletedTask;
    }
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(key);
        return Task.CompletedTask;
    }
    #endregion
}
