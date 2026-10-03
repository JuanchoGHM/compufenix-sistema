using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class EditarTicketWindow : Window
{
    private readonly int _idTicket;
    private readonly int _idEquipoOriginal;

    private class OpcionEquipo
    {
        public int IdEquipo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public EditarTicketWindow(Ticket ticket)
    {
        InitializeComponent();
        this.CentrarEnPantalla();

        _idTicket = ticket.IdTicket;
        _idEquipoOriginal = ticket.IdEquipo;

        TxtSubtitulo.Text = $"Ticket #{ticket.IdTicket}";
        TxtDiagnostico.Text = ticket.Diagnostico;
        FechaIngreso.SelectedDate = ticket.FechaIngreso;

        using var db = Configuracion.CrearDb();

        var clientes = new ServicioClientes(db).Buscar();
        CmbCliente.ItemsSource = clientes;
        CmbCliente.SelectedItem = clientes.FirstOrDefault(c => c.IdCliente == ticket.Equipo!.IdCliente);
        // Al asignar SelectedItem se dispara CmbCliente_SelectionChanged, que carga los equipos

        var tecnicos = db.Usuarios.Where(u => u.Rol == RolUsuario.Tecnico && u.Activo).ToList();

        // Si el técnico asignado a este ticket fue desactivado después, lo agregamos igual
        // para no perder la información de quién lo atendió
        if (ticket.IdTecnico.HasValue && !tecnicos.Any(t => t.IdUsuario == ticket.IdTecnico))
        {
            var tecnicoInactivo = db.Usuarios.FirstOrDefault(u => u.IdUsuario == ticket.IdTecnico);
            if (tecnicoInactivo != null)
            {
                tecnicos.Add(tecnicoInactivo);
            }
        }

        CmbTecnico.ItemsSource = tecnicos;
        CmbTecnico.SelectedItem = tecnicos.FirstOrDefault(t => t.IdUsuario == ticket.IdTecnico);

        CmbEstado.ItemsSource = Enum.GetValues<EstadoTicket>();
        CmbEstado.SelectedItem = ticket.Estado;
    }

    private void CmbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbCliente.SelectedItem is not Cliente cliente)
        {
            CmbEquipo.ItemsSource = null;
            return;
        }

        using var db = Configuracion.CrearDb();
        var equipos = new ServicioEquipos(db).ObtenerPorCliente(cliente.IdCliente);

        var opciones = equipos.Select(eq => new OpcionEquipo
        {
            IdEquipo = eq.IdEquipo,
            Descripcion = $"{eq.Tipo} {eq.Marca} {eq.Modelo}".Trim()
        }).ToList();

        CmbEquipo.ItemsSource = opciones;

        // Si el equipo original pertenece a este cliente, se mantiene seleccionado
        CmbEquipo.SelectedItem = opciones.FirstOrDefault(o => o.IdEquipo == _idEquipoOriginal)
            ?? opciones.FirstOrDefault();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (CmbEquipo.SelectedItem is not OpcionEquipo equipo)
        {
            MostrarError("Debes seleccionar un equipo. Si el cliente no tiene equipos, regístralos primero en Clientes.");
            return;
        }

        if (CmbEstado.SelectedItem is not EstadoTicket estado)
        {
            MostrarError("Debes seleccionar un estado.");
            return;
        }

        if (FechaIngreso.SelectedDate is not DateTime fecha)
        {
            MostrarError("Debes seleccionar una fecha de ingreso.");
            return;
        }

        var tecnico = CmbTecnico.SelectedItem as Usuario;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioTickets(db).Editar(
                _idTicket, equipo.IdEquipo, tecnico?.IdUsuario, estado, fecha, TxtDiagnostico.Text);
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