namespace PrestaLibros
{
    public partial class LibrosPage : ContentPage
    {
        private Libro _libroActual;
        private bool _esModoVista;

        // Constructor para registrar un libro nuevo
        public LibrosPage()
        {
            InitializeComponent();

            _libroActual = new Libro();
            _esModoVista = false;

            ConfigurarInterfaz();
        }

        // Constructor para ver, editar o eliminar un libro existente
        public LibrosPage(Libro libro, bool soloLectura)
        {
            InitializeComponent();

            _libroActual = libro;
            _esModoVista = soloLectura;

            CargarDatosEnPantalla();
            ConfigurarInterfaz();
        }

        // Cargar información del libro en los controles de texto
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

                ActualizarVistaPortada();
            }
        }

        // Configurar controles según el modo de trabajo (Ver o Editar)
        private void ConfigurarInterfaz()
        {
            bool editable = !_esModoVista;
            bool esEdicionExistente = !_esModoVista && _libroActual.id_libro > 0;

            // Habilitar o bloquear campos de texto
            txtIsbn.IsReadOnly = !editable;
            txtTitulo.IsReadOnly = !editable;
            txtAutor.IsReadOnly = !editable;
            txtEditorial.IsReadOnly = !editable;
            txtAnio.IsReadOnly = !editable;
            txtPortada.IsReadOnly = true;
            txtStock.IsReadOnly = !editable;

            if (_esModoVista)
            {
                Title = "Detalle del Libro";
                BtnGuardar.IsVisible = false;
                BtnEditar.IsVisible = true;
                BtnEliminar.IsVisible = true;
            }
            else
            {
                Title = _libroActual.id_libro == 0 ? "Registrar Libro" : "Editar Libro";
                BtnGuardar.IsVisible = true;
                BtnEditar.IsVisible = false;
                BtnEliminar.IsVisible = false;
            }

            // El botón de fotografía solo aparece si ya está guardado el libro y se está editando
            seleccionarPortadaBoton.IsVisible = esEdicionExistente;
            seleccionarPortadaBoton.IsEnabled = esEdicionExistente;

            ActualizarVistaPortada();
        }

        // Mostrar portada solamente si existe el archivo en el dispositivo
        private void ActualizarVistaPortada()
        {
            string ruta = _libroActual?.portada;
            bool existeFoto = !string.IsNullOrWhiteSpace(ruta) && File.Exists(ruta);

            contenedorPortada.IsVisible = existeFoto;
            portadaImagen.IsVisible = existeFoto;

            portadaImagen.Source = existeFoto ? ImageSource.FromFile(ruta) : null;
        }

        // Guardar o actualizar libro en la base de datos
        public async void guardarLibroClicked(object sender, EventArgs e)
        {
            try
            {
                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(txtTitulo.Text) || string.IsNullOrWhiteSpace(txtIsbn.Text))
                {
                    await DisplayAlert("Error", "El ISBN y el Título son obligatorios.", "OK");
                    return;
                }

                _libroActual.isbn = txtIsbn.Text.Trim();
                _libroActual.titulo = txtTitulo.Text.Trim();
                _libroActual.autor = txtAutor.Text?.Trim();
                _libroActual.editorial = txtEditorial.Text?.Trim();

                int anio = 0;
                int.TryParse(txtAnio.Text, out anio);
                _libroActual.anio_publicacion = anio;

                _libroActual.portada = string.IsNullOrWhiteSpace(txtPortada.Text) ? string.Empty : txtPortada.Text.Trim();

                int stock = 0;
                int.TryParse(txtStock.Text, out stock);
                _libroActual.stock_disponible = stock;

                var conexion = BaseDeDatos.ObtenerConeccion();

                // Si el ID es 0, es un registro nuevo (INSERT)
                if (_libroActual.id_libro == 0)
                {
                    conexion.Execute(
                        "INSERT INTO libros (isbn, titulo, autor, editorial, anio_publicacion, portada, stock_disponible, libros_prestamo) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        _libroActual.isbn,
                        _libroActual.titulo,
                        _libroActual.autor,
                        _libroActual.editorial,
                        _libroActual.anio_publicacion,
                        _libroActual.portada,
                        _libroActual.stock_disponible,
                        _libroActual.stock_disponible
                    );

                    await DisplayAlert("Éxito", "Libro guardado correctamente.", "OK");
                }
                else // Si tiene ID, se actualiza (UPDATE)
                {
                    conexion.Execute(
                        "UPDATE libros SET isbn = ?, titulo = ?, autor = ?, editorial = ?, anio_publicacion = ?, portada = ?, stock_disponible = ? WHERE id_libro = ?",
                        _libroActual.isbn,
                        _libroActual.titulo,
                        _libroActual.autor,
                        _libroActual.editorial,
                        _libroActual.anio_publicacion,
                        _libroActual.portada,
                        _libroActual.stock_disponible,
                        _libroActual.id_libro
                    );

                    await DisplayAlert("Éxito", "Libro actualizado correctamente.", "OK");
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Ocurrió un error al guardar: " + ex.Message, "OK");
            }
        }

        // Cambiar de modo vista a modo edición
        private void BtnEditar_Clicked(object sender, EventArgs e)
        {
            _esModoVista = false;
            ConfigurarInterfaz();
        }

        // Eliminar libro de la base de datos
        private async void BtnEliminar_Clicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Confirmar", "¿Deseas eliminar este libro?", "Sí", "No");

            if (confirmar)
            {
                try
                {
                    var conexion = BaseDeDatos.ObtenerConeccion();
                    conexion.Execute("DELETE FROM libros WHERE id_libro = ?", _libroActual.id_libro);

                    await DisplayAlert("Eliminado", "El libro ha sido eliminado.", "OK");
                    await Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "No se pudo eliminar: " + ex.Message, "OK");
                }
            }
        }

        // Cancelar o regresar a la pantalla anterior
        public async void cancelarLibroClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Seleccionar fotografía de portada desde la galería
        public async void seleccionarPortadaClicked(object sender, EventArgs e)
        {
            try
            {
                if (_esModoVista || _libroActual.id_libro == 0)
                    return;

                FileResult archivoRecibido = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Seleccione imagen de portada"
                });

                if (archivoRecibido == null)
                    return;

                string carpetaPortadas = Path.Combine(FileSystem.AppDataDirectory, "Portadas");
                Directory.CreateDirectory(carpetaPortadas);

                string extension = Path.GetExtension(archivoRecibido.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".jpg";
                }

                string nombreArchivo = Guid.NewGuid().ToString() + extension;
                string rutaDestino = Path.Combine(carpetaPortadas, nombreArchivo);

                using (Stream origen = await archivoRecibido.OpenReadAsync())
                using (FileStream destino = File.Create(rutaDestino))
                {
                    await origen.CopyToAsync(destino);
                }

                _libroActual.portada = rutaDestino;
                txtPortada.Text = rutaDestino;
                ActualizarVistaPortada();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudo seleccionar la fotografía: " + ex.Message, "OK");
            }
        }
    }
}