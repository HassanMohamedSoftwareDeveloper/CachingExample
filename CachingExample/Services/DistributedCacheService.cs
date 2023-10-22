
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace CachingExample.Services;

public class DistributedCacheService(IDistributedCache distributeCache) : ICacheService
{
    #region Methods :
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await distributeCache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(cachedValue))
            return default;

        var value = JsonConvert.DeserializeObject<T>(cachedValue);
        return value;
    }
    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, int expirationInMinutes, CancellationToken cancellationToken = default)
    {

        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue is not null)
            return cachedValue;

        cachedValue = await factory();
        await SetAsync(key, cachedValue, expirationInMinutes, cancellationToken);
        return cachedValue;
    }
    public async Task SetAsync<T>(string key, T value, int expirationInMinutes, CancellationToken cancellationToken = default)
    {
        if (value is not null)
        {
            var cachedValueAsString = JsonConvert.SerializeObject(value);

            await distributeCache.SetStringAsync(key,
                                                 cachedValueAsString,
                                                 new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationInMinutes) },
                                                 token: cancellationToken);
        }
    }
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributeCache.RemoveAsync(key, cancellationToken);
    }
    #endregion
}
