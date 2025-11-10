using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAO.Domain.Entities;
using SAO.Domain.Entities.Configuration;
using SAO.Domain.Repository;
using System.Formats.Asn1;
using System.Globalization;

namespace SAO.Infrastructure.Repositories.Csv
{
    public class CsvExtractor : IExtractor<object>
    {
        private readonly ILogger<CsvExtractor> _logger;
        private readonly IConfiguration _configuration;

        public string SourceName => "CSV Files";

        public CsvExtractor(ILogger<CsvExtractor> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IEnumerable<object>> ExtractAsync(CancellationToken cancellationToken = default)
        {
            var opinions = new List<object>();
            var basePath = _configuration["DataSources:CSV:BasePath"] ?? "./Data";

            try
            {
                // Leer archivo surveys_part1.csv
                var surveysPath = Path.Combine(basePath, "surveys_part1.csv");

                if (File.Exists(surveysPath))
                {
                    _logger.LogInformation("Leyendo archivo: {Path}", surveysPath);
                    var surveyOpinions = await ReadSurveysAsync(surveysPath, cancellationToken);
                    opinions.AddRange(surveyOpinions);

                    _logger.LogInformation("Leídas {Count} opiniones de surveys", surveyOpinions.Count);
                }
                else
                {
                    _logger.LogWarning("Archivo no encontrado: {Path}", surveysPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extrayendo datos de CSV");
                throw;
            }

            return opinions;
        }

        /// <summary>
        /// Lee el archivo de surveys y lo convierte a opiniones
        /// </summary>
        private async Task<List<Opinion>> ReadSurveysAsync(string filePath, CancellationToken cancellationToken)
        {
            var opinions = new List<Opinion>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                Delimiter = ","
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            await foreach (var record in csv.GetRecordsAsync<SurveyCsvRow>(cancellationToken))
            {
                try
                {
                    var opinion = new Opinion
                    {
                        IdOpinion = record.IdComment,
                        IdCliente = ParseClientId(record.IdCliente),
                        IdProducto = ParseProductId(record.IdProducto),
                        Fuente = record.Fuente,
                        Fecha = ParseDate(record.Fecha),
                        Comentario = record.Comentario ?? string.Empty
                    };

                    opinions.Add(opinion);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error procesando registro CSV: {Id}", record.IdComment);
                }
            }

            return opinions;
        }

        private int? ParseClientId(string? clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return null;

            var cleaned = clientId.Replace("C", "").Trim();
            return int.TryParse(cleaned, out var id) ? id : null;
        }

        private int ParseProductId(string? productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return 0;

            var cleaned = productId.Replace("P", "").Trim();
            return int.TryParse(cleaned, out var id) ? id : 0;
        }

        private DateTime ParseDate(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return DateTime.UtcNow;

            return DateTime.TryParse(dateStr, out var date) ? date : DateTime.UtcNow;
        }

        /// <summary>
        /// Clase para mapear las columnas del CSV
        /// </summary>
        private class SurveyCsvRow
        {
            public string IdComment { get; set; } = string.Empty;
            public string? IdCliente { get; set; }
            public string? IdProducto { get; set; }
            public string Fuente { get; set; } = string.Empty;
            public string? Fecha { get; set; }
            public string? Comentario { get; set; }
        }
    }
}
