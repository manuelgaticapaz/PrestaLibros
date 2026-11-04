namespace PrestaLibros
{
    public partial class LectoresPage : ContentPage
    {
        private Lector _lectorActual;
        private bool _esModoVista;

        // Constructor para registrar un lector nuevo
        public LectoresPage()
        {
            InitializeComponent();

            _lectorActual = new Lector();
            _esModoVista = false;

            ConfigurarInterfaz();
        }

        // Constructor para ver, editar o eliminar un lector existente
        public LectoresPage(Lector lector, bool soloLectura)
        {
            InitializeComponent();

            _lectorActual = lector;
            _esModoVista = soloLectura;

            CargarDatosEnPantalla();
            ConfigurarInterfaz();
        }

        // Cargar información del lector en los controles
        private void CargarDatosEnPantalla()
        {
            if (_lectorActual != null)
            {
                txtCodigoCarnet.Text = _lectorActual.codigo_carnet;
                txtNombres.Text = _lectorActual.nombres;
                txtApellidos.Text = _lectorActual.apellidos;
                txtTelefono.Text = _lectorActual.telefono;
                txtCorreo.Text = _lectorActual.correo;
                txtFotoPerfil.Text = _lectorActual.foto_perfil;

                ActualizarVistaFoto();
            }
        }

        // Configurar controles según el modo (Ver o Editar)
        private void ConfigurarInterfaz()
        {
            bool editable = !_esModoVista;
            bool esEdicionExistente = !_esModoVista && _lectorActual.id_lector > 0;

            // Habilitar o bloquear campos de texto
            txtCodigoCarnet.IsReadOnly = !editable;
            txtNombres.IsReadOnly = !editable;
            txtApellidos.IsReadOnly = !editable;
            txtTelefono.IsReadOnly = !editable;
            txtCorreo.IsReadOnly = !editable;
            txtFotoPerfil.IsReadOnly = true;

            if (_esModoVista)
            {
                Title = "Detalle del Lector";
                BtnGuardar.IsVisible = false;
                BtnEditar.IsVisible = true;
                BtnEliminar.IsVisible = true;
            }
            else
            {
                Title = _lectorActual.id_lector == 0 ? "Registrar Lector" : "Editar Lector";
                BtnGuardar.IsVisible = true;
                BtnEditar.IsVisible = false;
                BtnEliminar.IsVisible = false;
            }

            // El botón de foto solo aparece cuando el lector ya está guardado y se está editando
            seleccionarFotoBoton.IsVisible = esEdicionExistente;
            seleccionarFotoBoton.IsEnabled = esEdicionExistente;

            ActualizarVistaFoto();
        }

        // Mostrar foto de perfil solo si existe el archivo
        private void ActualizarVistaFoto()
        {
            string ruta = _lectorActual?.foto_perfil;
            bool existeFoto = !string.IsNullOrWhiteSpace(ruta) && File.Exists(ruta);

            contenedorFoto.IsVisible = existeFoto;
            fotoPerfilImagen.IsVisible = existeFoto;

            fotoPerfilImagen.Source = existeFoto ? ImageSource.FromFile(ruta) : null;
        }

        // Guardar o actualizar lector en la base de datos
        public async void guardarLectorClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCodigoCarnet.Text) || string.IsNullOrWhiteSpace(txtNombres.Text))
                {
                    await DisplayAlert("Error", "El Carnet y los Nombres son obligatorios.", "OK");
                    return;
                }

                _lectorActual.codigo_carnet = txtCodigoCarnet.Text.Trim();
                _lectorActual.nombres = txtNombres.Text.Trim();
                _lectorActual.apellidos = txtApellidos.Text?.Trim();
                _lectorActual.telefono = txtTelefono.Text?.Trim();
                _lectorActual.correo = string.IsNullOrWhiteSpace(txtCorreo.Text) ? string.Empty : txtCorreo.Text.Trim();
                _lectorActual.foto_perfil = string.IsNullOrWhiteSpace(txtFotoPerfil.Text) ? string.Empty : txtFotoPerfil.Text.Trim();
                _lectorActual.activo = 1;

                var conexion = BaseDeDatos.ObtenerConeccion();

                // Si el ID es 0, es un nuevo registro (INSERT)
                if (_lectorActual.id_lector == 0)
                {
                    conexion.Execute(@"
                        INSERT INTO lectores(codigo_carnet, nombres, apellidos, telefono, correo, foto_perfil, activo) 
                        VALUES (?, ?, ?, ?, ?, ?, ?)",
                        _lectorActual.codigo_carnet,
                        _lectorActual.nombres,
                        _lectorActual.apellidos,
                        _lectorActual.telefono,
                        _lectorActual.correo,
                        _lectorActual.foto_perfil,
                        _lectorActual.activo
                    );

                    await DisplayAlert("Éxito", "Lector guardado correctamente.", "OK");
                }
                else // Si tiene ID, se actualiza (UPDATE)
                {
                    conexion.Execute(@"
                        UPDATE lectores SET 
                            codigo_carnet = ?, 
                            nombres = ?, 
                            apellidos = ?, 
                            telefono = ?, 
                            correo = ?, 
                            foto_perfil = ? 
                        WHERE id_lector = ?",
                        _lectorActual.codigo_carnet,
                        _lectorActual.nombres,
                        _lectorActual.apellidos,
                        _lectorActual.telefono,
                        _lectorActual.correo,
                        _lectorActual.foto_perfil,
                        _lectorActual.id_lector
                    );

                    await DisplayAlert("Éxito", "Lector actualizado correctamente.", "OK");
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
            }
        }

        // Cambiar a modo edición
        private void BtnEditar_Clicked(object sender, EventArgs e)
        {
            _esModoVista = false;
            ConfigurarInterfaz();
        }

        // Eliminar lector
        private async void BtnEliminar_Clicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Confirmar", "¿Deseas eliminar este lector?", "Sí", "No");

            if (confirmar)
            {
                try
                {
                    var conexion = BaseDeDatos.ObtenerConeccion();
                    conexion.Execute("DELETE FROM lectores WHERE id_lector = ?", _lectorActual.id_lector);

                    await DisplayAlert("Eliminado", "El lector ha sido eliminado.", "OK");
                    await Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo eliminar: {ex.Message}", "OK");
                }
            }
        }

        // Cancelar / Regresar
        public async void cancelarLectorClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Seleccionar fotografía de perfil desde la galería
        public async void seleccionarFotoClicked(object sender, EventArgs e)
        {
            try
            {
                if (_esModoVista || _lectorActual.id_lector == 0)
                    return;

                FileResult archivoRecibido = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Seleccione foto de perfil"
                });

                if (archivoRecibido == null)
                    return;

                string carpetaFotos = Path.Combine(FileSystem.AppDataDirectory, "Perfiles");
                Directory.CreateDirectory(carpetaFotos);

                string extension = Path.GetExtension(archivoRecibido.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".jpg";
                }

                string nombreArchivo = Guid.NewGuid().ToString() + extension;
                string rutaDestino = Path.Combine(carpetaFotos, nombreArchivo);

                using (Stream origen = await archivoRecibido.OpenReadAsync())
                using (FileStream destino = File.Create(rutaDestino))
                {
                    await origen.CopyToAsync(destino);
                }

                _lectorActual.foto_perfil = rutaDestino;
                txtFotoPerfil.Text = rutaDestino;
                ActualizarVistaFoto();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo seleccionar la fotografía: {ex.Message}", "OK");
            }
        }
    }
}