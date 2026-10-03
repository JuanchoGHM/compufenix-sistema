using System.Windows;
using System.Windows.Controls;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class UsuariosView : UserControl
{
    public UsuariosView()
    {
        InitializeComponent();
        CargarUsuarios();
    }

    private void CargarUsuarios()
    {
        using var db = Configuracion.CrearDb();
        TablaUsuarios.ItemsSource = new ServicioUsuarios(db).ObtenerTodos();
    }

    private void NuevoUsuario_Click(object sender, RoutedEventArgs e)
    {
        var ventana = new UsuarioWindow { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarUsuarios();
        }
    }

    private void CambiarActivo_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var usuario = (Usuario)boton.DataContext;

        bool nuevoEstado = !usuario.Activo;
        string accion = nuevoEstado ? "activar" : "desactivar";

        bool confirmado = ConfirmDialog.Mostrar(
            Window.GetWindow(this),
            $"{(nuevoEstado ? "Activar" : "Desactivar")} usuario",
            $"¿Seguro que deseas {accion} a \"{usuario.Nombre}\"?",
            nuevoEstado ? "Activar" : "Desactivar");

        if (!confirmado) return;

        using var db = Configuracion.CrearDb();
        new ServicioUsuarios(db).CambiarActivo(usuario.IdUsuario, nuevoEstado);
        CargarUsuarios();
    }
}