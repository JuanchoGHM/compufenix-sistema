using System.Windows;

namespace Compufenix.UI;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    private void Confirmar_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    // Método de ayuda: muestra el diálogo y devuelve true si la persona confirmó
    public static bool Mostrar(Window dueño, string titulo, string mensaje, string textoBoton = "Eliminar")
    {
        var dialogo = new ConfirmDialog { Owner = dueño };
        dialogo.TxtTitulo.Text = titulo;
        dialogo.TxtMensaje.Text = mensaje;
        dialogo.BtnConfirmar.Content = textoBoton;
        return dialogo.ShowDialog() == true;
    }
}