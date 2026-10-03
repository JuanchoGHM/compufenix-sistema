using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class InventarioView : UserControl
{
    public InventarioView()
    {
        InitializeComponent();
        CargarProductos();
    }

    private void CargarProductos(string? texto = null)
    {
        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioInventario(db);
            TablaProductos.ItemsSource = servicio.Buscar(texto);
        }
        catch
        {
            // Si falla, la tabla se queda vacía por ahora
        }
    }

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
    {
        CargarProductos(TxtBuscar.Text);
    }

    private void NuevoProducto_Click(object sender, RoutedEventArgs e)
    {
        var ventana = new ProductoWindow { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarProductos(TxtBuscar.Text);
        }
    }

    private void Editar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var producto = (Producto)boton.DataContext;

        var ventana = new ProductoWindow(producto) { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarProductos(TxtBuscar.Text);
        }
    }

    private void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var producto = (Producto)boton.DataContext;

        bool confirmado = ConfirmDialog.Mostrar(
            Window.GetWindow(this),
            "Eliminar producto",
            $"¿Seguro que deseas eliminar \"{producto.Nombre}\"? Esta acción no se puede deshacer.",
            "Eliminar");

        if (!confirmado) return;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioInventario(db).Eliminar(producto.IdProducto);
            CargarProductos(TxtBuscar.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}