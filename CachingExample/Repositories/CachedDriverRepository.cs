using CachingExample.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CachingExample.Repositories;
public class CachedInMemoryDriverRepository : IDriverRepository
{
    #region Fields :
    private readonly int _cacheExpirationInMinutes;
    private readonly IDriverRepository _decorated;
    private readonly IMemoryCache _memoryCache;
    #endregion

    #region CTORS :
    public CachedInMemoryDriverRepository(IDriverRepository decorated, IMemoryCache memoryCache, IConfiguration configuration)
    {
        _cacheExpirationInMinutes = int.Parse(configuration["CacheExpirationInMinutes"]!);
        _decorated = decorated;
        _memoryCache = memoryCache;
    }
    #endregion

    #region Methods :
    public async Task<List<Driver>?> GetDrivers(CancellationToken cancellationToken = default)
    {
        string key = "drivers";
        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheExpirationInMinutes));
            return _decorated.GetDrivers(cancellationToken);
        });
    }
    public async Task<Driver?> GetDriverById(int id, CancellationToken cancellationToken = default)
    {
        string key = $"driver-{id}";
        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheExpirationInMinutes));
            return _decorated.GetDriverById(id, cancellationToken);
        });
    }
    public async Task<Driver?> AddDriver(Driver driver, CancellationToken cancellationToken = default)
    {
        var addedDriver = await _decorated.AddDriver(driver, cancellationToken);
        if (addedDriver is not { })
            return addedDriver;

        string key = $"driver-{addedDriver.Id}";
        _memoryCache.Set(key, addedDriver, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheExpirationInMinutes) });
        return addedDriver;
    }
    public async Task<bool> DeleteDriver(int id, CancellationToken cancellationToken = default)
    {
        string key = $"driver-{id}";
        _memoryCache.Remove(key);
        return await _decorated.DeleteDriver(id, cancellationToken);


    }
    #endregion
}
