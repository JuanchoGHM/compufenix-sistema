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
        this.SizeChanged += (s, e) => AjustarResponsive();
        this.Loaded += (s, e) => AjustarResponsive();
        this.ContentRendered += (s, e) => AjustarResponsive();
        AjustarResponsive();
    }

    private void AjustarResponsive()
    {
        bool anchoPequeno = ActualWidth < 850;

        ColPanelMarca.Width = anchoPequeno ? new GridLength(0) : new GridLength(1.1, GridUnitType.Star);
        PanelMarcaLogin.Visibility = anchoPequeno ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        var contrasena = ObtenerContrasena();
        var contrasena2 = ObtenerContrasena2();

        if (contrasena != contrasena2)
        {
            AvisoDialog.Mostrar(this, "Las contraseñas no coinciden",
                "Escribe la misma contraseña en los dos campos.", esError: true);
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

            AvisoDialog.Mostrar(this, "Administrador creado",
                "Administrador creado correctamente.");
            DialogResult = true;
        }
        catch (Exception ex)
        {
            AvisoDialog.Mostrar(this, "No se pudo crear el administrador", ex.Message, esError: true);
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