using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class ClienteWindow : Window
{
    private readonly Cliente _cliente;

    public ClienteWindow(Cliente? cliente = null)
    {
        InitializeComponent();

        _cliente = cliente ?? new Cliente();

        if (cliente != null)
        {
            TxtTitulo.Text = "Editar cliente";
            TxtIcono.Text = "\uE70F";
            TxtNombre.Text = cliente.Nombre;
            TxtTelefono.Text = cliente.Telefono;
            TxtCorreo.Text = cliente.Correo;
            TxtDireccion.Text = cliente.Direccion;
        }
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        _cliente.Nombre = TxtNombre.Text;
        _cliente.Telefono = TxtTelefono.Text;
        _cliente.Correo = TxtCorreo.Text;
        _cliente.Direccion = TxtDireccion.Text;

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioClientes(db).Guardar(_cliente);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            TxtError.Text = ex.Message;
            PanelError.Visibility = Visibility.Visible;
        }
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}