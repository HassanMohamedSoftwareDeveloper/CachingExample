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
builder.Services.AddScoped<ICacheService, CacheService>();

//builder.Services.AddScoped<DriverRepository>();
//builder.Services.AddScoped<IDriverRepository, CachedDriverRepository>();

//builder.Services.AddScoped<IDriverRepository>(provider =>
//{
//    var driverRepository = provider.GetRequiredService<DriverRepository>();
//    var memoryCache = provider.GetRequiredService<IMemoryCache>();
//    var configuration = provider.GetRequiredService<IConfiguration>();
//    return new CachedDriverRepository(driverRepository, memoryCache, configuration);
//});

//Scurtor
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
//builder.Services.Decorate<IDriverRepository, CachedInMemoryDriverRepository>();
builder.Services.Decorate<IDriverRepository, CachedInRedisDriverRepository>();



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