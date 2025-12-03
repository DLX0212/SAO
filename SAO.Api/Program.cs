using Microsoft.EntityFrameworkCore;
using SAO.API;
using SAO.Application.Interfaces;
using SAO.Application.Services;
using SAO.Domain.Entities.Configuration;
using SAO.Domain.Repository;
using SAO.Infrastructure.Repositories.Api;
using SAO.Infrastructure.Repositories.Csv;
using SAO.Infrastructure.Repositories.Db;
using SAO.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var connectionString = builder.Configuration.GetConnectionString("AnalyticalDatabase");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(120);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});

builder.Services.AddHttpClient("SocialMediaAPI", client =>
{
    var apiUrl = builder.Configuration["DataSources:API:BaseUrl"];
    if (!string.IsNullOrEmpty(apiUrl))
    {
        client.BaseAddress = new Uri(apiUrl);
    }
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddSingleton(sp =>
{
    var config = ETLConfiguration.Default;

    var runOnStartup = builder.Configuration.GetValue<bool?>("ETL:RunOnStartup");
    var intervalMinutes = builder.Configuration.GetValue<int?>("ETL:IntervalMinutes");
    var batchSize = builder.Configuration.GetValue<int?>("ETL:BatchSize");

    if (runOnStartup.HasValue || intervalMinutes.HasValue || batchSize.HasValue)
    {
        config = config.With(
            runOnStartup: runOnStartup,
            intervalMinutes: intervalMinutes,
            batchSize: batchSize
        );
    }

    return config;
});
builder.Services.AddScoped<DimensionLoaderService>();

builder.Services.AddSingleton(ClassificationConfiguration.Default);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IExtractor<object>, CsvExtractor>();
builder.Services.AddScoped<IExtractor<object>, DbExtractor>();
builder.Services.AddScoped<IExtractor<object>, ApiExtractor>();


builder.Services.AddScoped<IClassificationService, ClassificationService>();
builder.Services.AddScoped<IETLService, ETLService>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();

Console.WriteLine("Verificando Data Warehouse...");
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var loader = scope.ServiceProvider.GetRequiredService<DimensionLoaderService>();

    try
    {
        // Esto creará las tablas DIM_PRODUCTO, DIM_CLIENTE, etc.
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Esquema de base de datos verificado.");

        // Ejecutar carga de dimensiones
        Console.WriteLine("Iniciando carga de Dimensiones (CSV)...");
        await loader.CargarDimensionesAsync();
        Console.WriteLine("Carga de dimensiones finalizada.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error crítico: {ex.Message}");
    }
}

Console.WriteLine("Iniciando Worker Service...");
await host.RunAsync();