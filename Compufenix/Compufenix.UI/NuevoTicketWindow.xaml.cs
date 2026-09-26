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

    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (CmbEquipo.SelectedItem is not OpcionEquipo equipoElegido)
        {
            MostrarError("Debes seleccionar un equipo.");
            return;
        }

        var tecnicoElegido = CmbTecnico.SelectedItem as Usuario;

        var ticket = new Ticket
        {
            IdEquipo = equipoElegido.IdEquipo,
            IdTecnico = tecnicoElegido?.IdUsuario,
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