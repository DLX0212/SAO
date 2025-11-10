using SAO.Application.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Interfaces
{
    public interface IETLService
    {
        /// <summary>
        /// Ejecuta el proceso ETL completo
        /// </summary>
        Task<ETLResult> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
