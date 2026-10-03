using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class ClientesView : UserControl
{
    private readonly Paginador<Cliente> _paginador = new();
    public ClientesView()
    {
        InitializeComponent();
        CargarClientes();
    }

    private void CargarClientes(string? texto = null)
    {
        try
        {
            using var db = Configuracion.CrearDb();
            var clientes = new ServicioClientes(db).Buscar(texto);
            _paginador.Cargar(clientes);
            MostrarPaginaActual();
        }
        catch
        {
            // La tabla se queda vacía si falla
        }
    }

    private void MostrarPaginaActual()
    {
        TablaClientes.ItemsSource = _paginador.ObtenerPaginaActual();
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
        CargarClientes(TxtBuscar.Text);
    }

    private void VerEquipos_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var cliente = (Cliente)boton.DataContext;

        var ventana = new FichaClienteWindow(cliente) { Owner = Window.GetWindow(this) };
        ventana.ShowDialog();
    }


    private void NuevoCliente_Click(object sender, RoutedEventArgs e)
    {
        var ventana = new ClienteWindow { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarClientes(TxtBuscar.Text);
        }
    }

    private void Editar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var cliente = (Cliente)boton.DataContext;

        var ventana = new ClienteWindow(cliente) { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarClientes(TxtBuscar.Text);
        }
    }

    private void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var cliente = (Cliente)boton.DataContext;

        bool confirmado = ConfirmDialog.Mostrar(
            Window.GetWindow(this),
            "Eliminar cliente",
            $"¿Seguro que deseas eliminar a \"{cliente.Nombre}\"? Esta acción no se puede deshacer.",
            "Eliminar");

        if (!confirmado) return;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioClientes(db).Eliminar(cliente.IdCliente);
            CargarClientes(TxtBuscar.Text);
        }
        catch (InvalidOperationException ex)
        {
            // Mensajes propios, pensados para el usuario
            AvisoDialog.Mostrar(Window.GetWindow(this), "No se puede eliminar", ex.Message, esError: true);
        }
        catch (Exception ex)
        {
            // Errores inesperados
            var mensaje = ex.InnerException?.Message ?? ex.Message;
            AvisoDialog.Mostrar(Window.GetWindow(this), "No se pudo eliminar", mensaje, esError: true);
        }
    }
}