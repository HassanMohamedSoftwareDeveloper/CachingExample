using StackExchange.Redis;
using System.Text.Json;

namespace CachingExample.Services;

public class CacheService : ICacheService
{
    #region Fields :
    private readonly IDatabase _cacheDb;
    private readonly int _cacheExpirationInMinutes;
    #endregion

    #region CTORS :
    public CacheService(IConfiguration configuration)
    {
        var redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")!);
        _cacheDb = redis.GetDatabase();
        _cacheExpirationInMinutes = int.Parse(configuration["CacheExpirationInMinutes"]!);
    }
    #endregion

    #region Methods :
    public T? GetData<T>(string key)
    {
        RedisValue value = _cacheDb.StringGet(key);

        return value.IsNullOrEmpty
            ? default
            : JsonSerializer.Deserialize<T>(value.ToString());
    }
    public bool SetData<T>(string key, T value)
    {
        var expiryTime = DateTimeOffset.Now.AddMinutes(_cacheExpirationInMinutes).Subtract(DateTime.Now);
        return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiry: expiryTime);
    }
    public bool RemoveData(string key)
    {
        return _cacheDb.KeyDelete(key);
    }
    #endregion
}