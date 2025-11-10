using SAO.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SAO.API;


/// Worker Service

/// <summary>
/// Worker Service que ejecuta el proceso ETL de forma programada
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("Worker Service ETL iniciado");
        _logger.LogInformation("Fecha: {Date}", DateTime.Now);
        _logger.LogInformation("========================================");

        // Leer configuración
        var runOnStartup = _configuration.GetValue<bool>("ETL:RunOnStartup", true);
        var intervalMinutes = _configuration.GetValue<int>("ETL:IntervalMinutes", 60);

        // Ejecutar inmediatamente si está configurado
        if (runOnStartup)
        {
            _logger.LogInformation("Ejecutando ETL al inicio...");
            await ExecuteETLProcessAsync(stoppingToken);
        }

        // Bucle principal - ejecutar periódicamente
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Esperando {Minutes} minutos para la próxima ejecución...", intervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteETLProcessAsync(stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Worker Service detenido");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el Worker Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Ejecuta el proceso ETL completo
    /// </summary>
    private async Task ExecuteETLProcessAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("");
        _logger.LogInformation("==================================================");
        _logger.LogInformation("INICIANDO PROCESO ETL - {Date}", DateTime.Now);
        _logger.LogInformation("==================================================");

        try
        {
            // Crear un scope para resolver servicios
            using var scope = _serviceProvider.CreateScope();
            var etlService = scope.ServiceProvider.GetRequiredService<IETLService>();

            // Ejecutar el proceso ETL
            var result = await etlService.ExecuteAsync(cancellationToken);

            // Mostrar resultado
            if (result.Success)
            {
                _logger.LogInformation("");
                _logger.LogInformation("==================================================");
                _logger.LogInformation("ETL COMPLETADO EXITOSAMENTE");
                _logger.LogInformation("==================================================");
                _logger.LogInformation("Total Extraído:     {Count}", result.TotalExtracted);
                _logger.LogInformation("Total Transformado: {Count}", result.TotalTransformed);
                _logger.LogInformation("Total Cargado:      {Count}", result.TotalLoaded);
                _logger.LogInformation("Total Errores:      {Count}", result.TotalErrors);
                _logger.LogInformation("Duración:           {Duration} ms", result.DurationMs);
                _logger.LogInformation("==================================================");
                _logger.LogInformation("");

                // Mostrar detalles por fuente
                if (result.ExtractedBySource.Any())
                {
                    _logger.LogInformation("Detalle por fuente:");
                    foreach (var source in result.ExtractedBySource)
                    {
                        _logger.LogInformation("  - {Source}: {Count} registros", source.Key, source.Value);
                    }
                }

                // Mostrar advertencias
                if (result.Warnings.Any())
                {
                    _logger.LogWarning("");
                    _logger.LogWarning("Advertencias ({Count}):", result.Warnings.Count);
                    foreach (var warning in result.Warnings)
                    {
                        _logger.LogWarning("  - {Warning}", warning);
                    }
                }
            }
            else
            {
                _logger.LogError("");
                _logger.LogError("==================================================");
                _logger.LogError("ETL FINALIZADO CON ERRORES");
                _logger.LogError("==================================================");
                _logger.LogError("Error: {Error}", result.ErrorMessage);
                _logger.LogError("==================================================");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error crítico ejecutando el proceso ETL");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker Service ETL deteniéndose...");
        await base.StopAsync(cancellationToken);
    }
}