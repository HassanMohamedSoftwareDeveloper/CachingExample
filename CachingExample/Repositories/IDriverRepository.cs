using CachingExample.Entities;

namespace CachingExample.Repositories;

public interface IDriverRepository
{
    Task<List<Driver>?> GetDrivers(CancellationToken cancellationToken = default);
    Task<Driver?> GetDriverById(int id, CancellationToken cancellationToken = default);
    Task<Driver?> AddDriver(Driver driver, CancellationToken cancellationToken = default);
    Task<bool> DeleteDriver(int id, CancellationToken cancellationToken = default);
}
