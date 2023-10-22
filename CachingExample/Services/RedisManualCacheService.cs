// Ignore Spelling: Redis

using Newtonsoft.Json;
using StackExchange.Redis;

namespace CachingExample.Services;

public class RedisManualCacheService : ICacheService
{
    #region Fields :
    private readonly IDatabase _cacheDb;
    #endregion

    #region CTORS :
    public RedisManualCacheService(IConfiguration configuration)
    {
        var redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")!);
        _cacheDb = redis.GetDatabase();
    }
    #endregion

    #region Methods :
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await _cacheDb.StringGetAsync(key);
        if (cachedValue.IsNullOrEmpty)
            return default;

        var value = JsonConvert.DeserializeObject<T>(cachedValue!);
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
            await _cacheDb.StringSetAsync(key, cachedValueAsString, TimeSpan.FromMinutes(expirationInMinutes));
        }
    }
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cacheDb.KeyDeleteAsync(key);
    }
    #endregion
}