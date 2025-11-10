using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Result
{
    public class ETLResult
    {
        public bool Success { get; set; }
        public int TotalExtracted { get; set; }
        public int TotalTransformed { get; set; }
        public int TotalLoaded { get; set; }
        public int TotalErrors { get; set; }
        public long DurationMs { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, int> ExtractedBySource { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public ETLResult()
        {
            Success = true;
        }

        /// <summary>
        /// Agrega una advertencia al resultado
        /// </summary>
        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        /// <summary>
        /// Marca el resultado como fallido
        /// </summary>
        public void MarkAsFailed(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
}
