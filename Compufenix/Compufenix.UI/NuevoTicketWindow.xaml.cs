using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class NuevoTicketWindow : Window
{
    // Envuelve un Equipo agregando un texto listo para mostrar en el ComboBox
    private class OpcionEquipo
    {
        public int IdEquipo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public NuevoTicketWindow()
    {
        InitializeComponent();
        MaxHeight = SystemParameters.WorkArea.Height - 20; // nunca más alta que la pantalla
        FechaIngreso.SelectedDate = DateTime.Now;
        CargarListas();
        this.CentrarEnPantalla();

    }

    private void CargarListas()
    {
        using var db = Configuracion.CrearDb();

        var equipos = new ServicioEquipos(db).ObtenerTodosConCliente();
        CmbEquipo.ItemsSource = equipos.Select(eq => new OpcionEquipo
        {
            IdEquipo = eq.IdEquipo,
            Descripcion = $"{eq.Cliente!.Nombre} — {eq.Tipo} {eq.Marca}".Trim()
        }).ToList();

        var tecnicos = db.Usuarios
            .Where(u => u.Rol == RolUsuario.Tecnico && u.Activo)
            .ToList();
        CmbTecnico.ItemsSource = tecnicos;
    }

    // Abre el registro rápido de cliente + equipo y deja el equipo nuevo seleccionado
    private void NuevoCliente_Click(object sender, RoutedEventArgs e)
    {
        var ventana = new ClienteRapidoWindow { Owner = this };
        if (ventana.ShowDialog() != true) return;

        CargarListas();
        CmbEquipo.SelectedItem = (CmbEquipo.ItemsSource as IEnumerable<OpcionEquipo>)?
            .FirstOrDefault(o => o.IdEquipo == ventana.IdEquipoCreado);
    }

    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (CmbEquipo.SelectedItem is not OpcionEquipo equipoElegido)
        {
            MostrarError("Debes seleccionar un equipo.");
            return;
        }
        if (FechaIngreso.SelectedDate is not DateTime fecha)
        {
            MostrarError("Debes seleccionar una fecha de ingreso.");
            return;
        }

        var tecnicoElegido = CmbTecnico.SelectedItem as Usuario;

        var ticket = new Ticket
        {
            IdEquipo = equipoElegido.IdEquipo,
            IdTecnico = tecnicoElegido?.IdUsuario,
            FechaIngreso = fecha,
            Diagnostico = TxtDiagnostico.Text
        };

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioTickets(db).Crear(ticket);
            DialogResult = true;
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

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}