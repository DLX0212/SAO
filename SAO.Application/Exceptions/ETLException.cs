using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Exceptions
{
    public class ETLException : Exception
    {
        public ETLException(string message) : base(message)
        {
        }

        public ETLException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
