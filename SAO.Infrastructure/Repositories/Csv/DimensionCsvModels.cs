using CsvHelper.Configuration.Attributes;

namespace SAO.Infrastructure.Repositories.Csv
{
    // Mapeo para products.csv
    public class ProductoCsv
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }

        [Name("Categoría")] 
        public string Categoria { get; set; }
    }

    // Mapeo para clients.csv
    public class ClienteCsv
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
    }

    // Mapeo para fuente_datos.csv
    public class FuenteCsv
    {
        public string IdFuente { get; set; }
        public string TipoFuente { get; set; }
        public DateTime FechaCarga { get; set; }
    }
}