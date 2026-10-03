using System.Windows;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var usuario = Sesion.UsuarioActual;
        if (usuario != null)
        {
            TxtBienvenida.Text = $"Hola, {usuario.Nombre}";
            TxtNombreUsuario.Text = usuario.Nombre;
            TxtRolUsuario.Text = usuario.Rol == RolUsuario.Administrador
                ? "Administrador"
                : "Técnico";
        }

        // Estas opciones son solo para el administrador
        var visibilidadAdmin = Sesion.EsAdministrador
            ? Visibility.Visible
            : Visibility.Collapsed;

        BtnClientes.Visibility = visibilidadAdmin;
        BtnReportes.Visibility = visibilidadAdmin;
        BtnUsuarios.Visibility = visibilidadAdmin;
    }

    private void CerrarSesion_Click(object sender, RoutedEventArgs e)
    {
        Sesion.UsuarioActual = null;
        Hide();

        var login = new LoginWindow();
        if (login.ShowDialog() == true)
        {
            // Abre una ventana principal nueva con el usuario que entró
            var nueva = new MainWindow();
            Application.Current.MainWindow = nueva;
            nueva.Show();
        }
        else
        {
            Application.Current.Shutdown();
        }

        Close();
    }
}