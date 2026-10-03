using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class RegistroAdminWindow : Window
{
    public RegistroAdminWindow()
    {
        InitializeComponent();
    }

    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        if (TxtContrasena.Password != TxtContrasena2.Password)
        {
            MessageBox.Show("Las contraseñas no coinciden.");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioUsuarios(db);

            servicio.CrearUsuario(
                TxtNombre.Text,
                TxtCorreo.Text,
                TxtContrasena.Password,
                RolUsuario.Administrador);

            MessageBox.Show("Administrador creado correctamente.");
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}