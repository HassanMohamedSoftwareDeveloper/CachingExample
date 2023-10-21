using CachingExample.Database;
using CachingExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace CachingExample.Repositories;

public class DriverRepository(CachingDbContext dbContext) : IDriverRepository
{
    public async Task<List<Driver>?> GetDrivers(CancellationToken cancellationToken = default)
    {
        return await dbContext.Drivers
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);
    }
    public async Task<Driver?> GetDriverById(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Drivers
           .AsNoTracking()
           .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }
    public async Task<Driver?> AddDriver(Driver driver, CancellationToken cancellationToken = default)
    {
        var addedEntity = await dbContext.Drivers
             .AddAsync(driver, cancellationToken: cancellationToken);
        var saveResult = await dbContext.SaveChangesAsync(cancellationToken);
        return saveResult > 0 ? addedEntity.Entity : default;
    }
    public async Task<bool> DeleteDriver(int id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Drivers
             .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        if (entity is not { })
            return false;
        dbContext.Drivers.Remove(entity);
        var saveResult = await dbContext.SaveChangesAsync(cancellationToken);
        return saveResult > 0;

    }
}
