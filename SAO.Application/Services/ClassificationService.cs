using SAO.Domain.Entities;
using SAO.Domain.Entities.Configuration;
using Microsoft.Extensions.Logging;
using SAO.Application.Interfaces;
using System;


namespace SAO.Application.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly ILogger<ClassificationService> _logger;
        private readonly ClassificationConfiguration _config;

        public ClassificationService(
            ILogger<ClassificationService> logger,
            ClassificationConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Procesa una opinión: clasifica y calcula puntaje
        /// </summary>
        public void ProcessOpinion(Opinion opinion)
        {
            if (opinion == null || !opinion.IsValid())
            {
                _logger.LogWarning("Opinión inválida, no se puede procesar");
                return;
            }

            // Clasificar la opinión
            opinion.Clasificacion = ClassifyOpinion(opinion.Comentario);

            // Calcular puntaje de satisfacción
            opinion.PuntajeSatisfaccion = CalculateSatisfactionScore(opinion.Clasificacion);

            _logger.LogDebug(
                "Opinión {Id} clasificada como {Clasificacion} con puntaje {Puntaje}",
                opinion.IdOpinion,
                opinion.Clasificacion,
                opinion.PuntajeSatisfaccion);
        }

        /// <summary>
        /// Clasifica un comentario basado en palabras clave
        /// </summary>
        private string ClassifyOpinion(string comentario)
        {
            if (string.IsNullOrWhiteSpace(comentario))
                return "Neutra";

            var comentarioLower = comentario.ToLower();
            var palabras = comentarioLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int puntajePositivo = 0;
            int puntajeNegativo = 0;

            // Contar palabras positivas y negativas
            foreach (var palabra in palabras)
            {
                if (_config.CustomPositiveWords.Contains(palabra))
                    puntajePositivo++;

                if (_config.CustomNegativeWords.Contains(palabra))
                    puntajeNegativo++;
            }

            // Determinar clasificación
            if (puntajePositivo > puntajeNegativo)
                return "Positiva";

            if (puntajeNegativo > puntajePositivo)
                return "Negativa";

            return "Neutra";
        }

        /// <summary>
        /// Calcula el puntaje de satisfacción (0-100)
        /// </summary>
        private decimal CalculateSatisfactionScore(string clasificacion)
        {
            return clasificacion switch
            {
                "Positiva" => 80m,
                "Negativa" => 20m,
                "Neutra" => 50m,
                _ => 50m
            };
        }
    }
}
