using System.Windows;
using Compufenix.Business;

namespace Compufenix.UI;

public partial class LoginWindow : Window
{
    private bool _contrasenaVisible;

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void Entrar_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;

        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioUsuarios(db);
            var usuario = servicio.IniciarSesion(TxtCorreo.Text, ObtenerContrasena());
            if (usuario == null)
            {
                TxtError.Text = "Correo o contraseña incorrectos.";
                TxtError.Visibility = Visibility.Visible;
                TxtContrasena.Clear();
                TxtContrasena.Focus();
                return;
            }

            // Anota quién entró y cierra el login con éxito
            Sesion.UsuarioActual = usuario;
            DialogResult = true;
        }
        catch (InvalidOperationException ex)
        {
            // Mensajes de bloqueo por intentos fallidos
            TxtError.Text = ex.Message;
            TxtError.Visibility = Visibility.Visible;
            TxtContrasena.Clear();
        }
        catch (Exception ex)
        {
            TxtError.Text = "No se pudo iniciar sesión: " + ex.Message;
            TxtError.Visibility = Visibility.Visible;
        }
    }

    private string ObtenerContrasena() =>
        _contrasenaVisible ? TxtContrasenaVisible.Text : TxtContrasena.Password;

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


    private void OlvideContrasena_Click(object sender, RoutedEventArgs e)
    {
        var ventana = new RecuperarContrasenaWindow { Owner = this };
        ventana.ShowDialog();
    }
}