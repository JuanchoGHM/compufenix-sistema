using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class FichaTicketWindow : Window
{
    private int _idTicket;

    private class OpcionProducto
    {
        public int IdProducto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    private class FilaRepuesto
    {
        public string Descripcion { get; set; } = string.Empty;
        public string FechaTexto { get; set; } = string.Empty;
    }

    public FichaTicketWindow(int idTicket)
    {
        InitializeComponent();
        _idTicket = idTicket;

        CmbEstado.ItemsSource = Enum.GetValues<EstadoTicket>();

        CargarTodo();
    }

    private void CargarTodo()
    {
        using var db = Configuracion.CrearDb();
        var servicio = new ServicioTickets(db);
        var ticket = servicio.ObtenerPorId(_idTicket);

        TxtTitulo.Text = $"Ticket #{ticket.IdTicket} — {ticket.Equipo!.Cliente!.Nombre}";
        TxtSubtitulo.Text = $"{ticket.Equipo.Tipo} {ticket.Equipo.Marca} · " +
            $"Técnico: {(ticket.Tecnico != null ? ticket.Tecnico.Nombre : "Sin asignar")}";
        CmbEstado.SelectedItem = ticket.Estado;
        TxtCostoTotal.Text = ticket.CostoTotal.ToString("C");

        var productos = new ServicioInventario(db).Buscar();
        CmbProducto.ItemsSource = productos.Select(p => new OpcionProducto
        {
            IdProducto = p.IdProducto,
            Descripcion = $"{p.Nombre} (Stock: {p.StockActual})"
        }).ToList();

        var repuestos = servicio.ObtenerRepuestosUsados(_idTicket);
        var filas = repuestos.Select(m => new FilaRepuesto
        {
            Descripcion = $"{m.Cantidad} × {m.Producto!.Nombre}",
            FechaTexto = m.Fecha.ToString("dd/MM/yyyy HH:mm")
        }).ToList();

        ListaRepuestos.ItemsSource = filas;
        TxtSinRepuestos.Visibility = filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void GuardarEstado_Click(object sender, RoutedEventArgs e)
    {
        if (CmbEstado.SelectedItem is not EstadoTicket estado) return;

        using var db = Configuracion.CrearDb();
        new ServicioTickets(db).CambiarEstado(_idTicket, estado);

        TxtEstadoGuardado.Visibility = Visibility.Visible;

        var temporizador = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        temporizador.Tick += (s, args) =>
        {
            TxtEstadoGuardado.Visibility = Visibility.Collapsed;
            temporizador.Stop();
        };
        temporizador.Start();
    }

    private void UsarRepuesto_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (CmbProducto.SelectedItem is not OpcionProducto producto)
        {
            MostrarError("Debes seleccionar un producto.");
            return;
        }

        if (!int.TryParse(TxtCantidad.Text, out int cantidad))
        {
            MostrarError("La cantidad debe ser un número entero.");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioTickets(db).UsarRepuesto(_idTicket, producto.IdProducto, cantidad);

            TxtCantidad.Clear();
            CargarTodo(); // refresca costo, historial y stock disponible en el ComboBox
        }
        catch (Exception ex)
        {
            MostrarError(ex.Message);
        }
    }

    private void MostrarError(string mensaje)
    {
        TxtError.Text = mensaje;
        PanelError.Visibility = Visibility.Visible;
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}