namespace PrestaLibros
{
    public partial class MainPage : ContentPage
    {
        // Variable para saber qué sección está activa actualmente
        private string seccionActual = "Libros";

        public MainPage()
        {
            InitializeComponent();
            try { BaseDeDatos.ObtenerConeccion(); } catch { };
            CargarLibros();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Esto recarga la lista automáticamente cada vez que regreses de la pantalla "LibrosPage"
            if (seccionActual == "Libros")
            {
                CargarLibros();
            }
        }

        // --- CÓDIGO PARA CARGAR LOS LIBROS DE LA BASE DE DATOS ---
        private void CargarLibros()
        {
            try
            {
                var conexion = BaseDeDatos.ObtenerConeccion();
                // Realiza la consulta SELECT a tu tabla "libros"
                var listaLibros = conexion.Table<Libro>().ToList();

                // Asigna los datos al CollectionView de la interfaz
                CvElementos.ItemsSource = listaLibros;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"No se pudieron cargar los libros: {ex.Message}", "OK");
            }
        }

        private void OnFiltroLibrosClicked(object sender, EventArgs e)
        {
            seccionActual = "Libros";
            LblSeccionTitulo.Text = "Gestión de Libros";

            // Cambiar estilos visuales de las pestañas
            BtnLibros.BackgroundColor = Color.FromArgb("#3498db"); BtnLibros.TextColor = Colors.White;
            BtnLectores.BackgroundColor = Color.FromArgb("#e0e0e0"); BtnLectores.TextColor = Color.FromArgb("#2c3e50");
            BtnPrestamos.BackgroundColor = Color.FromArgb("#e0e0e0"); BtnPrestamos.TextColor = Color.FromArgb("#2c3e50");

            CargarLibros(); // Llamamos a la función que llena los datos de libros
        }

        private void OnFiltroLectoresClicked(object sender, EventArgs e)
        {
            seccionActual = "Lectores";
            LblSeccionTitulo.Text = "Gestión de Lectores";

            BtnLectores.BackgroundColor = Color.FromArgb("#3498db"); BtnLectores.TextColor = Colors.White;
            BtnLibros.BackgroundColor = Color.FromArgb("#e0e0e0"); BtnLibros.TextColor = Color.FromArgb("#2c3e50");
            BtnPrestamos.BackgroundColor = Color.FromArgb("#e0e0e0"); BtnPrestamos.TextColor = Color.FromArgb("#2c3e50");

            CvElementos.ItemsSource = null; // Limpiar o poner la lista de lectores después
        }

        private void OnFiltroPrestamosClicked(object sender, EventArgs e)
        {
            seccionActual = "Prestamos";
            LblSeccionTitulo.Text = "Gestión de Préstamos";

            BtnPrestamos.BackgroundColor = Color.FromArgb("#3498db"); BtnPrestamos.TextColor = Colors.White;
            BtnLibros.BackgroundColor = Color.FromArgb("#e0e0e0"); BtnLibros.TextColor = Color.FromArgb("#2c3e50");
            BtnLectores.BackgroundColor = Color.FromArgb("#e0e0e0"); BtnLectores.TextColor = Color.FromArgb("#2c3e50");

            CvElementos.ItemsSource = null; // Limpiar o poner la lista de préstamos después
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
                // await Navigation.PushAsync(new PrestamosPage());
                await DisplayAlert("Préstamos", "Pronto crearás esta página.", "OK");
            }
        }
    }
}