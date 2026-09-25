using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class ClientesView : UserControl
{
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
            TablaClientes.ItemsSource = new ServicioClientes(db).Buscar(texto);
        }
        catch
        {
            // La tabla se queda vacía si falla
        }
    }

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
    {
        CargarClientes(TxtBuscar.Text);
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
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}