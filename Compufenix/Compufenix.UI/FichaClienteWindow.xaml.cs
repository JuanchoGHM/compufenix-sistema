using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class FichaClienteWindow : Window
{
    private readonly Cliente _cliente;

    private class FilaEquipo
    {
        public int IdEquipo { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }

    public FichaClienteWindow(Cliente cliente)
    {
        InitializeComponent();
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

        var equipo = new Equipo
        {
            IdCliente = _cliente.IdCliente,
            Tipo = TxtTipo.Text,
            Marca = TxtMarca.Text,
            Modelo = TxtModelo.Text,
            NumeroSerie = TxtNumeroSerie.Text
        };

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioEquipos(db).Registrar(equipo);

            TxtTipo.Clear();
            TxtMarca.Clear();
            TxtModelo.Clear();
            TxtNumeroSerie.Clear();

            CargarEquipos();
        }
        catch (Exception ex)
        {
            TxtError.Text = ex.Message;
            PanelError.Visibility = Visibility.Visible;
        }
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