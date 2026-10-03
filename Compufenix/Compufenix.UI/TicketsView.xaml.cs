using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class TicketsView : UserControl
{
    private List<Ticket> _ticketsActuales = new();

    public TicketsView()
    {
        InitializeComponent();
        CargarTickets();
    }

    private void CargarTickets(string? texto = null)
    {
        try
        {
            using var db = Configuracion.CrearDb();
            _ticketsActuales = new ServicioTickets(db).Buscar(texto);
            TablaTickets.ItemsSource = _ticketsActuales;
        }
        catch
        {
            // La tabla se queda vacía si falla
        }
    }

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
    {
        CargarTickets(TxtBuscar.Text);
    }
    private void Editar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var ticket = (Ticket)boton.DataContext;

        var ventana = new EditarTicketWindow(ticket) { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarTickets(TxtBuscar.Text);
        }
    }

    private void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var ticket = (Ticket)boton.DataContext;

        bool confirmado = ConfirmDialog.Mostrar(
            Window.GetWindow(this),
            "Eliminar ticket",
            $"¿Seguro que deseas eliminar el ticket #{ticket.IdTicket}? Esta acción no se puede deshacer.",
            "Eliminar");

        if (!confirmado) return;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioTickets(db).Eliminar(ticket.IdTicket);
            CargarTickets(TxtBuscar.Text);
        }
        catch (Exception ex)
        {
            AvisoDialog.Mostrar(Window.GetWindow(this), "No se pudo eliminar", ex.Message, esError: true);
        }
    }

    private void Exportar_Click(object sender, RoutedEventArgs e)
    {
        MenuExportar.PlacementTarget = (Button)sender;
        MenuExportar.IsOpen = true;
    }

    private void NuevoTicket_Click(object sender, RoutedEventArgs e)
    {
        var ventana = new NuevoTicketWindow { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarTickets(TxtBuscar.Text);
        }
    }

    private void TablaTickets_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (TablaTickets.SelectedItem is not Ticket ticket) return;

        var ventana = new FichaTicketWindow(ticket.IdTicket) { Owner = Window.GetWindow(this) };
        ventana.ShowDialog();
        CargarTickets(TxtBuscar.Text);
    }

    private void ExportarExcel_Click(object sender, RoutedEventArgs e)
    {
        MenuExportar.IsOpen = false;

        var dialogo = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = "Tickets_" + DateTime.Now.ToString("yyyyMMdd")
        };

        if (dialogo.ShowDialog() != true) return;

        try
        {
            ExportadorTickets.ExportarExcel(_ticketsActuales, dialogo.FileName);
            AvisoDialog.Mostrar(Window.GetWindow(this), "Excel generado", "El archivo se generó correctamente.");
        }
        catch (Exception ex)
        {
            AvisoDialog.Mostrar(Window.GetWindow(this), "No se pudo generar el archivo", ex.Message, esError: true);
        }
    }

    private void ExportarPdf_Click(object sender, RoutedEventArgs e)
    {
        MenuExportar.IsOpen = false;

        var dialogo = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = "Tickets_" + DateTime.Now.ToString("yyyyMMdd")
        };

        if (dialogo.ShowDialog() != true) return;

        try
        {
            ExportadorTickets.ExportarPdf(_ticketsActuales, dialogo.FileName);
            AvisoDialog.Mostrar(Window.GetWindow(this), "PDF generado", "El archivo se generó correctamente.");
        }
        catch (Exception ex)
        {
            AvisoDialog.Mostrar(Window.GetWindow(this), "No se pudo generar el archivo", ex.Message, esError: true);
        }
    }
}