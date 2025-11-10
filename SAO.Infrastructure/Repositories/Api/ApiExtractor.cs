using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAO.Domain.Entities;
using SAO.Domain.Entities.Configuration;
using SAO.Domain.Repository;
using System.Net.Http.Json;



namespace SAO.Infrastructure.Repositories.Api;
public class ApiExtractor : IExtractor<object>
{
    private readonly ILogger<ApiExtractor> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public string SourceName => "Social Media API";

    public ApiExtractor(
        ILogger<ApiExtractor> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IEnumerable<object>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        var opinions = new List<object>();
        var apiUrl = _configuration["DataSources:API:BaseUrl"];
        var apiKey = _configuration["DataSources:API:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            _logger.LogWarning("API URL no configurada");
            return opinions;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("SocialMediaAPI");
            client.BaseAddress = new Uri(apiUrl);

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }

            var platforms = new[] { "Facebook", "Twitter", "Instagram" };

            foreach (var platform in platforms)
            {
                try
                {
                    _logger.LogInformation("Extrayendo comentarios de {Platform}...", platform);

                    var endpoint = $"/api/comments?platform={platform}&days=30";
                    var response = await client.GetAsync(endpoint, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var comments = await response.Content.ReadFromJsonAsync<List<SocialMediaComment>>(
                            cancellationToken: cancellationToken);

                        if (comments != null)
                        {
                            var platformOpinions = comments.Select(c => new Opinion
                            {
                                IdOpinion = c.Id,
                                IdCliente = ParseClientId(c.CustomerId),
                                IdProducto = ParseProductId(c.ProductId),
                                Fuente = platform,
                                Fecha = c.Date,
                                Comentario = c.Comment
                            });

                            opinions.AddRange(platformOpinions);

                            _logger.LogInformation(
                                "Extraídos {Count} comentarios de {Platform}",
                                comments.Count,
                                platform);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Error en API para {Platform}: {Status}",
                            platform,
                            response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extrayendo de {Platform}", platform);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico extrayendo datos de la API");
            throw;
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

    /// <summary>
    /// Modelo para deserializar la respuesta de la API
    /// </summary>
    private class SocialMediaComment
    {
        public string Id { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
