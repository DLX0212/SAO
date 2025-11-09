using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Repository
{
    public interface IExtractor<T> where T : class
    {
        Task<IEnumerable<T>> ExtractAsync(CancellationToken cancellationToken = default);
        string SourceName { get; }
    }
}
