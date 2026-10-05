using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class FichaClienteWindow : Window
{
    private readonly Cliente _cliente;
    private int? _idEquipoEditando; // null = agregando un equipo nuevo

    private class FilaEquipo
    {
        public int IdEquipo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? NumeroSerie { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }

    public FichaClienteWindow(Cliente cliente)
    {
        InitializeComponent();
        MaxHeight = SystemParameters.WorkArea.Height - 20; // nunca más alta que la pantalla
        this.CentrarEnPantalla();
        _cliente = cliente;

        TxtNombreCliente.Text = cliente.Nombre;
        TxtDatosCliente.Text = $"{cliente.Telefono} · {cliente.Correo}";

        CargarEquipos();
    }

    private void CargarEquipos()
    {
        using var db = Configuracion.CrearDb();
        var equipos = new ServicioEquipos(db).ObtenerDeCliente(_cliente.IdCliente);

        var filas = equipos.Select(eq => new FilaEquipo
        {
            IdEquipo = eq.IdEquipo,
            Tipo = eq.Tipo,
            Marca = eq.Marca,
            Modelo = eq.Modelo,
            NumeroSerie = eq.NumeroSerie,
            Titulo = $"{eq.Tipo} {eq.Marca} {eq.Modelo}".Trim(),
            Detalle = string.IsNullOrWhiteSpace(eq.NumeroSerie)
                ? "Sin número de serie"
                : $"N.° de serie: {eq.NumeroSerie}"
        }).ToList();

        ListaEquipos.ItemsSource = filas;
        TxtSinEquipos.Visibility = filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void AgregarEquipo_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioEquipos(db);

            if (_idEquipoEditando is int idEquipo)
            {
                servicio.Actualizar(idEquipo, TxtTipo.Text, TxtMarca.Text, TxtModelo.Text, TxtNumeroSerie.Text);
            }
            else
            {
                servicio.Registrar(new Equipo
                {
                    IdCliente = _cliente.IdCliente,
                    Tipo = TxtTipo.Text,
                    Marca = TxtMarca.Text,
                    Modelo = TxtModelo.Text,
                    NumeroSerie = TxtNumeroSerie.Text
                });
            }

            SalirModoEdicion();
            CargarEquipos();
        }
        catch (Exception ex)
        {
            TxtError.Text = ex.Message;
            PanelError.Visibility = Visibility.Visible;
        }
    }

    private void EditarEquipo_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not FilaEquipo fila) return;

        PanelError.Visibility = Visibility.Collapsed;
        _idEquipoEditando = fila.IdEquipo;

        TxtTipo.Text = fila.Tipo;
        TxtMarca.Text = fila.Marca ?? string.Empty;
        TxtModelo.Text = fila.Modelo ?? string.Empty;
        TxtNumeroSerie.Text = fila.NumeroSerie ?? string.Empty;

        TxtTituloFormulario.Text = "Editar equipo";
        BtnGuardarEquipo.Content = "Guardar cambios";
        BtnCancelarEdicion.Visibility = Visibility.Visible;
    }

    private void CancelarEdicion_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;
        SalirModoEdicion();
    }

    // Vuelve el formulario al modo "agregar equipo"
    private void SalirModoEdicion()
    {
        _idEquipoEditando = null;
        TxtTipo.Clear();
        TxtMarca.Clear();
        TxtModelo.Clear();
        TxtNumeroSerie.Clear();
        TxtTituloFormulario.Text = "Agregar equipo";
        BtnGuardarEquipo.Content = "+ Agregar equipo";
        BtnCancelarEdicion.Visibility = Visibility.Collapsed;
    }

    private void EliminarEquipo_Click(object sender, RoutedEventArgs e)
    {
        var boton = (System.Windows.Controls.Button)sender;
        var idEquipo = (int)boton.Tag;

        bool confirmado = ConfirmDialog.Mostrar(
            this, "Eliminar equipo",
            "¿Seguro que deseas eliminar este equipo? Esta acción no se puede deshacer.",
            "Eliminar");

        if (!confirmado) return;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioEquipos(db).Eliminar(idEquipo);
            if (_idEquipoEditando == idEquipo) SalirModoEdicion();
            CargarEquipos();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}