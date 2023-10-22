using CachingExample.Entities;
using CachingExample.Services;

namespace CachingExample.Repositories;
public class CachedDriverRepository : IDriverRepository
{
    #region Fields :
    private readonly int _cacheExpirationInMinutes;
    private readonly IDriverRepository _decorated;
    private readonly ICacheService _cacheService;
    #endregion

    #region CTORS :
    public CachedDriverRepository(IDriverRepository decorated, IConfiguration configuration, ICacheService cacheService)
    {
        _cacheExpirationInMinutes = int.Parse(configuration["CacheExpirationInMinutes"]!);
        _decorated = decorated;
        _cacheService = cacheService;
    }
    #endregion

    #region Methods :
    public async Task<List<Driver>?> GetDrivers(CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(CacheKeys.Driver.ListKey,
                                                    () => _decorated.GetDrivers(cancellationToken),
                                                    _cacheExpirationInMinutes,
                                                    cancellationToken);
    }
    public async Task<Driver?> GetDriverById(int id, CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(string.Format(CacheKeys.Driver.ByIdKey, id),
                                                   () => _decorated.GetDriverById(id, cancellationToken),
                                                   _cacheExpirationInMinutes,
                                                   cancellationToken);
    }
    public async Task<Driver?> AddDriver(Driver driver, CancellationToken cancellationToken = default)
    {
        var addedDriver = await _decorated.AddDriver(driver, cancellationToken);
        if (addedDriver is not { })
            return addedDriver;

        await _cacheService.SetAsync(string.Format(CacheKeys.Driver.ByIdKey, addedDriver.Id), addedDriver, _cacheExpirationInMinutes, cancellationToken);
        return addedDriver;
    }
    public async Task<bool> DeleteDriver(int id, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync(string.Format(CacheKeys.Driver.ByIdKey, id), cancellationToken);
        return await _decorated.DeleteDriver(id, cancellationToken);
    }
    #endregion
}
