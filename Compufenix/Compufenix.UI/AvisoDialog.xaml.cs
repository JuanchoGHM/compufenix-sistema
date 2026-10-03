using System.Windows;
using System.Windows.Media;

namespace Compufenix.UI;

public partial class AvisoDialog : Window
{
    public AvisoDialog()
    {
        InitializeComponent();
        this.CentrarEnPantalla();
    }

    private void Aceptar_Click(object sender, RoutedEventArgs e) => Close();

    // Método de ayuda: crea, personaliza y muestra el aviso
    public static void Mostrar(Window dueño, string titulo, string mensaje, bool esError = false)
    {
        var dialogo = new AvisoDialog { Owner = dueño };
        dialogo.TxtTitulo.Text = titulo;
        dialogo.TxtMensaje.Text = mensaje;

        if (esError)
        {
            dialogo.CirculoIcono.Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2));
            dialogo.TxtIcono.Text = "\uE783";
            dialogo.TxtIcono.Foreground = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
        }

        dialogo.ShowDialog();
    }
}