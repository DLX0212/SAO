using SAO.Domain.Base;

namespace SAO.Domain.Entities
{

    public class FuenteDatos : BaseEntity
    {
        public string IdFuente { get; set; } = string.Empty;
        public string TipoFuente { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; }
        public int RegistrosProcesados { get; set; }

        public FuenteDatos()
        {
            IdFuente = Guid.NewGuid().ToString();
            FechaCarga = DateTime.UtcNow;
        }

        public FuenteDatos(string id, string tipoFuente) : this()
        {
            IdFuente = id;
            TipoFuente = tipoFuente;
        }
    }
}
