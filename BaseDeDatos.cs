using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaLibros
{
    public class BaseDeDatos
    {
        static SQLiteConnection Coneccion;

        public static SQLiteConnection ObtenerConeccion()
        {
            string ruta = Path.Combine(FileSystem.AppDataDirectory, "intecap.db");
            Coneccion = new SQLiteConnection(ruta);

            Coneccion.Execute(@"CREATE TABLE IF NOT EXISTS lectores(
            id_lector INTEGER PRIMARY KEY AUTOINCREMENT, 
            codigo_carnet TEXT,
            nombres TEXT,
            apellidos TEXT,
            telefono TEXT,
            correo TEXT,
            foto_perfil TEXT,
            activo INTEGER
            );");

            Coneccion.Execute(@"CREATE TABLE IF NOT EXISTS libros(
            id_libro INTEGER PRIMARY KEY AUTOINCREMENT, 
            isbn TEXT,
            titulo TEXT,
            autor TEXT,
            editorial TEXT,
            anio_publicacion INTEGER,
            portada TEXT,
            stock_disponible INTEGER,
            libros_prestamo TEXT); ");

            Coneccion.Execute(@"CREATE TABLE IF NOT EXISTS prestamos(
            id_prestamo INTEGER PRIMARY KEY AUTOINCREMENT, 
            id_libro INTEGER,
            id_lector INTEGER,
            fecha_prestamo TEXT,
            fecha_devolucion_esperada TEXT,
            recha_real_devolucion TEXT,
            estado_prestamo TEXT,
            observaciones TEXT);
            ");

            return Coneccion;
        }
    }
}
