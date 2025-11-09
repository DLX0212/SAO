using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Repository
{
    public interface IEntity<TId>
    {
        TId Id { get; set; }
        DateTime CreatedAt { get; set; }
        bool IsActive { get; set; }
    }
}
