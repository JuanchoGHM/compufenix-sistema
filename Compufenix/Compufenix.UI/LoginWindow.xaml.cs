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
        catch (Exception ex)
        {
            TxtError.Text = "No se pudo iniciar sesión: " + ex.Message;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}