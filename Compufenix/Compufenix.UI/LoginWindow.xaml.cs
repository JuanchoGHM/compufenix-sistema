using System.Windows;
using Compufenix.Business;

namespace Compufenix.UI;

public partial class LoginWindow : Window
{
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
            var usuario = servicio.IniciarSesion(TxtCorreo.Text, TxtContrasena.Password);

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

    private void OlvideContrasena_Click(object sender, RoutedEventArgs e)
    {
        var ventana = new RecuperarContrasenaWindow { Owner = this };
        ventana.ShowDialog();
    }
}