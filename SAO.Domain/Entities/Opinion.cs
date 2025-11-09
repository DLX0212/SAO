using SAO.Domain.Base;

namespace SAO.Domain.Entities
{
    public class Opinion : BaseEntity
    {
        public string IdOpinion { get; set; } = string.Empty;
        public int? IdCliente { get; set; }
        public int IdProducto { get; set; }

        public string Fuente { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public string Clasificacion { get; set; } = string.Empty;

        public decimal PuntajeSatisfaccion { get; set; }
        public DateTime Fecha { get; set; }

        public Opinion()
        {
            Fecha = DateTime.UtcNow;
        }

        public Opinion(string id, int idProducto, string comentario, string fuente, decimal puntajeSatisfaccion)
        {
            IdOpinion = id;
            IdProducto = idProducto;
            Comentario = comentario;
            Fuente = fuente;
            PuntajeSatisfaccion = puntajeSatisfaccion;
            Fecha = DateTime.UtcNow;
        }
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Comentario) && IdProducto > 0 && !string.IsNullOrWhiteSpace(Fuente);
        }

    }
}
