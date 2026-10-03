using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

    // ===== Navegación entre pantallas =====

    private void MostrarInicio()
    {
        VistaModulo.Content = null;
        VistaModulo.Visibility = Visibility.Collapsed;
        VistaInicio.Visibility = Visibility.Visible;
        MarcarActivo(BtnInicio);
        CargarResumen(); // refresca los números
    }

    private void MostrarModulo(UserControl vista, Button boton)
    {
        VistaInicio.Visibility = Visibility.Collapsed;
        VistaModulo.Content = vista;
        VistaModulo.Visibility = Visibility.Visible;
        MarcarActivo(boton);
    }

    // Resalta el botón de la sección en la que estás
    private void MarcarActivo(Button activo)
    {
        var botones = new[] { BtnInicio, BtnInventario, BtnTickets, BtnClientes, BtnReportes, BtnUsuarios };

        foreach (var boton in botones)
        {
            bool esActivo = boton == activo;

            boton.Background = esActivo
                ? new SolidColorBrush(Color.FromRgb(0xEA, 0x58, 0x0C))
                : Brushes.Transparent;

            boton.Foreground = esActivo
                ? Brushes.White
                : new SolidColorBrush(Color.FromRgb(0xBF, 0xDB, 0xFE));
        }
    }

    private void Inicio_Click(object sender, RoutedEventArgs e)
    {
        MostrarInicio();
    }

    private void Inventario_Click(object sender, RoutedEventArgs e)
    {
        MostrarModulo(new InventarioView(), BtnInventario);
    }

    private void Clientes_Click(object sender, RoutedEventArgs e)
    {
        MostrarModulo(new ClientesView(), BtnClientes);
    }

    private void Tickets_Click(object sender, RoutedEventArgs e)
    {
        MostrarModulo(new TicketsView(), BtnTickets);
    }

    private void Usuarios_Click(object sender, RoutedEventArgs e)
    {
        MostrarModulo(new UsuariosView(), BtnUsuarios);
    }

    private void Reportes_Click(object sender, RoutedEventArgs e)
    {
        MostrarModulo(new ReportesView(), BtnReportes);
    }

    // ===== Resumen del Inicio =====

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

    // ===== Cerrar sesión =====

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