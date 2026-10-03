using System.Windows;
using System.Windows.Media;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class VerTicketWindow : Window
{
    private class FilaRepuesto
    {
        public string Descripcion { get; set; } = string.Empty;
        public string FechaTexto { get; set; } = string.Empty;
    }
    private class FilaHistorial
    {
        public string NombreEstado { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public Visibility LineaVisible { get; set; }
    }

    public VerTicketWindow(int idTicket)
    {
        InitializeComponent();
        this.CentrarEnPantalla();

        using var db = Configuracion.CrearDb();
        var servicio = new ServicioTickets(db);
        var ticket = servicio.ObtenerPorId(idTicket);

        TxtTitulo.Text = $"Ticket #{ticket.IdTicket}";
        TxtSubtitulo.Text = "Detalle completo del ticket de servicio";

        TxtEstado.Text = Textos.Mostrar(ticket.Estado);
        ColorearEstado(ticket.Estado);

        TxtCliente.Text = ticket.Equipo!.Cliente!.Nombre;
        TxtEquipo.Text = $"{ticket.Equipo.Tipo} {ticket.Equipo.Marca} {ticket.Equipo.Modelo}".Trim();
        TxtTecnico.Text = ticket.Tecnico?.Nombre ?? "Sin asignar";
        TxtFecha.Text = ticket.FechaIngreso.ToString("dd/MM/yyyy");

        TxtDiagnostico.Text = string.IsNullOrWhiteSpace(ticket.Diagnostico)
            ? "Sin diagnóstico registrado."
            : ticket.Diagnostico;

        var historial = servicio.ObtenerHistorial(idTicket);
        var filasHistorial = historial.Select((h, indice) => new FilaHistorial
        {
            NombreEstado = Textos.Mostrar(h.Estado),
            Detalle = $"{h.Fecha:dd/MM/yyyy HH:mm}" +
                (h.Usuario != null ? $" · {h.Usuario.Nombre}" : ""),
            LineaVisible = indice < historial.Count - 1 ? Visibility.Visible : Visibility.Collapsed
        }).ToList();

        ListaHistorial.ItemsSource = filasHistorial;

        var repuestos = servicio.ObtenerRepuestosUsados(idTicket);
        var filas = repuestos.Select(m => new FilaRepuesto
        {
            Descripcion = $"{m.Cantidad} × {m.Producto!.Nombre}",
            FechaTexto = m.Fecha.ToString("dd/MM/yyyy HH:mm")
        }).ToList();

        ListaRepuestos.ItemsSource = filas;
        TxtSinRepuestos.Visibility = filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        TxtCostoTotal.Text = ticket.CostoTotal.ToString("C");
    }

    // Colorea la pastilla de estado según qué tan avanzado está el ticket
    private void ColorearEstado(EstadoTicket estado)
    {
        var (fondo, texto) = estado switch
        {
            EstadoTicket.Cancelado => ("#FEE2E2", "#991B1B"),
            EstadoTicket.Entregado => ("#DCFCE7", "#166534"),
            EstadoTicket.Reparado => ("#DCFCE7", "#166534"),
            EstadoTicket.EsperandoRepuesto => ("#FEF3C7", "#92400E"),
            _ => ("#DBEAFE", "#1E40AF") // Recibido, EnDiagnostico, EnReparacion
        };

        PildoraEstado.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fondo));
        TxtEstado.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(texto));
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}