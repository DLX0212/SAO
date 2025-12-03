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

            // ADAPTACIÓN AL DATA WAREHOUSE


            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("DIM_CLIENTE"); 
                entity.HasKey(e => e.IdCliente);
                entity.Property(e => e.IdCliente).ValueGeneratedNever();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Email);
            });

            // Configuración de DIM_PRODUCTO
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("DIM_PRODUCTO"); 
                entity.HasKey(e => e.IdProducto);

                entity.Property(e => e.IdProducto).ValueGeneratedNever();

                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Categoria).HasColumnName("categoria").IsRequired().HasMaxLength(100);

                entity.Property(e => e.Precio).HasColumnType("decimal(10,2)");
            });

            // Configuración de DIM_FUENTE
            modelBuilder.Entity<FuenteDatos>(entity =>
            {
                entity.ToTable("DIM_FUENTE"); 
                entity.HasKey(e => e.IdFuente);
                entity.Property(e => e.IdFuente).HasMaxLength(50);
                entity.Property(e => e.TipoFuente).HasMaxLength(50);
            });

            // Configuración de FACT_OPINION (Tu tabla Opiniones)
            modelBuilder.Entity<Opinion>(entity =>
            {
                entity.ToTable("FACT_OPINION"); 
                entity.HasKey(e => e.IdOpinion);
                entity.HasIndex(e => e.IdProducto);
                entity.HasIndex(e => e.IdCliente);
            });
        }
    }
}

