using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Exceptions
{
    public class ValidationException : ETLException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(string message, Dictionary<string, string[]> errors)
            : base(message)
        {
            Errors = errors;
        }
    }
}
