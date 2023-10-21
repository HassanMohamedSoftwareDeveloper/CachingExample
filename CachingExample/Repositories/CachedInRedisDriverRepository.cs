// Ignore Spelling: Redis

using CachingExample.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CachingExample.Repositories;

public class CachedInRedisDriverRepository : IDriverRepository
{
    #region Fields :
    private readonly int _cacheExpirationInMinutes;
    private readonly IDriverRepository _decorated;
    private readonly IDistributedCache _distributedCache;
    #endregion

    #region CTORS :
    public CachedInRedisDriverRepository(IDriverRepository decorated, IDistributedCache distributedCache, IConfiguration configuration)
    {
        _cacheExpirationInMinutes = int.Parse(configuration["CacheExpirationInMinutes"]!);
        _decorated = decorated;
        _distributedCache = distributedCache;
    }
    #endregion

    #region Methods :
    public async Task<List<Driver>?> GetDrivers(CancellationToken cancellationToken = default)
    {
        string key = "drivers";
        var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (cachedValue is { })
        {
            return JsonSerializer.Deserialize<List<Driver>>(cachedValue);
        }

        var drivers = await _decorated.GetDrivers(cancellationToken);
        if (drivers is { } and { Count: > 0 })
        {
            await _distributedCache.SetStringAsync(key,
                                                   JsonSerializer.Serialize(drivers),
                                                   new DistributedCacheEntryOptions
                                                   {
                                                       AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheExpirationInMinutes)
                                                   },
                                                   cancellationToken);
        }

        return drivers;
    }
    public async Task<Driver?> GetDriverById(int id, CancellationToken cancellationToken = default)
    {
        string key = $"driver-{id}";

        var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (cachedValue is { })
        {
            return JsonSerializer.Deserialize<Driver>(cachedValue);
        }

        var driver = await _decorated.GetDriverById(id, cancellationToken);
        if (driver is { })
        {
            await _distributedCache.SetStringAsync(key,
                                                   JsonSerializer.Serialize(driver),
                                                   new DistributedCacheEntryOptions
                                                   {
                                                       AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheExpirationInMinutes)
                                                   },
                                                   cancellationToken);
        }

        return driver;
    }
    public async Task<Driver?> AddDriver(Driver driver, CancellationToken cancellationToken = default)
    {
        var addedDriver = await _decorated.AddDriver(driver, cancellationToken);
        if (addedDriver is not { })
            return addedDriver;

        string key = $"driver-{addedDriver.Id}";
        await _distributedCache.SetStringAsync(key,
                                               JsonSerializer.Serialize(addedDriver),
                                               new DistributedCacheEntryOptions
                                               {
                                                   AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheExpirationInMinutes)
                                               },
                                               cancellationToken);
        return addedDriver;
    }
    public async Task<bool> DeleteDriver(int id, CancellationToken cancellationToken = default)
    {
        string key = $"driver-{id}";
        await _distributedCache.RemoveAsync(key, cancellationToken);
        return await _decorated.DeleteDriver(id, cancellationToken);


    }
    #endregion
}