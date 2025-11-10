using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Exceptions
{
    public class ExtractionException : ETLException
    {
        public string SourceName { get; }

        public ExtractionException(string sourceName, string message)
            : base($"Error extrayendo de {sourceName}: {message}")
        {
            SourceName = sourceName;
        }

        public ExtractionException(string sourceName, string message, Exception innerException)
            : base($"Error extrayendo de {sourceName}: {message}", innerException)
        {
            SourceName = sourceName;
        }
    }
}
