using System.Windows;
using Compufenix.Business;

namespace Compufenix.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);


        // Fuerza el formato de moneda y números de Costa Rica en toda la app
        var cultura = new System.Globalization.CultureInfo("es-CR");
        System.Threading.Thread.CurrentThread.CurrentCulture = cultura;
        System.Threading.Thread.CurrentThread.CurrentUICulture = cultura;
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultura;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultura;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                System.Windows.Markup.XmlLanguage.GetLanguage(cultura.IetfLanguageTag)));

        // Evita que el programa se cierre cuando se cierre la primera ventana
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioUsuarios(db);

            // Si no hay usuarios, se pide crear el administrador
            if (!servicio.ExisteAlgunUsuario())
            {
                var registro = new RegistroAdminWindow();
                if (registro.ShowDialog() != true)
                {
                    Shutdown();
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo conectar con la base de datos:\n" + ex.Message);
            Shutdown();
            return;
        }

        // Pide iniciar sesión
        var login = new LoginWindow();
        if (login.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        // Abre la ventana principal
        var ventanaPrincipal = new MainWindow();
        MainWindow = ventanaPrincipal;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        ventanaPrincipal.Show();
    }
}