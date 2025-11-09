using SAO.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Cliente> Clientes { get; }
        IRepository<Producto> Productos { get; }
        IRepository<Opinion> Opiniones { get; }
        IRepository<FuenteDatos> FuenteDatos { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
