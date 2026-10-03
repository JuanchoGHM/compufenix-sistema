using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class RegistroAdminWindow : Window
{
    private bool _contrasenaVisible;
    private bool _contrasena2Visible;

    public RegistroAdminWindow()
    {
        InitializeComponent();
    }

    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        var contrasena = ObtenerContrasena();
        var contrasena2 = ObtenerContrasena2();

        if (contrasena != contrasena2)
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
                contrasena,
                RolUsuario.Administrador);

            MessageBox.Show("Administrador creado correctamente.");
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }


    private string ObtenerContrasena() =>
        _contrasenaVisible ? TxtContrasenaVisible.Text : TxtContrasena.Password;

    private string ObtenerContrasena2() =>
        _contrasena2Visible ? TxtContrasena2Visible.Text : TxtContrasena2.Password;

    private void MostrarContrasena_Click(object sender, RoutedEventArgs e)
    {
        _contrasenaVisible = !_contrasenaVisible;

        if (_contrasenaVisible)
        {
            TxtContrasenaVisible.Text = TxtContrasena.Password;
            TxtContrasena.Visibility = Visibility.Collapsed;
            TxtContrasenaVisible.Visibility = Visibility.Visible;
        }
        else
        {
            TxtContrasena.Password = TxtContrasenaVisible.Text;
            TxtContrasenaVisible.Visibility = Visibility.Collapsed;
            TxtContrasena.Visibility = Visibility.Visible;
        }
    }

    private void MostrarContrasena2_Click(object sender, RoutedEventArgs e)
    {
        _contrasena2Visible = !_contrasena2Visible;

        if (_contrasena2Visible)
        {
            TxtContrasena2Visible.Text = TxtContrasena2.Password;
            TxtContrasena2.Visibility = Visibility.Collapsed;
            TxtContrasena2Visible.Visibility = Visibility.Visible;
        }
        else
        {
            TxtContrasena2.Password = TxtContrasena2Visible.Text;
            TxtContrasena2Visible.Visibility = Visibility.Collapsed;
            TxtContrasena2.Visibility = Visibility.Visible;
        }
    }
}