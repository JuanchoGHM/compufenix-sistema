using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class MovimientoWindow : Window
{
    private readonly Producto _producto;

    // Un "paquetito" con los datos ya listos para mostrar en el historial
    private class FilaHistorial
    {
        public string Descripcion { get; set; } = string.Empty;
        public string FechaTexto { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public MovimientoWindow(Producto producto)
    {
        InitializeComponent();
        _producto = producto;
        TxtProducto.Text = $"{producto.Nombre} · Stock actual: {producto.StockActual}";
        CargarHistorial();
    }

    private void CargarHistorial()
    {
        using var db = Configuracion.CrearDb();
        var movimientos = new ServicioInventario(db).ObtenerMovimientos(_producto.IdProducto);

        var filas = movimientos.Select(m => new FilaHistorial
        {
            Descripcion = (m.Tipo == TipoMovimiento.Entrada ? "Entrada de " : "Salida de ") + m.Cantidad + " unidad(es)",
            FechaTexto = m.Fecha.ToString("dd/MM/yyyy HH:mm"),
            Color = m.Tipo == TipoMovimiento.Entrada ? "#22C55E" : "#EF4444"
        }).Take(8).ToList();

        ListaHistorial.ItemsSource = filas;
        TxtSinMovimientos.Visibility = filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Registrar_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (!int.TryParse(TxtCantidad.Text, out int cantidad))
        {
            MostrarError("La cantidad debe ser un número entero.");
            return;
        }

        var tipo = OpEntrada.IsChecked == true ? TipoMovimiento.Entrada : TipoMovimiento.Salida;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioInventario(db).RegistrarMovimiento(_producto.IdProducto, tipo, cantidad);
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