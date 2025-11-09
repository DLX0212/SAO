using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Entities.Configuration
{
    public sealed class ApiConfiguration
    {
 
        /// URL base de la API

        public string BaseUrl { get; init; } = string.Empty;

        /// API Key para autenticación

        public string ApiKey { get; init; } = string.Empty;


        /// Timeout de la petición en segundos

        public int TimeoutSeconds { get; init; }

 
        /// Máximo de reintentos en caso de fallo

        public int MaxRetries { get; init; }


        /// Headers adicionales para las peticiones
 
        public Dictionary<string, string> Headers { get; init; } = new();


        /// Endpoints configurados

        public Dictionary<string, string> Endpoints { get; init; } = new();

        private ApiConfiguration() { }

        public static ApiConfiguration Create(
            string baseUrl,
            string apiKey = "",
            int timeoutSeconds = 60)
        {
            return new ApiConfiguration
            {
                BaseUrl = baseUrl.TrimEnd('/'),
                ApiKey = apiKey,
                TimeoutSeconds = timeoutSeconds,
                MaxRetries = 3,
                Headers = new Dictionary<string, string>(),
                Endpoints = new Dictionary<string, string>()
            };
        }

        /// Obtiene la URL completa de un endpoint

        public string GetEndpointUrl(string endpointKey)
        {
            if (Endpoints.TryGetValue(endpointKey, out var endpoint))
            {
                return $"{BaseUrl}/{endpoint.TrimStart('/')}";
            }

            throw new KeyNotFoundException($"Endpoint '{endpointKey}' no encontrado en la configuración");
        }

        /// Valida que la configuración sea correcta
        
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                errorMessage = "BaseUrl no puede estar vacío";
                return false;
            }

            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            {
                errorMessage = "BaseUrl no es una URL válida";
                return false;
            }

            if (TimeoutSeconds <= 0)
            {
                errorMessage = "TimeoutSeconds debe ser mayor a 0";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
