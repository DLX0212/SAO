using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SAO.Domain.Entities;
using SAO.Domain.Repository;


namespace SAO.Infrastructure.Repositories.Db
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerFactory _loggerFactory;
        private IDbContextTransaction? _transaction;

        private IRepository<Cliente>? _clientes;
        private IRepository<Producto>? _productos;
        private IRepository<Opinion>? _opiniones;
        private IRepository<FuenteDatos>? _fuenteDatos;

        public UnitOfWork(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _loggerFactory = loggerFactory;
        }

        public IRepository<Cliente> Clientes =>
            _clientes ??= new Repository<Cliente>(_context, _loggerFactory.CreateLogger<Repository<Cliente>>());

        public IRepository<Producto> Productos =>
            _productos ??= new Repository<Producto>(_context, _loggerFactory.CreateLogger<Repository<Producto>>());

        public IRepository<Opinion> Opiniones =>
            _opiniones ??= new Repository<Opinion>(_context, _loggerFactory.CreateLogger<Repository<Opinion>>());

        public IRepository<FuenteDatos> FuenteDatos =>
            _fuenteDatos ??= new Repository<FuenteDatos>(_context, _loggerFactory.CreateLogger<Repository<FuenteDatos>>());

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No hay transacción activa");

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                return;

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }

}
