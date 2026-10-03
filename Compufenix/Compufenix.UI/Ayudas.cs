using System;
using System.Windows;

namespace Compufenix.UI;

public static class Ayudas
{
    // Centra una ventana exactamente en el centro de la pantalla,
    // sin importar su alto o ancho final (funciona incluso con SizeToContent)
    public static void CentrarEnPantalla(this Window ventana)
    {
        void Centrar(object? sender, EventArgs e)
        {
            var area = SystemParameters.WorkArea;
            ventana.Left = area.Left + (area.Width - ventana.ActualWidth) / 2;
            ventana.Top = area.Top + (area.Height - ventana.ActualHeight) / 2;
            ventana.ContentRendered -= Centrar;
        }

        ventana.ContentRendered += Centrar;
    }
}