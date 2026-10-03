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

    private void Editar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var usuario = (Usuario)boton.DataContext;

        var ventana = new UsuarioWindow(usuario) { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() == true)
        {
            CargarUsuarios();
        }
    }

    private void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        var boton = (Button)sender;
        var usuario = (Usuario)boton.DataContext;

        bool confirmado = ConfirmDialog.Mostrar(
            Window.GetWindow(this),
            "Eliminar usuario",
            $"¿Seguro que deseas eliminar a \"{usuario.Nombre}\"? Esta acción no se puede deshacer.",
            "Eliminar");

        if (!confirmado) return;

        try
        {
            using var db = Configuracion.CrearDb();
            int idUsuarioActual = Sesion.UsuarioActual!.IdUsuario;
            new ServicioUsuarios(db).Eliminar(usuario.IdUsuario, idUsuarioActual);
            CargarUsuarios();
        }
        catch (Exception ex)
        {
            AvisoDialog.Mostrar(Window.GetWindow(this), "No se pudo eliminar", ex.Message, esError: true);
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