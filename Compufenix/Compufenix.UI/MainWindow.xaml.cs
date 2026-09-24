using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var usuario = Sesion.UsuarioActual;
        if (usuario != null)
        {
            TxtBienvenida.Text = $"Hola, {usuario.Nombre}";
            TxtNombreUsuario.Text = usuario.Nombre;
            TxtRolUsuario.Text = usuario.Rol == RolUsuario.Administrador
                ? "Administrador"
                : "Técnico";
        }

        // Estas opciones son solo para el administrador
        var visibilidadAdmin = Sesion.EsAdministrador
            ? Visibility.Visible
            : Visibility.Collapsed;

        BtnClientes.Visibility = visibilidadAdmin;
        BtnReportes.Visibility = visibilidadAdmin;
        BtnUsuarios.Visibility = visibilidadAdmin;

        CargarResumen();
    }

    // Llena las 4 tarjetas con datos reales
    private void CargarResumen()
    {
        try
        {
            using var db = Configuracion.CrearDb();
            var resumen = new ServicioResumen(db).Obtener();

            TxtProductos.Text = resumen.Productos.ToString();
            TxtStockBajo.Text = resumen.StockBajo.ToString();
            TxtTicketsAbiertos.Text = resumen.TicketsAbiertos.ToString();
            TxtClientes.Text = resumen.Clientes.ToString();
        }
        catch
        {
            // Si falla, las tarjetas se quedan con "—"
        }
    }

    private void CerrarSesion_Click(object sender, RoutedEventArgs e)
    {
        Sesion.UsuarioActual = null;
        Hide();

        var login = new LoginWindow();
        if (login.ShowDialog() == true)
        {
            // Abre una ventana principal nueva con el usuario que entró
            var nueva = new MainWindow();
            Application.Current.MainWindow = nueva;
            nueva.Show();
        }
        else
        {
            Application.Current.Shutdown();
        }

        Close();
    }
}