using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaLibros
{
    public class OpcionMenu
    {
        public string Titulo { get; set; }
        public string Icono { get; set; } // Opcional, por si quieres poner un emoji o texto descriptivo
        public Type PaginaDestino { get; set; } // Guarda la página a la que debe ir
    }
}
