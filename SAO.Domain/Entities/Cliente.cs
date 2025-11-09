using SAO.Domain.Base;

namespace SAO.Domain.Entities
{
    public class Cliente : BaseEntity
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaRegistro { get; set; }


        public Cliente()
        {
            FechaRegistro = DateTime.UtcNow;
        }

        public Cliente(int idCliente, string nombre, string email, string telefono)
        {
            IdCliente = idCliente;
            Nombre = nombre;
            Email = email;
            Telefono = telefono;
            FechaRegistro = DateTime.UtcNow;
        }
        public bool IsEmailValid()
        {
            return !string.IsNullOrWhiteSpace(Email) && Email.Contains("@") && Email.Contains(".");
        }
    }
}
