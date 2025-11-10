using Microsoft.EntityFrameworkCore;
using SAO.API;
using SAO.Application.Interfaces;
using SAO.Application.Services;
using SAO.Domain.Entities.Configuration;
using SAO.Domain.Repository;
using SAO.Infrastructure.Repositories.Api;
using SAO.Infrastructure.Repositories.Csv;
using SAO.Infrastructure.Repositories.Db;

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

builder.Services.AddSingleton(ClassificationConfiguration.Default);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IExtractor<object>, CsvExtractor>();
builder.Services.AddScoped<IExtractor<object>, DbExtractor>();
builder.Services.AddScoped<IExtractor<object>, ApiExtractor>();


builder.Services.AddScoped<IClassificationService, ClassificationService>();
builder.Services.AddScoped<IETLService, ETLService>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();


Console.WriteLine("Verificando base de datos...");
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Base de datos lista");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error con la base de datos: {ex.Message}");
        throw;
    }
}

Console.WriteLine("Iniciando Worker Service...");
await host.RunAsync();