using SAO.Application.Exceptions;
using SAO.Application.Interfaces;
using SAO.Application.Result;
using SAO.Domain.Entities;
using SAO.Domain.Entities.Configuration;
using SAO.Domain.Repository;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SAO.Application.Services
{
    public class ETLService : IETLService
    {
        private readonly ILogger<ETLService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClassificationService _classificationService;
        private readonly IEnumerable<IExtractor<object>> _extractors;
        private readonly ETLConfiguration _config;

        public ETLService(
            ILogger<ETLService> logger,
            IUnitOfWork unitOfWork,
            IClassificationService classificationService,
            IEnumerable<IExtractor<object>> extractors,
            ETLConfiguration config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _classificationService = classificationService;
            _extractors = extractors;
            _config = config;
        }

        public async Task<ETLResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ETLResult();

            _logger.LogInformation("========== INICIANDO PROCESO ETL ==========");

            try
            {
                if (!_config.IsValid(out var configError))
                {
                    throw new ETLException($"Configuración inválida: {configError}");
                }

                //  OBTENER LISTAS DE VALIDACIÓN 
                // Cargamos los IDs existentes para validar integridad referencial
                _logger.LogInformation("Cargando catálogos para validación...");

                var productosDb = await _unitOfWork.Productos.GetAllAsync(cancellationToken);
                var clientesDb = await _unitOfWork.Clientes.GetAllAsync(cancellationToken);

                var validProductIds = productosDb.Select(p => p.IdProducto).ToHashSet();
                var validClientIds = clientesDb.Select(c => c.IdCliente).ToHashSet();

                _logger.LogInformation("Catálogos en memoria: {ProdCount} productos, {ClientCount} clientes.", validProductIds.Count, validClientIds.Count);


                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // FASE 1: EXTRACCIÓN
                _logger.LogInformation("FASE 1: Extrayendo datos de todas las fuentes...");
                var extractedData = await ExtractAllDataAsync(result, cancellationToken);
                result.TotalExtracted = extractedData.Count;

                // FASE 2: TRANSFORMACIÓN Y LIMPIEZA
                _logger.LogInformation("FASE 2: Transformando y limpiando datos...");
                // Pasamos los IDs válidos para filtrar
                var opinions = TransformToOpinions(extractedData, result, validProductIds, validClientIds);
                result.TotalTransformed = opinions.Count;

                // FASE 3: CLASIFICACIÓN
                _logger.LogInformation("FASE 3: Clasificando opiniones...");
                ClassifyAllOpinions(opinions);

                // FASE 4: LIMPIEZA DE TABLA (STAGING)
                _logger.LogInformation("LIMPIEZA: Eliminando datos existentes en FACT_OPINION...");
                await _unitOfWork.ClearFactTablesAsync(cancellationToken);
                _logger.LogInformation("✓ Tabla de hechos limpiada correctamente.");

                // FASE 5: CARGA
                _logger.LogInformation("FASE 5: Cargando {Count} opiniones validadas...", opinions.Count);
                var loaded = await LoadOpinionsAsync(opinions, cancellationToken);
                result.TotalLoaded = loaded;

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                stopwatch.Stop();
                result.DurationMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "========== ETL COMPLETADO EXITOSAMENTE ==========\n" +
                    "Extraídos:      {Extracted}\n" +
                    "Transformados:  {Transformed}\n" +
                    "Cargados:       {Loaded}\n" +
                    "Errores/Skip:   {Errors}\n" +
                    "Duración:       {Duration}ms",
                    result.TotalExtracted,
                    result.TotalTransformed,
                    result.TotalLoaded,
                    result.TotalErrors,
                    result.DurationMs);
            }
            catch (Exception ex)
            {
                result.MarkAsFailed(ex.Message);
                _logger.LogError(ex, "Error crítico en el proceso ETL");

                try
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error al revertir la transacción");
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            return result;
        }

        private async Task<List<Opinion>> ExtractAllDataAsync(ETLResult result, CancellationToken cancellationToken)
        {
            var allOpinions = new List<Opinion>();

            foreach (var extractor in _extractors)
            {
                try
                {
                    _logger.LogInformation("→ Extrayendo de: {Source}", extractor.SourceName);
                    var data = await extractor.ExtractAsync(cancellationToken);
                    var opinions = data.OfType<Opinion>().ToList();
                    allOpinions.AddRange(opinions);
                    result.ExtractedBySource[extractor.SourceName] = opinions.Count;
                    _logger.LogInformation("✓ Extraídos {Count} registros", opinions.Count);
                }
                catch (Exception ex)
                {
                    // No contamos como error crítico, permitimos continuar con otras fuentes
                    result.AddWarning($"Fallo parcial en fuente {extractor.SourceName}: {ex.Message}");
                    _logger.LogWarning("Saltando fuente {Source} por error de conexión.", extractor.SourceName);
                }
            }
            return allOpinions;
        }

        private List<Opinion> TransformToOpinions(
            List<Opinion> extractedData,
            ETLResult result,
            HashSet<int> validProductIds,
            HashSet<int> validClientIds)
        {
            var validOpinions = new List<Opinion>();

            foreach (var opinion in extractedData)
            {
                try
                {
                    // 1. Limpieza básica
                    opinion.Comentario = CleanComment(opinion.Comentario);

                    // 2. Validación de Integridad Referencial (Producto)
                    // Si el producto no existe en DIM_PRODUCTO, no podemos insertar (FK Violation)
                    if (!validProductIds.Contains(opinion.IdProducto))
                    {
                        result.TotalErrors++;
                        // Opcional: Loguear solo los primeros errores para no saturar la consola
                        // _logger.LogWarning("Opinión ignorada: Producto ID {Id} no existe.", opinion.IdProducto);
                        continue;
                    }

                    // 3. Validación de Integridad Referencial (Cliente)
                    // Si el cliente no existe, lo hacemos anónimo (NULL) para salvar la opinión
                    if (opinion.IdCliente.HasValue && !validClientIds.Contains(opinion.IdCliente.Value))
                    {
                        opinion.IdCliente = null;
                    }

                    // 4. Validación de negocio
                    if (!opinion.IsValid())
                    {
                        result.TotalErrors++;
                        continue;
                    }

                    validOpinions.Add(opinion);
                }
                catch (Exception ex)
                {
                    result.TotalErrors++;
                    _logger.LogWarning(ex, "Error transformando opinión {Id}", opinion.IdOpinion);
                }
            }

            return validOpinions;
        }

        private void ClassifyAllOpinions(List<Opinion> opinions)
        {
            // Procesamiento paralelo para mayor velocidad
            Parallel.ForEach(opinions, opinion =>
            {
                try
                {
                    _classificationService.ProcessOpinion(opinion);
                }
                catch { /* Ignorar fallos puntuales de clasificación */ }
            });
            _logger.LogInformation("✓ Clasificadas {Count} opiniones", opinions.Count);
        }

        private async Task<int> LoadOpinionsAsync(List<Opinion> opinions, CancellationToken cancellationToken)
        {
            int totalLoaded = 0;
            int batchSize = _config.BatchSize;

            for (int i = 0; i < opinions.Count; i += batchSize)
            {
                var batch = opinions.Skip(i).Take(batchSize).ToList();
                var loaded = await _unitOfWork.Opiniones.AddRangeAsync(batch, cancellationToken);
                totalLoaded += loaded;
                _logger.LogDebug("→ Cargado lote de {Count}", loaded);
            }
            return totalLoaded;
        }

        private string CleanComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return string.Empty;
            return comment.Trim().Replace("  ", " ").Replace("\n", " ").Replace("\r", "").Replace("\t", " ");
        }
    }
}