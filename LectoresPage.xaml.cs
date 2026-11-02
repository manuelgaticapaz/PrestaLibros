namespace PrestaLibros;

public partial class LectoresPage : ContentPage
{
    public LectoresPage()
    {
        InitializeComponent();
    }

    public void guardarLectorClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtCodigoCarnet.Text) || string.IsNullOrWhiteSpace(txtNombres.Text) || string.IsNullOrWhiteSpace(txtApellidos.Text))
            {
                DisplayAlert("Error", "El Carnet y Nombres son obligatorios.", "OK");
                return;
            }

            Lector nuevoLector = new Lector
            {
                codigo_carnet = txtCodigoCarnet.Text,
                nombres = txtNombres.Text,
                apellidos = txtApellidos.Text,
                telefono = txtTelefono.Text,
                correo = string.IsNullOrWhiteSpace(txtCorreo.Text) ? string.Empty : txtCorreo.Text,
                foto_perfil = string.IsNullOrWhiteSpace(txtFotoPerfil.Text) ? string.Empty : txtFotoPerfil.Text,
                activo = 1
            };

            var controladorBD = BaseDeDatos.ObtenerConeccion();
            controladorBD.Execute(@"INSERT INTO lectores(
            codigo_carnet,
            nombres,
            apellidos,
            telefono,
            correo,
            foto_perfil,
            activo
			) VALUES (
			?,?,?,?,?,?,?)", nuevoLector.codigo_carnet, nuevoLector.nombres, nuevoLector.apellidos, nuevoLector.telefono, nuevoLector.correo, nuevoLector.foto_perfil, nuevoLector.activo);

            DisplayAlert("Éxito", "Libro guardado correctamente", "Ok");

            cancelarLectorClicked(sender, e);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
        }
    }

    public void cancelarLectorClicked(object sender, EventArgs e)
    {
        txtCodigoCarnet.Text = string.Empty;
        txtNombres.Text = string.Empty;
        txtApellidos.Text = string.Empty;
        txtTelefono.Text = string.Empty;
        txtCorreo.Text = string.Empty;
        txtFotoPerfil.Text = string.Empty;

        Navigation.PopAsync();
    }
}