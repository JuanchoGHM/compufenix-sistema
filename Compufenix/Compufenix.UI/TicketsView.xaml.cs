using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;

namespace Compufenix.UI;

public partial class TicketsView : UserControl
{
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
            TablaTickets.ItemsSource = new ServicioTickets(db).Buscar(texto);
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
        if (TablaTickets.SelectedItem is not Compufenix.Models.Ticket ticket) return;

        var ventana = new FichaTicketWindow(ticket.IdTicket) { Owner = Window.GetWindow(this) };
        ventana.ShowDialog();
        CargarTickets(TxtBuscar.Text);
    }
}