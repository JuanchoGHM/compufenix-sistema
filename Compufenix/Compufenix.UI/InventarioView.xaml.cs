using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class InventarioView : UserControl
{
    private readonly Paginador<Producto> _paginador = new();

    private readonly bool _soloStockBajo;

    public InventarioView(bool soloStockBajo = false)
    {
        InitializeComponent();
        _soloStockBajo = soloStockBajo;
        CargarProductos();
    }

    private void CargarProductos(string? texto = null)
    {
        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioInventario(db);
            var productos = servicio.Buscar(texto);

            if (_soloStockBajo)
                productos = productos.Where(p => p.EstadoAlerta).ToList();

            _paginador.Cargar(productos);
            MostrarPaginaActual();
        }
        catch
        {
            // Si falla, la tabla se queda vacía por ahora
        }
    }
    private void MostrarPaginaActual()
    {
        TablaProductos.ItemsSource = _paginador.ObtenerPaginaActual();
        TxtPagina.Text = _paginador.TextoPagina;
        BtnPaginaAnterior.IsEnabled = _paginador.PaginaActual > 1;
        BtnPaginaSiguiente.IsEnabled = _paginador.PaginaActual < _paginador.TotalPaginas;
    }

    private void PaginaAnterior_Click(object sender, RoutedEventArgs e)
    {
        if (_paginador.Anterior()) MostrarPaginaActual();
    }

    private void PaginaSiguiente_Click(object sender, RoutedEventArgs e)
    {
        if (_paginador.Siguiente()) MostrarPaginaActual();
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

    private void Movimiento_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var producto = (Producto)boton.DataContext;

        var ventana = new MovimientoWindow(producto) { Owner = Window.GetWindow(this) };
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