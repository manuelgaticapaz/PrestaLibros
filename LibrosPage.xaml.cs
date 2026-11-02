namespace PrestaLibros;

public partial class LibrosPage : ContentPage
{
    private Libro _libroActual;
    private bool _esModoVista;

    // 1. Constructor para NUEVO libro
    public LibrosPage()
    {
        InitializeComponent();
        _libroActual = new Libro();
        _esModoVista = false;
        ConfigurarInterfaz();
    }

    // 2. Constructor para VER, EDITAR o ELIMINAR (recibe el objeto seleccionado)
    public LibrosPage(Libro libro, bool soloLectura)
    {
        InitializeComponent();
        _libroActual = libro;
        _esModoVista = soloLectura;

        CargarDatosEnPantalla();
        ConfigurarInterfaz();
    }

    private void CargarDatosEnPantalla()
    {
        if (_libroActual != null)
        {
            txtIsbn.Text = _libroActual.isbn;
            txtTitulo.Text = _libroActual.titulo;
            txtAutor.Text = _libroActual.autor;
            txtEditorial.Text = _libroActual.editorial;
            txtAnio.Text = _libroActual.anio_publicacion.ToString();
            txtPortada.Text = _libroActual.portada;
            txtStock.Text = _libroActual.stock_disponible.ToString();
        }
    }

    private void ConfigurarInterfaz()
    {
        // Si estamos en modo vista, los campos son de solo lectura
        bool habilitar = !_esModoVista;
        txtIsbn.IsReadOnly = !habilitar;
        txtTitulo.IsReadOnly = !habilitar;
        txtAutor.IsReadOnly = !habilitar;
        txtEditorial.IsReadOnly = !habilitar;
        txtAnio.IsReadOnly = !habilitar;
        txtPortada.IsReadOnly = !habilitar;
        txtStock.IsReadOnly = !habilitar;

        // Mostrar u ocultar botones según el modo
        if (_esModoVista)
        {
            Title = "Detalle del Libro";
            BtnGuardar.IsVisible = false;
            BtnEditar.IsVisible = true;
            BtnEliminar.IsVisible = true;
        }
        else
        {
            // Si el id es 0 o nulo es nuevo, de lo contrario es edición
            Title = (_libroActual.id_libro == 0) ? "Registrar Libro" : "Editar Libro";
            BtnGuardar.IsVisible = true;
            BtnEditar.IsVisible = false;
            BtnEliminar.IsVisible = false;
        }
    }

    public void guardarLibroClicked(object sender, EventArgs e)
    {
        try
        {
            // 1. Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtTitulo.Text) || string.IsNullOrWhiteSpace(txtIsbn.Text))
            {
                DisplayAlert("Error", "El ISBN y el Título son obligatorios.", "OK");
                return;
            }

            // 2. Poblar el objeto con los datos de los Entry
            _libroActual.isbn = txtIsbn.Text;
            _libroActual.titulo = txtTitulo.Text;
            _libroActual.autor = txtAutor.Text;
            _libroActual.editorial = txtEditorial.Text;
            _libroActual.anio_publicacion = int.TryParse(txtAnio.Text, out int anio) ? anio : 0;
            _libroActual.portada = string.IsNullOrWhiteSpace(txtPortada.Text) ? string.Empty : txtPortada.Text;
            _libroActual.stock_disponible = int.TryParse(txtStock.Text, out int stock) ? stock : 0;

            var controladorBD = BaseDeDatos.ObtenerConeccion();

            // 3. Determinar si es un INSERT (nuevo) o UPDATE (editar)
            if (_libroActual.id_libro == 0)
            {
                controladorBD.Execute(@"INSERT INTO libros (
                    isbn, titulo, autor, editorial, anio_publicacion, portada, stock_disponible, libros_prestamo
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                _libroActual.isbn, _libroActual.titulo, _libroActual.autor,
                _libroActual.editorial, _libroActual.anio_publicacion,
                _libroActual.portada, _libroActual.stock_disponible, _libroActual.stock_disponible);

                DisplayAlert("Éxito", "Libro guardado correctamente", "OK");
            }
            else
            {
                controladorBD.Execute(@"UPDATE libros SET 
                    isbn = ?, titulo = ?, autor = ?, editorial = ?, anio_publicacion = ?, portada = ?, stock_disponible = ? 
                    WHERE id_libro = ?",
                _libroActual.isbn, _libroActual.titulo, _libroActual.autor,
                _libroActual.editorial, _libroActual.anio_publicacion,
                _libroActual.portada, _libroActual.stock_disponible, _libroActual.id_libro);

                DisplayAlert("Éxito", "Libro actualizado correctamente", "OK");
            }

            Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
        }
    }

    private void BtnEditar_Clicked(object sender, EventArgs e)
    {
        // Cambiar el estado a modo edición
        _esModoVista = false;
        ConfigurarInterfaz();
    }

    private async void BtnEliminar_Clicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Confirmar", "¿Deseas eliminar este libro?", "Sí", "No");
        if (confirmar)
        {
            try
            {
                var controladorBD = BaseDeDatos.ObtenerConeccion();
                controladorBD.Execute("DELETE FROM libros WHERE id_libro = ?", _libroActual.id_libro);

                await DisplayAlert("Eliminado", "El libro ha sido eliminado", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo eliminar: {ex.Message}", "OK");
            }
        }
    }

    public void cancelarLibroClicked(Object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

}