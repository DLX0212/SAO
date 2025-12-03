using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAO.Domain.Entities;
using SAO.Infrastructure.Repositories.Csv;
using SAO.Infrastructure.Repositories.Db;
using System.Globalization;

namespace SAO.Infrastructure.Services
{
    public class DimensionLoaderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DimensionLoaderService> _logger;

        public DimensionLoaderService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<DimensionLoaderService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CargarDimensionesAsync()
        {
            var basePath = _configuration["DataSources:CSV:BasePath"] ?? "./Data";
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null,
                BadDataFound = null
            };

            // Orden de carga 
            await CargarProductos(basePath, config);
            await CargarClientes(basePath, config);
            await CargarFuentes(basePath, config);
        }

        private async Task CargarProductos(string basePath, CsvConfiguration config)
        {
            if (_context.Productos.Any()) return; 

            var path = Path.Combine(basePath, "products.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning($"Archivo no encontrado: {path}");
                return;
            }

            _logger.LogInformation("Cargando DIM_PRODUCTO...");
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<ProductoCsv>();
            var entidades = records.Select(r => new Producto
            {
                IdProducto = r.IdProducto, 
                Nombre = r.Nombre,
                Categoria = r.Categoria,
                Precio = 0 
            }).ToList();

            await _context.Productos.AddRangeAsync(entidades);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"✓ {entidades.Count} productos cargados.");
        }

        private async Task CargarClientes(string basePath, CsvConfiguration config)
        {
            if (_context.Clientes.Any()) return;

            var path = Path.Combine(basePath, "clients.csv");
            if (!File.Exists(path)) return;

            _logger.LogInformation("Cargando DIM_CLIENTE...");
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<ClienteCsv>();
            var entidades = records.Select(r => new Cliente
            {
                IdCliente = r.IdCliente,
                Nombre = r.Nombre,
                Email = r.Email,
                Telefono = "No Registrado"
            }).ToList();

            await _context.Clientes.AddRangeAsync(entidades);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"✓ {entidades.Count} clientes cargados.");
        }

        private async Task CargarFuentes(string basePath, CsvConfiguration config)
        {
            if (_context.FuentesDatos.Any()) return;

            var path = Path.Combine(basePath, "fuente_datos.csv");
            if (!File.Exists(path)) return;

            _logger.LogInformation("Cargando DIM_FUENTE...");
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<FuenteCsv>();
            var entidades = records.Select(r => new FuenteDatos
            {
                IdFuente = r.IdFuente,
                TipoFuente = r.TipoFuente,
                FechaCarga = r.FechaCarga
            }).ToList();

            await _context.FuentesDatos.AddRangeAsync(entidades);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"✓ {entidades.Count} fuentes cargadas.");
        }
    }
}
