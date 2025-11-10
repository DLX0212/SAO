using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Exceptions
{
    public class TransformationException : ETLException
    {
        public string RecordId { get; }

        public TransformationException(string recordId, string message)
            : base($"Error transformando registro {recordId}: {message}")
        {
            RecordId = recordId;
        }

        public TransformationException(string recordId, string message, Exception innerException)
            : base($"Error transformando registro {recordId}: {message}", innerException)
        {
            RecordId = recordId;
        }
    }
}
