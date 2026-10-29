namespace PrestaLibros;

public partial class LibrosPage : ContentPage
{
	public LibrosPage()
	{
		InitializeComponent();
	}

    public void guardarLibroClicked(object sender, EventArgs e)
    {
        try
        {
            // 1. Validar que los campos obligatorios no estén vacíos
            if (string.IsNullOrWhiteSpace(txtTitulo.Text) || string.IsNullOrWhiteSpace(txtIsbn.Text))
            {
                DisplayAlert("Error", "El ISBN y el Título son obligatorios.", "OK");
                return;
            }

            // 2. Crear y poblar el objeto Libro con los datos de los Entry
            Libro nuevoLibro = new Libro
            {
                isbn = txtIsbn.Text,
                titulo = txtTitulo.Text,
                autor = txtAutor.Text,
                editorial = txtEditorial.Text,
                // Convertimos de string a int de manera segura, si falla asigna 0
                anio_publicacion = int.TryParse(txtAnio.Text, out int anio) ? anio : 0,
                portada = string.IsNullOrWhiteSpace(txtPortada.Text) ? string.Empty : txtPortada.Text,
                stock_disponible = int.TryParse(txtStock.Text, out int stock) ? stock : 0
            };

            var controladorBD = BaseDeDatos.ObtenerConeccion();
            controladorBD.Execute(@"INSERT INTO libros (
            isbn,
            titulo,
            autor,
            editorial,
            anio_publicacion,
            portada,
            stock_disponible,
            libros_prestamo
            ) VALUES(
            ?, ?, ?, ?, ?, ?, ?, ?
            )"
            , nuevoLibro.isbn, nuevoLibro.titulo, nuevoLibro.autor, nuevoLibro.editorial, nuevoLibro.anio_publicacion, nuevoLibro.portada, nuevoLibro.stock_disponible, nuevoLibro.stock_disponible );

            DisplayAlert("Éxito", "Libro guardado correctamente", "OK");

            // Opcional: Limpiar los campos después de guardar
            cancelarLibroClicked(sender, e);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
        }
    }

    public void cancelarLibroClicked(Object sender, EventArgs e)
    {
        // Limpiar todos los Entry restableciéndolos a vacío
        txtIsbn.Text = string.Empty;
        txtTitulo.Text = string.Empty;
        txtAutor.Text = string.Empty;
        txtEditorial.Text = string.Empty;
        txtAnio.Text = string.Empty;
        txtPortada.Text = string.Empty;
        txtStock.Text = string.Empty;

        // Opcional: Si quieres regresar a la página anterior en la pila de navegación
        Navigation.PopAsync();
    }

}