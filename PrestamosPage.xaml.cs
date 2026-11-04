namespace PrestaLibros;

public partial class PrestamosPage : ContentPage
{
    private Prestamo _prestamoActual;
    private bool _esModoVista;
    private List<Libro> _listaLibros = new();
    private List<Lector> _listaLectores = new();

    // 1. Constructor para NUEVO préstamo
    public PrestamosPage()
    {
        InitializeComponent();
        _prestamoActual = new Prestamo
        {
            fecha_prestamo = DateTime.Today.ToString("yyyy-MM-dd"),
            fecha_devolucion_esperada = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
            estado_prestamo = "Prestado"
        };
        _esModoVista = false;

        CargarListas();
        InicializarNuevo();
        ConfigurarInterfaz();
    }

    // 2. Constructor para VER o EDITAR un préstamo existente
    public PrestamosPage(Prestamo prestamo, bool soloLectura)
    {
        InitializeComponent();
        _prestamoActual = prestamo;
        _esModoVista = soloLectura;

        CargarListas();
        CargarDatosEnPantalla();
        ConfigurarInterfaz();
    }

    private void CargarListas()
    {
        try
        {
            var conexion = BaseDeDatos.ObtenerConeccion();
            _listaLibros = conexion.Table<Libro>().ToList();
            _listaLectores = conexion.Table<Lector>().ToList();

            pickerLibro.ItemsSource = _listaLibros;
            pickerLector.ItemsSource = _listaLectores;
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"No se pudieron cargar listas: {ex.Message}", "OK");
        }
    }

    private void InicializarNuevo()
    {
        dpFechaPrestamo.Date = DateTime.Today;
        dpFechaDevolucionEsperada.Date = DateTime.Today.AddDays(7);
        pickerEstado.SelectedItem = "Prestado";
    }

    private void CargarDatosEnPantalla()
    {
        if (_prestamoActual != null)
        {
            // Seleccionar libro
            pickerLibro.SelectedItem = _listaLibros.FirstOrDefault(l => l.id_libro == _prestamoActual.id_libro);

            // Seleccionar lector
            pickerLector.SelectedItem = _listaLectores.FirstOrDefault(lec => lec.id_lector == _prestamoActual.id_lector);

            // Fechas
            if (DateTime.TryParse(_prestamoActual.fecha_prestamo, out var fp))
            {
                dpFechaPrestamo.Date = fp;
            }

            if (DateTime.TryParse(_prestamoActual.fecha_devolucion_esperada, out var fde))
            {
                dpFechaDevolucionEsperada.Date = fde;
            }

            txtFechaRealDevolucion.Text = _prestamoActual.recha_real_devolucion;
            pickerEstado.SelectedItem = string.IsNullOrWhiteSpace(_prestamoActual.estado_prestamo) ? "Prestado" : _prestamoActual.estado_prestamo;
            txtObservaciones.Text = _prestamoActual.observaciones;
        }
    }

    private void ConfigurarInterfaz()
    {
        bool habilitar = !_esModoVista;
        pickerLibro.IsEnabled = habilitar;
        pickerLector.IsEnabled = habilitar;
        dpFechaPrestamo.IsEnabled = habilitar;
        dpFechaDevolucionEsperada.IsEnabled = habilitar;
        txtFechaRealDevolucion.IsReadOnly = !habilitar;
        pickerEstado.IsEnabled = habilitar;
        txtObservaciones.IsReadOnly = !habilitar;

        if (_esModoVista)
        {
            Title = "Detalle del Préstamo";
            LblTitulo.Text = "ℹ️ Detalle del Préstamo";
            BtnGuardar.IsVisible = false;
            BtnEditar.IsVisible = true;
            BtnEliminar.IsVisible = true;
        }
        else
        {
            Title = (_prestamoActual.id_prestamo == 0) ? "Registrar Préstamo" : "Editar Préstamo";
            LblTitulo.Text = (_prestamoActual.id_prestamo == 0) ? "🔄 Registrar Nuevo Préstamo" : "✏️ Editar Préstamo";
            BtnGuardar.IsVisible = true;
            BtnEditar.IsVisible = false;
            BtnEliminar.IsVisible = false;
        }
    }

    private async void guardarPrestamoClicked(object sender, EventArgs e)
    {
        try
        {
            var libroSeleccionado = pickerLibro.SelectedItem as Libro;
            var lectorSeleccionado = pickerLector.SelectedItem as Lector;

            if (libroSeleccionado == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un libro.", "OK");
                return;
            }

            if (lectorSeleccionado == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un lector.", "OK");
                return;
            }

            var conexion = BaseDeDatos.ObtenerConeccion();
            string fechaPrestamo = (dpFechaPrestamo.Date ?? DateTime.Today).ToString("yyyy-MM-dd");
            string fechaDevolucionEsperada = (dpFechaDevolucionEsperada.Date ?? DateTime.Today.AddDays(7)).ToString("yyyy-MM-dd");
            string fechaRealDevolucion = txtFechaRealDevolucion.Text?.Trim() ?? string.Empty;
            string estado = pickerEstado.SelectedItem?.ToString() ?? "Prestado";
            string observaciones = txtObservaciones.Text?.Trim() ?? string.Empty;

            // 1. NUEVO PRÉSTAMO
            if (_prestamoActual.id_prestamo == 0)
            {
                // Verificar disponibilidad de stock del libro
                var libroDb = conexion.Table<Libro>().FirstOrDefault(l => l.id_libro == libroSeleccionado.id_libro);
                if (libroDb == null || libroDb.stock_disponible <= 0)
                {
                    await DisplayAlert("Sin Stock", $"El libro '{libroSeleccionado.titulo}' no tiene stock disponible para préstamo.", "OK");
                    return;
                }

                // Guardar el préstamo
                conexion.Execute(@"INSERT INTO prestamos (
                    id_libro, id_lector, fecha_prestamo, fecha_devolucion_esperada, recha_real_devolucion, estado_prestamo, observaciones
                ) VALUES (?, ?, ?, ?, ?, ?, ?)",
                    libroSeleccionado.id_libro,
                    lectorSeleccionado.id_lector,
                    fechaPrestamo,
                    fechaDevolucionEsperada,
                    fechaRealDevolucion,
                    estado,
                    observaciones);

                // Si se registra como Prestado, descontar 1 del stock disponible
                if (estado == "Prestado")
                {
                    conexion.Execute("UPDATE libros SET stock_disponible = stock_disponible - 1 WHERE id_libro = ?", libroSeleccionado.id_libro);
                }

                await DisplayAlert("Éxito", "Préstamo registrado correctamente", "OK");
            }
            // 2. ACTUALIZAR PRÉSTAMO EXISTENTE
            else
            {
                string estadoAnterior = _prestamoActual.estado_prestamo ?? "Prestado";
                int libroAnteriorId = _prestamoActual.id_libro;

                // Si cambió de Prestado a Devuelto -> restaurar 1 al stock
                if (estadoAnterior == "Prestado" && estado == "Devuelto")
                {
                    conexion.Execute("UPDATE libros SET stock_disponible = stock_disponible + 1 WHERE id_libro = ?", libroAnteriorId);
                    if (string.IsNullOrWhiteSpace(fechaRealDevolucion))
                    {
                        fechaRealDevolucion = DateTime.Today.ToString("yyyy-MM-dd");
                    }
                }
                // Si cambió de Devuelto a Prestado -> verificar stock y restar 1
                else if (estadoAnterior == "Devuelto" && estado == "Prestado")
                {
                    var libroDb = conexion.Table<Libro>().FirstOrDefault(l => l.id_libro == libroSeleccionado.id_libro);
                    if (libroDb == null || libroDb.stock_disponible <= 0)
                    {
                        await DisplayAlert("Sin Stock", $"El libro '{libroSeleccionado.titulo}' no tiene stock disponible.", "OK");
                        return;
                    }
                    conexion.Execute("UPDATE libros SET stock_disponible = stock_disponible - 1 WHERE id_libro = ?", libroSeleccionado.id_libro);
                }
                // Si sigue Prestado pero se cambió de libro
                else if (estado == "Prestado" && libroAnteriorId != libroSeleccionado.id_libro)
                {
                    // Devolver stock al libro anterior
                    conexion.Execute("UPDATE libros SET stock_disponible = stock_disponible + 1 WHERE id_libro = ?", libroAnteriorId);

                    // Descontar stock al nuevo libro
                    var libroNuevoDb = conexion.Table<Libro>().FirstOrDefault(l => l.id_libro == libroSeleccionado.id_libro);
                    if (libroNuevoDb == null || libroNuevoDb.stock_disponible <= 0)
                    {
                        // Revertir libro anterior
                        conexion.Execute("UPDATE libros SET stock_disponible = stock_disponible - 1 WHERE id_libro = ?", libroAnteriorId);
                        await DisplayAlert("Sin Stock", $"El libro '{libroSeleccionado.titulo}' no tiene stock disponible.", "OK");
                        return;
                    }
                    conexion.Execute("UPDATE libros SET stock_disponible = stock_disponible - 1 WHERE id_libro = ?", libroSeleccionado.id_libro);
                }

                conexion.Execute(@"UPDATE prestamos SET 
                    id_libro = ?, id_lector = ?, fecha_prestamo = ?, fecha_devolucion_esperada = ?, recha_real_devolucion = ?, estado_prestamo = ?, observaciones = ? 
                    WHERE id_prestamo = ?",
                    libroSeleccionado.id_libro,
                    lectorSeleccionado.id_lector,
                    fechaPrestamo,
                    fechaDevolucionEsperada,
                    fechaRealDevolucion,
                    estado,
                    observaciones,
                    _prestamoActual.id_prestamo);

                await DisplayAlert("Éxito", "Préstamo actualizado correctamente", "OK");
            }

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
        }
    }

    private void BtnEditar_Clicked(object sender, EventArgs e)
    {
        _esModoVista = false;
        ConfigurarInterfaz();
    }

    private async void BtnEliminar_Clicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Confirmar", "¿Deseas eliminar este registro de préstamo?", "Sí", "No");
        if (confirmar)
        {
            try
            {
                var conexion = BaseDeDatos.ObtenerConeccion();

                // Si el préstamo estaba activo ("Prestado"), restaurar el stock al libro
                if (_prestamoActual.estado_prestamo == "Prestado")
                {
                    conexion.Execute("UPDATE libros SET stock_disponible = stock_disponible + 1 WHERE id_libro = ?", _prestamoActual.id_libro);
                }

                conexion.Execute("DELETE FROM prestamos WHERE id_prestamo = ?", _prestamoActual.id_prestamo);

                await DisplayAlert("Eliminado", "El préstamo ha sido eliminado", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo eliminar: {ex.Message}", "OK");
            }
        }
    }

    private void cancelarPrestamoClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}