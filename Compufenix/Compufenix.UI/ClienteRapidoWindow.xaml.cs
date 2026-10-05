using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

// Crea un cliente junto con su primer equipo, para poder abrir su ticket sin salir de "Nuevo ticket"
public partial class ClienteRapidoWindow : Window
{
    // Id del equipo recién creado (para dejarlo seleccionado en el ticket)
    public int IdEquipoCreado { get; private set; }

    public ClienteRapidoWindow()
    {
        InitializeComponent();
        MaxHeight = SystemParameters.WorkArea.Height - 20; // nunca más alta que la pantalla
        this.CentrarEnPantalla();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        var cliente = new Cliente
        {
            Nombre = TxtNombre.Text,
            Telefono = TxtTelefono.Text,
            Correo = TxtCorreo.Text,
            Direccion = TxtDireccion.Text
        };

        var equipo = new Equipo
        {
            Tipo = TxtTipo.Text,
            Marca = TxtMarca.Text,
            Modelo = TxtModelo.Text,
            NumeroSerie = TxtNumeroSerie.Text
        };

        try
        {
            using var db = Configuracion.CrearDb();
            var creado = new ServicioClientes(db).GuardarConEquipo(cliente, equipo);

            IdEquipoCreado = creado.IdEquipo;
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