using Microsoft.EntityFrameworkCore;
using SAO.Domain.Entities;


namespace SAO.Infrastructure.Repositories.Db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Opinion> Opiniones { get; set; }
        public DbSet<FuenteDatos> FuentesDatos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.IdCliente);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Telefono)
                    .HasMaxLength(20);

                entity.HasIndex(e => e.Email);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(e => e.IdProducto);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Categoria)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Precio)
                    .HasColumnType("decimal(10,2)");

                entity.HasIndex(e => e.Categoria);
            });

            // Configuración de Opinion
            modelBuilder.Entity<Opinion>(entity =>
            {
                entity.ToTable("Opiniones");
                entity.HasKey(e => e.IdOpinion);

                entity.Property(e => e.IdOpinion)
                    .HasMaxLength(50);

                entity.Property(e => e.Fuente)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Comentario)
                    .IsRequired();

                entity.Property(e => e.Clasificacion)
                    .HasMaxLength(20);

                entity.Property(e => e.PuntajeSatisfaccion)
                    .HasColumnType("decimal(5,2)");

                entity.HasIndex(e => e.IdProducto);
                entity.HasIndex(e => e.IdCliente);
                entity.HasIndex(e => e.Fecha);
                entity.HasIndex(e => e.Clasificacion);
            });

            // Configuración de FuenteDatos
            modelBuilder.Entity<FuenteDatos>(entity =>
            {
                entity.ToTable("FuentesDatos");
                entity.HasKey(e => e.IdFuente);

                entity.Property(e => e.IdFuente)
                    .HasMaxLength(50);

                entity.Property(e => e.TipoFuente)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.TipoFuente);
                entity.HasIndex(e => e.FechaCarga);
            });
        }
    }
}

