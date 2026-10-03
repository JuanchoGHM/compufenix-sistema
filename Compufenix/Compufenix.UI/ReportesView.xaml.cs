using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;

namespace Compufenix.UI;

public partial class ReportesView : UserControl
{
    private class FilaEstadoMostrar
    {
        public string NombreEstado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public ReportesView()
    {
        InitializeComponent();

        FechaDesde.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        FechaHasta.SelectedDate = DateTime.Now;

        CargarReportes();
    }

    private void CargarReportes()
    {
        using var db = Configuracion.CrearDb();
        var servicio = new ServicioReportes(db);

        // Inventario
        TxtTotalProductos.Text = servicio.TotalProductos().ToString();
        TxtTotalUnidades.Text = servicio.TotalUnidadesEnStock().ToString();

        var stockBajo = servicio.ProductosConStockBajo();
        ListaStockBajo.ItemsSource = stockBajo;
        TxtSinAlertas.Visibility = stockBajo.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Tickets por estado
        var estados = servicio.TicketsPorEstado()
            .Select(f => new FilaEstadoMostrar
            {
                NombreEstado = Textos.Mostrar(f.Estado),
                Cantidad = f.Cantidad
            }).ToList();

        ListaEstados.ItemsSource = estados;
        TxtSinTickets.Visibility = estados.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Ingresos con el rango de fechas actual
        CalcularIngresos();
    }

    private void CalcularIngresos()
    {
        if (FechaDesde.SelectedDate is not DateTime desde ||
            FechaHasta.SelectedDate is not DateTime hasta)
        {
            return;
        }

        using var db = Configuracion.CrearDb();
        var total = new ServicioReportes(db).IngresosEntre(desde, hasta);
        TxtIngresos.Text = total.ToString("C");
    }

    private void Calcular_Click(object sender, RoutedEventArgs e)
    {
        CalcularIngresos();
    }
}