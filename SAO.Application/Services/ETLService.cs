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

        /// <summary>
        /// Ejecuta el proceso ETL completo
        /// </summary>
        public async Task<ETLResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ETLResult();

            _logger.LogInformation("========== INICIANDO PROCESO ETL ==========");

            try
            {
                // Validar configuración
                if (!_config.IsValid(out var configError))
                {
                    throw new ETLException($"Configuración inválida: {configError}");
                }

                // Iniciar transacción
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // FASE 1: EXTRACCIÓN
                _logger.LogInformation("FASE 1: Extrayendo datos de todas las fuentes...");
                var extractedData = await ExtractAllDataAsync(result, cancellationToken);
                result.TotalExtracted = extractedData.Count;

                // FASE 2: TRANSFORMACIÓN (directa a entidades)
                _logger.LogInformation("FASE 2: Transformando y limpiando datos...");
                var opinions = TransformToOpinions(extractedData, result);
                result.TotalTransformed = opinions.Count;

                // FASE 3: CLASIFICACIÓN
                _logger.LogInformation("FASE 3: Clasificando opiniones...");
                ClassifyAllOpinions(opinions);

                // FASE 4: CARGA
                _logger.LogInformation("FASE 4: Cargando datos en la base de datos...");
                var loaded = await LoadOpinionsAsync(opinions, cancellationToken);
                result.TotalLoaded = loaded;

                // Confirmar transacción
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                stopwatch.Stop();
                result.DurationMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "========== ETL COMPLETADO EXITOSAMENTE ==========\n" +
                    "Extraídos: {Extracted}\n" +
                    "Transformados: {Transformed}\n" +
                    "Cargados: {Loaded}\n" +
                    "Errores: {Errors}\n" +
                    "Duración: {Duration}ms",
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

        /// <summary>
        /// Extrae datos de todas las fuentes configuradas
        /// </summary>
        private async Task<List<Opinion>> ExtractAllDataAsync(
            ETLResult result,
            CancellationToken cancellationToken)
        {
            var allOpinions = new List<Opinion>();

            foreach (var extractor in _extractors)
            {
                try
                {
                    _logger.LogInformation("→ Extrayendo de: {Source}", extractor.SourceName);

                    var data = await extractor.ExtractAsync(cancellationToken);
                    var opinions = data.OfType<Opinion>().ToList();
                    var count = opinions.Count;

                    allOpinions.AddRange(opinions);
                    result.ExtractedBySource[extractor.SourceName] = count;

                    _logger.LogInformation(
                        "✓ Extraídos {Count} registros de {Source}",
                        count,
                        extractor.SourceName);
                }
                catch (Exception ex)
                {
                    result.TotalErrors++;
                    result.AddWarning($"Error extrayendo de {extractor.SourceName}: {ex.Message}");
                    _logger.LogError(ex, "Error extrayendo de {Source}", extractor.SourceName);
                }
            }

            return allOpinions;
        }

        /// <summary>
        /// Transforma y limpia las opiniones extraídas
        /// </summary>
        private List<Opinion> TransformToOpinions(
            List<Opinion> extractedData,
            ETLResult result)
        {
            var validOpinions = new List<Opinion>();

            foreach (var opinion in extractedData)
            {
                try
                {
                    // Limpiar comentario
                    opinion.Comentario = CleanComment(opinion.Comentario);

                    // Validar
                    if (!opinion.IsValid())
                    {
                        result.TotalErrors++;
                        result.AddWarning($"Opinión inválida: {opinion.IdOpinion}");
                        continue;
                    }

                    validOpinions.Add(opinion);
                }
                catch (Exception ex)
                {
                    result.TotalErrors++;
                    result.AddWarning($"Error transformando opinión {opinion.IdOpinion}: {ex.Message}");
                    _logger.LogWarning(ex, "Error transformando opinión {Id}", opinion.IdOpinion);
                }
            }

            return validOpinions;
        }

        /// <summary>
        /// Clasifica todas las opiniones
        /// </summary>
        private void ClassifyAllOpinions(List<Opinion> opinions)
        {
            foreach (var opinion in opinions)
            {
                try
                {
                    _classificationService.ProcessOpinion(opinion);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error clasificando opinión {Id}", opinion.IdOpinion);
                }
            }

            _logger.LogInformation("✓ Clasificadas {Count} opiniones", opinions.Count);
        }

        /// <summary>
        /// Carga las opiniones en la base de datos por lotes
        /// </summary>
        private async Task<int> LoadOpinionsAsync(
            List<Opinion> opinions,
            CancellationToken cancellationToken)
        {
            int totalLoaded = 0;
            int batchSize = _config.BatchSize;

            for (int i = 0; i < opinions.Count; i += batchSize)
            {
                var batch = opinions.Skip(i).Take(batchSize).ToList();

                try
                {
                    var loaded = await _unitOfWork.Opiniones.AddRangeAsync(batch, cancellationToken);
                    totalLoaded += loaded;

                    _logger.LogDebug("→ Cargado lote de {Count} opiniones", loaded);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cargando lote de opiniones");
                    throw new ETLException("Error en la fase de carga", ex);
                }
            }

            _logger.LogInformation("✓ Total cargado: {Count} opiniones", totalLoaded);
            return totalLoaded;
        }

        /// <summary>
        /// Limpia un comentario: remueve espacios extras, saltos de línea
        /// </summary>
        private string CleanComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return string.Empty;

            return comment
                .Trim()
                .Replace("  ", " ")
                .Replace("\n", " ")
                .Replace("\r", "")
                .Replace("\t", " ");
        }
    }
}
