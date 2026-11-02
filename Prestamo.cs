using SQLite;

namespace PrestaLibros
{
    [Table("prestamos")]
    public class Prestamo
    {
        [PrimaryKey, AutoIncrement]
        public int id_prestamo { get; set; }
        public int id_libro { get; set; }
        public int id_lector { get; set; }
        public string? fecha_prestamo { get; set; }
        public string? fecha_devolucion_esperada { get; set; }
        public string? recha_real_devolucion { get; set; }
        public string? estado_prestamo { get; set; }
        public string? observaciones { get; set; }

        // Propiedad alias para mayor claridad de código
        [Ignore]
        public string? fecha_real_devolucion
        {
            get => recha_real_devolucion;
            set => recha_real_devolucion = value;
        }

        // Propiedades para mostrar información relacionada en la vista
        [Ignore]
        public string? TituloLibro { get; set; }

        [Ignore]
        public string? NombreLector { get; set; }
    }
}
