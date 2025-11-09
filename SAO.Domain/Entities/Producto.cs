using SAO.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Entities
{
    public class Producto : BaseEntity
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal? Precio { get; set; }

        public Producto()
        {
        }

        public Producto(int id, string nombre, string categoria)
        {
            IdProducto = id;
            Nombre = nombre;
            Categoria = categoria;
        }
    }
}
