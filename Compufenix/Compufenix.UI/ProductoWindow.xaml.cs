using System.Globalization;
using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class ProductoWindow : Window
{
    private readonly Producto _producto;

    public ProductoWindow(Producto? producto = null)
    {
        InitializeComponent();
        this.CentrarEnPantalla();

        _producto = producto ?? new Producto();

        if (producto != null)
        {
            TxtTitulo.Text = "Editar producto";
            TxtIcono.Text = "\uE70F"; // icono de lápiz
            TxtNombre.Text = producto.Nombre;
            TxtCategoria.Text = producto.Categoria;
            TxtStockActual.Text = producto.StockActual.ToString();
            TxtStockMinimo.Text = producto.StockMinimo.ToString();
            TxtPrecio.Text = producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (!int.TryParse(TxtStockActual.Text, out int stockActual))
        {
            MostrarError("El stock actual debe ser un número entero.");
            return;
        }

        if (!int.TryParse(TxtStockMinimo.Text, out int stockMinimo))
        {
            MostrarError("El stock mínimo debe ser un número entero.");
            return;
        }

        if (!decimal.TryParse(TxtPrecio.Text, NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal precio))
        {
            MostrarError("El precio debe ser un número (use punto para decimales).");
            return;
        }

        _producto.Nombre = TxtNombre.Text;
        _producto.Categoria = TxtCategoria.Text;
        _producto.StockActual = stockActual;
        _producto.StockMinimo = stockMinimo;
        _producto.PrecioUnitario = precio;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioInventario(db).Guardar(_producto);
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