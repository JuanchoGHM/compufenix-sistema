using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class MainWindow : Window
{
    private class FilaTicketAntiguoMostrar
    {
        public string Cliente { get; set; } = string.Empty;
        public string Equipo { get; set; } = string.Empty;
        public string NombreEstado { get; set; } = string.Empty;
        public int DiasAbierto { get; set; }
    }

    private class FilaClienteMostrar
    {
        public string Cliente { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double AnchoBarra { get; set; }
    }

    private class FilaMesMostrar
    {
        public string Mes { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double AlturaBarra { get; set; }
    }


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

            CargarInicioExtra();
        }
        catch
        {
            // Si falla, las tarjetas se quedan con "—"
        }
    }

    private void CargarInicioExtra()
    {
        using var db = Configuracion.CrearDb();
        var servicio = new ServicioReportes(db);

        // Tickets más antiguos sin resolver
        var antiguos = servicio.TicketsMasAntiguosSinResolver()
            .Select(t => new FilaTicketAntiguoMostrar
            {
                Cliente = t.Cliente,
                Equipo = t.Equipo,
                NombreEstado = Textos.Mostrar(t.Estado),
                DiasAbierto = t.DiasAbierto
            }).ToList();

        ListaTicketsAntiguos.ItemsSource = antiguos;
        TxtSinAntiguos.Visibility = antiguos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Top clientes con más tickets (barra proporcional al máximo, hasta 220px)
        var topClientes = servicio.TopClientesPorTickets();
        int maximo = topClientes.Count > 0 ? topClientes.Max(c => c.CantidadTickets) : 1;

        var filasTop = topClientes.Select(c => new FilaClienteMostrar
        {
            Cliente = c.Cliente,
            Cantidad = c.CantidadTickets,
            AnchoBarra = (double)c.CantidadTickets / maximo * 220
        }).ToList();

        ListaTopClientes.ItemsSource = filasTop;
        TxtSinTopClientes.Visibility = filasTop.Count == 0 ? Visibility.Visible : Visibility.Collapsed;


        // Tickets atendidos por mes (barra proporcional al mes con más tickets, hasta 120px)
        var porMes = servicio.TicketsPorMes();
        int maximoMes = porMes.Count > 0 ? Math.Max(porMes.Max(m => m.Cantidad), 1) : 1;

        ListaTicketsPorMes.ItemsSource = porMes.Select(m => new FilaMesMostrar
        {
            Mes = m.Mes,
            Cantidad = m.Cantidad,
            AlturaBarra = Math.Max((double)m.Cantidad / maximoMes * 120, 4)
        }).ToList();
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