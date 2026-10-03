using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class MainWindow : Window
{
    private MonitorInactividad? _monitorInactividad;

    private class FilaTicketAntiguoMostrar
    {
        public int IdTicket { get; set; }
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

        _monitorInactividad = new MonitorInactividad(this, TimeSpan.FromMinutes(10));
        _monitorInactividad.SesionExpirada += () => Dispatcher.Invoke(CerrarPorInactividad);

        ActualizarBotonTema();

        this.SizeChanged += (s, e) => AjustarResponsive();
        AjustarResponsive();
    }

    // ===== Navegación entre pantallas =====

    private void MostrarInicio()
    {
        VistaModulo.Content = null;
        VistaModulo.Visibility = Visibility.Collapsed;
        VistaInicio.Visibility = Visibility.Visible;
        MarcarActivo(BtnInicio);
        CargarResumen(); // refresca los números
        ActualizarBadgeStockBajo();
        AnimarEntrada(VistaInicio);
    }

    private void MostrarModulo(UserControl vista, Button boton)
    {
        VistaInicio.Visibility = Visibility.Collapsed;
        VistaModulo.Content = vista;
        VistaModulo.Visibility = Visibility.Visible;
        MarcarActivo(boton);
        AnimarEntrada(VistaModulo);
        ActualizarBadgeStockBajo(); // se actualiza aunque no estés viendo el Inicio
    }

    // Aparición suave de la pantalla (fundido de 0 a 1 en 200 milisegundos)
    private void AnimarEntrada(UIElement elemento)
    {
        elemento.Opacity = 0;
        var animacion = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
        elemento.BeginAnimation(UIElement.OpacityProperty, animacion);
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


    // ===== Tarjetas del Inicio: navegación rápida =====

    private void TarjetaProductos_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var vista = new InventarioView();
        vista.DatosCambiaron += ActualizarBadgeStockBajo;
        MostrarModulo(vista, BtnInventario);
    }

    private void TarjetaStockBajo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var vista = new InventarioView(soloStockBajo: true);
        vista.DatosCambiaron += ActualizarBadgeStockBajo;
        MostrarModulo(vista, BtnInventario);
    }

    private void TarjetaTickets_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        MostrarModulo(new TicketsView(soloAbiertos: true), BtnTickets);
    }

    private void TarjetaClientes_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        MostrarModulo(new ClientesView(), BtnClientes);
    }

    private void Inicio_Click(object sender, RoutedEventArgs e)
    {
        MostrarInicio();
    }

    private void Inventario_Click(object sender, RoutedEventArgs e)
    {
        var vista = new InventarioView();
        vista.DatosCambiaron += ActualizarBadgeStockBajo;
        MostrarModulo(vista, BtnInventario);
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


    // Se puede llamar aunque el Inicio no esté visible: actualiza solo el punto rojo del menú
    private void ActualizarBadgeStockBajo()
    {
        try
        {
            using var db = Configuracion.CrearDb();
            var resumen = new ServicioResumen(db).Obtener();

            if (resumen.StockBajo > 0)
            {
                BadgeStockBajo.Visibility = Visibility.Visible;
                TxtBadgeStockBajo.Text = resumen.StockBajo.ToString();
            }
            else
            {
                BadgeStockBajo.Visibility = Visibility.Collapsed;
            }
        }
        catch
        {
            // Si falla, se deja el badge como estaba
        }
    }

    private void AlternarTema_Click(object sender, RoutedEventArgs e)
    {
        GestorTema.Alternar();
        ActualizarBotonTema();
    }

    private void ActualizarBotonTema()
    {
        if (GestorTema.EsOscuro)
        {
            IconoTema.Text = "\uE706"; // sol
            TxtTema.Text = "Modo claro";
        }
        else
        {
            IconoTema.Text = "\uE708"; // luna
            TxtTema.Text = "Modo oscuro";
        }
    }


    private bool _sidebarColapsada;

    private void AjustarResponsive()
    {
        bool debeColapsar = ActualWidth < 1100;

        if (debeColapsar == _sidebarColapsada) return; // ya está en el estado correcto
        _sidebarColapsada = debeColapsar;

        ColSidebar.Width = new GridLength(debeColapsar ? 72 : 260);

        var visibilidadTexto = debeColapsar ? Visibility.Collapsed : Visibility.Visible;
        var paddingBoton = debeColapsar ? new Thickness(0, 11, 0, 11) : new Thickness(14, 11, 14, 11);
        var alineacionBoton = debeColapsar ? HorizontalAlignment.Center : HorizontalAlignment.Stretch;

        foreach (var boton in new[] { BtnInicio, BtnInventario, BtnTickets, BtnClientes, BtnReportes, BtnUsuarios })
        {
            boton.Padding = paddingBoton;
            boton.HorizontalContentAlignment = alineacionBoton;
        }

        // El panel lateral necesita menos margen cuando está colapsado,
        // o no le queda espacio real a ningún contenido
        PanelLateral.Margin = debeColapsar
            ? new Thickness(8, 28, 8, 20)
            : new Thickness(20, 28, 20, 20);

        // Los botones de abajo (tema y cerrar sesión) también deben achicar su padding
        var paddingBotonInferior = debeColapsar ? new Thickness(0, 10, 0, 10) : new Thickness(14, 10, 14, 10);
        BtnTema.Padding = paddingBotonInferior;
        BtnCerrarSesion.Padding = paddingBotonInferior;

        TxtLogoTexto.Visibility = visibilidadTexto;
        TxtLabelInicio.Visibility = visibilidadTexto;
        TxtLabelInventario.Visibility = visibilidadTexto;
        TxtLabelTickets.Visibility = visibilidadTexto;
        TxtLabelClientes.Visibility = visibilidadTexto;
        TxtLabelReportes.Visibility = visibilidadTexto;
        TxtLabelUsuarios.Visibility = visibilidadTexto;
        TxtNombreUsuario.Visibility = visibilidadTexto;
        TxtRolUsuario.Visibility = visibilidadTexto;
        TxtTema.Visibility = visibilidadTexto;
        TxtLabelCerrarSesion.Visibility = visibilidadTexto;

        int columnasTarjetas = ActualWidth switch
        {
            < 800 => 1,
            < 1300 => 2,
            _ => 4
        };
        GridTarjetas.Columns = columnasTarjetas;
    }

    private void CargarInicioExtra()
    {
        using var db = Configuracion.CrearDb();
        var servicio = new ServicioReportes(db);

        // Tickets más antiguos sin resolver
        var antiguos = servicio.TicketsMasAntiguosSinResolver()
            .Select(t => new FilaTicketAntiguoMostrar
            {
                IdTicket = t.IdTicket,
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


    private void TicketAntiguo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var borde = (Border)sender;
        var idTicket = (int)borde.Tag;

        var ventana = new VerTicketWindow(idTicket) { Owner = this };
        ventana.ShowDialog();
    }


    private void BarraCliente_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var grid = (Grid)sender;
        var nombreCliente = (string)grid.Tag;

        var vista = new TicketsView();
        MostrarModulo(vista, BtnTickets);
        vista.FiltrarPorTexto(nombreCliente);
    }


    // ===== Cerrar sesión =====

    private void CerrarSesion_Click(object sender, RoutedEventArgs e)
    {
        _monitorInactividad?.Detener();
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

    private void CerrarPorInactividad()
    {
        _monitorInactividad?.Detener();
        Sesion.UsuarioActual = null;
        Hide();

        AvisoDialog.Mostrar(this, "Sesión cerrada",
            "Se cerró tu sesión automáticamente por inactividad.", esError: true);

        var login = new LoginWindow();
        if (login.ShowDialog() == true)
        {
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