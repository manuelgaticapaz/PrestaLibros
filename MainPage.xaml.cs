namespace PrestaLibros
{
    public partial class MainPage : ContentPage
    {
        // Variable para saber qué sección está activa actualmente
        private string seccionActual = "Libros";

        public MainPage()
        {
            InitializeComponent();
            try { BaseDeDatos.ObtenerConeccion(); } catch { }
            CargarLibros();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Recargar automáticamente según la pestaña seleccionada
            if (seccionActual == "Libros")
            {
                CargarLibros();
            }
            else if (seccionActual == "Lectores")
            {
                CargarLectores();
            }
            else if (seccionActual == "Prestamos")
            {
                CargarPrestamos();
            }
        }

        // --- CÓDIGO PARA CARGAR LOS LIBROS DE LA BASE DE DATOS ---
        private void CargarLibros()
        {
            try
            {
                CvElementos.ItemTemplate = (DataTemplate)Resources["LibroTemplate"];
                var conexion = BaseDeDatos.ObtenerConeccion();
                var listaLibros = conexion.Table<Libro>().ToList();
                CvElementos.ItemsSource = listaLibros;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"No se pudieron cargar los libros: {ex.Message}", "OK");
            }
        }

        private void CargarLectores()
        {
            try
            {
                CvElementos.ItemTemplate = (DataTemplate)Resources["LectorTemplate"];
                var conexion = BaseDeDatos.ObtenerConeccion();
                var listaLectores = conexion.Table<Lector>().ToList();
                CvElementos.ItemsSource = listaLectores;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"No se pudieron cargar los lectores: {ex.Message}", "OK");
            }
        }

        private void CargarPrestamos()
        {
            try
            {
                CvElementos.ItemTemplate = (DataTemplate)Resources["PrestamoTemplate"];
                var conexion = BaseDeDatos.ObtenerConeccion();
                var listaPrestamos = conexion.Table<Prestamo>().ToList();
                var listaLibros = conexion.Table<Libro>().ToList();
                var listaLectores = conexion.Table<Lector>().ToList();

                // Asociar datos del libro y lector para mostrarlos en la tarjeta
                foreach (var prestamo in listaPrestamos)
                {
                    var libro = listaLibros.FirstOrDefault(l => l.id_libro == prestamo.id_libro);
                    var lector = listaLectores.FirstOrDefault(lec => lec.id_lector == prestamo.id_lector);

                    prestamo.TituloLibro = libro != null ? libro.titulo : $"Libro #{prestamo.id_libro}";
                    prestamo.NombreLector = lector != null ? $"{lector.nombres} {lector.apellidos}".Trim() : $"Lector #{prestamo.id_lector}";
                }

                CvElementos.ItemsSource = listaPrestamos;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"No se pudieron cargar los préstamos: {ex.Message}", "OK");
            }
        }

        private void ActualizarEstiloPestanas(Button activa, Button inactiva1, Button inactiva2)
        {
            activa.BackgroundColor = Color.FromArgb("#3498db");
            activa.TextColor = Colors.White;

            bool esOscuro = Application.Current?.RequestedTheme == AppTheme.Dark;
            Color fondoInactivo = esOscuro ? Color.FromArgb("#2c2c2e") : Color.FromArgb("#e0e0e0");
            Color textoInactivo = esOscuro ? Color.FromArgb("#e4e6eb") : Color.FromArgb("#2c3e50");

            inactiva1.BackgroundColor = fondoInactivo;
            inactiva1.TextColor = textoInactivo;

            inactiva2.BackgroundColor = fondoInactivo;
            inactiva2.TextColor = textoInactivo;
        }

        private void OnFiltroLibrosClicked(object sender, EventArgs e)
        {
            seccionActual = "Libros";
            LblSeccionTitulo.Text = "Gestión de Libros";

            ActualizarEstiloPestanas(BtnLibros, BtnLectores, BtnPrestamos);
            CargarLibros();
        }

        private void OnFiltroLectoresClicked(object sender, EventArgs e)
        {
            seccionActual = "Lectores";
            LblSeccionTitulo.Text = "Gestión de Lectores";

            ActualizarEstiloPestanas(BtnLectores, BtnLibros, BtnPrestamos);
            CargarLectores();
        }

        private void OnFiltroPrestamosClicked(object sender, EventArgs e)
        {
            seccionActual = "Prestamos";
            LblSeccionTitulo.Text = "Gestión de Préstamos";

            ActualizarEstiloPestanas(BtnPrestamos, BtnLibros, BtnLectores);
            CargarPrestamos();
        }

        private async void OnNuevoElementoClicked(object sender, EventArgs e)
        {
            // Dependiendo de la sección activa, el botón "+" abre la página correspondiente
            if (seccionActual == "Libros")
            {
                await Navigation.PushAsync(new LibrosPage());
            }
            else if (seccionActual == "Lectores")
            {
                await Navigation.PushAsync(new LectoresPage());
            }
            else if (seccionActual == "Prestamos")
            {
                await Navigation.PushAsync(new PrestamosPage());
            }
        }

        private async void OnVerElementoClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                if (button.CommandParameter is Libro libroSeleccionado)
                {
                    await Navigation.PushAsync(new LibrosPage(libroSeleccionado, true));
                }
                else if (button.CommandParameter is Prestamo prestamoSeleccionado)
                {
                    await Navigation.PushAsync(new PrestamosPage(prestamoSeleccionado, true));
                }
            }
        }
    }
}