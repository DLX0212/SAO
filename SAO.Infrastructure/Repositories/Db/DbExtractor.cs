using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAO.Domain.Entities;
using SAO.Domain.Entities.Configuration;
using SAO.Domain.Repository;
using System.Data;

namespace SAO.Infrastructure.Repositories.Db
{
    public class DbExtractor : IExtractor<object>
    {
        private readonly ILogger<DbExtractor> _logger;
        private readonly IConfiguration _configuration;

        public string SourceName => "Database (Web Reviews)";

        public DbExtractor(ILogger<DbExtractor> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IEnumerable<object>> ExtractAsync(CancellationToken cancellationToken = default)
        {
            var opinions = new List<object>();
            var connectionString = _configuration.GetConnectionString("SourceDatabase");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning("Connection string 'SourceDatabase' no configurada");
                return opinions;
            }

            try
            {
                _logger.LogInformation("Conectando a la base de datos de origen...");

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                var webReviews = await ExtractWebReviewsAsync(connection, cancellationToken);
                opinions.AddRange(webReviews);

                _logger.LogInformation("Extraídas {Count} reseñas de la base de datos", webReviews.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extrayendo datos de la base de datos");
                throw;
            }

            return opinions;
        }

        private async Task<List<Opinion>> ExtractWebReviewsAsync(
            SqlConnection connection,
            CancellationToken cancellationToken)
        {
            var opinions = new List<Opinion>();

            var query = @"
            SELECT 
                IdReview,
                IdCliente,
                IdProducto,
                Fecha,
                Comentario,
                Rating
            FROM WebReviews
            WHERE Fecha >= DATEADD(month, -6, GETDATE())
            ORDER BY Fecha DESC";

            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 120;

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                try
                {
                    var opinion = new Opinion
                    {
                        IdOpinion = reader.GetString(0),
                        IdCliente = ParseClientId(reader.GetString(1)),
                        IdProducto = ParseProductId(reader.GetString(2)),
                        Fuente = "Web",
                        Fecha = reader.GetDateTime(3),
                        Comentario = reader.GetString(4),
                        PuntajeSatisfaccion = CalculateScore(reader.GetInt32(5))
                    };

                    opinions.Add(opinion);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error procesando registro de BD");
                }
            }

            return opinions;
        }

        private int? ParseClientId(string clientId)
        {
            var cleaned = clientId.Replace("C", "").Trim();
            return int.TryParse(cleaned, out var id) ? id : null;
        }

        private int ParseProductId(string productId)
        {
            var cleaned = productId.Replace("P", "").Trim();
            return int.TryParse(cleaned, out var id) ? id : 0;
        }

        private decimal CalculateScore(int rating)
        {
            return (decimal)rating / 5.0m * 100;
        }
    }
}
