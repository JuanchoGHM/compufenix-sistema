using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class UsuarioWindow : Window
{
    public UsuarioWindow()
    {
        InitializeComponent();
        CmbRol.ItemsSource = Enum.GetValues<RolUsuario>();
        CmbRol.SelectedItem = RolUsuario.Tecnico;
    }

    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (CmbRol.SelectedItem is not RolUsuario rol)
        {
            MostrarError("Debes seleccionar un rol.");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioUsuarios(db).CrearUsuario(
                TxtNombre.Text, TxtCorreo.Text, TxtContrasena.Password, rol);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            MostrarError(ex.Message);
        }
    }

    private void MostrarError(string mensaje)
    {
        TxtError.Text = mensaje;
        PanelError.Visibility = Visibility.Visible;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}