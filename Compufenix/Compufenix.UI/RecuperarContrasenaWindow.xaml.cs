using System.Windows;
using Compufenix.Business;

namespace Compufenix.UI;

public partial class RecuperarContrasenaWindow : Window
{
    private string _correo = string.Empty;
    private bool _nuevaVisible;
    private bool _repetirVisible;

    public RecuperarContrasenaWindow()
    {
        InitializeComponent();
        this.CentrarEnPantalla();
    }

    private void EnviarCodigo_Click(object sender, RoutedEventArgs e)
    {
        PanelError1.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(TxtCorreo.Text))
        {
            MostrarError1("Escribe tu correo.");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            var codigo = new ServicioUsuarios(db).GenerarCodigoRecuperacion(TxtCorreo.Text);

            if (codigo == null)
            {
                MostrarError1("No se encontró un usuario activo con ese correo.");
                return;
            }

            ServicioCorreo.EnviarCodigoRecuperacion(TxtCorreo.Text, codigo);

            _correo = TxtCorreo.Text.Trim();
            TxtCorreoEnviado.Text = $"Se envió un código a {_correo}";
            PanelPaso1.Visibility = Visibility.Collapsed;
            PanelPaso2.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MostrarError1("No se pudo enviar el correo: " + ex.Message);
        }
    }

    private void MostrarNueva_Click(object sender, RoutedEventArgs e)
    {
        _nuevaVisible = !_nuevaVisible;

        if (_nuevaVisible)
        {
            TxtNuevaContrasenaVisible.Text = TxtNuevaContrasena.Password;
            TxtNuevaContrasena.Visibility = Visibility.Collapsed;
            TxtNuevaContrasenaVisible.Visibility = Visibility.Visible;
        }
        else
        {
            TxtNuevaContrasena.Password = TxtNuevaContrasenaVisible.Text;
            TxtNuevaContrasenaVisible.Visibility = Visibility.Collapsed;
            TxtNuevaContrasena.Visibility = Visibility.Visible;
        }
    }

    private void MostrarRepetir_Click(object sender, RoutedEventArgs e)
    {
        _repetirVisible = !_repetirVisible;

        if (_repetirVisible)
        {
            TxtRepetirContrasenaVisible.Text = TxtRepetirContrasena.Password;
            TxtRepetirContrasena.Visibility = Visibility.Collapsed;
            TxtRepetirContrasenaVisible.Visibility = Visibility.Visible;
        }
        else
        {
            TxtRepetirContrasena.Password = TxtRepetirContrasenaVisible.Text;
            TxtRepetirContrasenaVisible.Visibility = Visibility.Collapsed;
            TxtRepetirContrasena.Visibility = Visibility.Visible;
        }
    }

    private string ObtenerNuevaContrasena() =>
        _nuevaVisible ? TxtNuevaContrasenaVisible.Text : TxtNuevaContrasena.Password;

    private string ObtenerRepetirContrasena() =>
        _repetirVisible ? TxtRepetirContrasenaVisible.Text : TxtRepetirContrasena.Password;

    private void Restablecer_Click(object sender, RoutedEventArgs e)
    {
        PanelError2.Visibility = Visibility.Collapsed;

        var nueva = ObtenerNuevaContrasena();
        var repetir = ObtenerRepetirContrasena();

        if (nueva != repetir)
        {
            MostrarError2("Las contraseñas no coinciden.");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioUsuarios(db).RestablecerContrasena(_correo, TxtCodigo.Text.Trim(), nueva);

            AvisoDialog.Mostrar(this, "Contraseña actualizada",
                "Tu contraseña se cambió correctamente. Ya puedes iniciar sesión.");
            Close();
        }
        catch (Exception ex)
        {
            MostrarError2(ex.Message);
        }
    }

    private void MostrarError1(string mensaje)
    {
        TxtError1.Text = mensaje;
        PanelError1.Visibility = Visibility.Visible;
    }

    private void MostrarError2(string mensaje)
    {
        TxtError2.Text = mensaje;
        PanelError2.Visibility = Visibility.Visible;
    }
}