using SQLite;

namespace PrestaLibros
{
    [Table("lectores")]
    public class Lector
    {
        [PrimaryKey, AutoIncrement]
        public int id_lector { get; set; }
        public string codigo_carnet { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
        public string foto_perfil { get; set; }
        public int activo { get; set; }

        [Ignore]
        public string NombreCompleto => $"{nombres} {apellidos}".Trim();

        [Ignore]
        public string DescripcionLector => $"{codigo_carnet} - {nombres} {apellidos}".Trim();
    }
}