namespace APP.Models
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        // Relación uno a muchos con Consolas
        public ICollection<Consola> Consolas { get; set; } = new List<Consola>();
    }

    public class Consola
    {
        public int IdConsola { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Almacenamiento { get; set; }
        public string Generacion { get; set; }    
        public bool IncluyeJuegos { get; set; }     
        public string Imagen { get; set; }
        public decimal Precio { get; set; }

        // Clave foránea
        public int ProveedoresIdProveedor { get; set; }

        // Propiedad de navegación
        public Proveedor Proveedor { get; set; }
    }
}
