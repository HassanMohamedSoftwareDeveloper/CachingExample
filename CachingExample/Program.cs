using CachingExample.Database;
using CachingExample.Repositories;
using CachingExample.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CachingDbContext>(config =>
{
    config.EnableDetailedErrors();
    config.EnableSensitiveDataLogging();
    config.UseSqlServer(builder.Configuration.GetConnectionString("CachingDbConnection"), options =>
    {
        options.MigrationsHistoryTable("CachingMigrations", "CACHING");
    });
});
//builder.Services.AddSingleton<ICacheService, RedisManualCacheService>();
//builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
builder.Services.AddSingleton<ICacheService, DistributedCacheService>();

//builder.Services.AddScoped<DriverRepository>();
//builder.Services.AddScoped<IDriverRepository, CachedDriverRepository>();

//builder.Services.AddScoped<IDriverRepository>(provider =>
//{
//    var driverRepository = provider.GetRequiredService<DriverRepository>();
//    var configuration = provider.GetRequiredService<IConfiguration>();
//    var cacheService = provider.GetRequiredService<ICacheService>();
//    return new CachedDriverRepository(driverRepository, configuration, cacheService);
//});

//Scurtor
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.Decorate<IDriverRepository, CachedDriverRepository>();



builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    string redisConnection = builder.Configuration.GetConnectionString("RedisConnection")!;
    redisOptions.Configuration = redisConnection;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
await app.PopulateDatabasePreparation();
app.Run();