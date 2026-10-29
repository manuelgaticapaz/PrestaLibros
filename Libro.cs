using SQLite;

namespace PrestaLibros
{
    [Table("libros")]
    public class Libro
    {
        [PrimaryKey, AutoIncrement]
        public int id_libro { get; set; }
        public string isbn { get; set; }
        public string titulo { get; set; }
        public string autor { get; set; }
        public string editorial { get; set; }
        public int anio_publicacion { get; set; }
        public string portada { get; set; }
        public int stock_disponible { get; set; }
        public string libros_prestamo { get; set; }
    }
}