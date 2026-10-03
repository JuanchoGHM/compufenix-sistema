using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class UsuarioWindow : Window
{
    private readonly Usuario? _usuarioExistente;

    // Sin parámetros: usuario nuevo. Con un usuario: modo edición.
    public UsuarioWindow(Usuario? usuario = null)
    {
        InitializeComponent();
        this.CentrarEnPantalla();

        _usuarioExistente = usuario;
        CmbRol.ItemsSource = Enum.GetValues<RolUsuario>();

        if (usuario != null)
        {
            TxtTitulo.Text = "Editar usuario";
            TxtSubtitulo.Text = "Actualiza los datos del usuario.";
            TxtIcono.Text = "\uE70F";
            TxtNombre.Text = usuario.Nombre;
            TxtCorreo.Text = usuario.Correo;
            CmbRol.SelectedItem = usuario.Rol;
            TxtEtiquetaContrasena.Text = "Nueva contraseña (déjala vacía para no cambiarla)";
            BtnGuardar.Content = "Guardar cambios";
        }
        else
        {
            CmbRol.SelectedItem = RolUsuario.Tecnico;
        }
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (CmbRol.SelectedItem is not RolUsuario rol)
        {
            MostrarError("Debes seleccionar un rol.");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioUsuarios(db);

            if (_usuarioExistente == null)
            {
                servicio.CrearUsuario(TxtNombre.Text, TxtCorreo.Text, TxtContrasena.Password, rol);
            }
            else
            {
                string? nuevaContrasena = string.IsNullOrEmpty(TxtContrasena.Password)
                    ? null
                    : TxtContrasena.Password;

                servicio.Editar(_usuarioExistente.IdUsuario, TxtNombre.Text, TxtCorreo.Text, rol, nuevaContrasena);
            }

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