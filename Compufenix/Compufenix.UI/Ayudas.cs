using System;
using System.Windows;

namespace Compufenix.UI;

public static class Ayudas
{
    // Centra una ventana exactamente en el centro de la pantalla,
    // y la vuelve a centrar cada vez que cambia de tamaño
    // (por ejemplo, cuando muestra más o menos contenido).
    public static void CentrarEnPantalla(this Window ventana)
    {
        ventana.Opacity = 0;

        void Centrar()
        {
            var area = SystemParameters.WorkArea;
            ventana.Left = area.Left + (area.Width - ventana.ActualWidth) / 2;
            ventana.Top = area.Top + (area.Height - ventana.ActualHeight) / 2;
        }

        void AlRenderizar(object? sender, EventArgs e)
        {
            Centrar();
            ventana.Opacity = 1;
            ventana.ContentRendered -= AlRenderizar;
        }

        ventana.ContentRendered += AlRenderizar;
        ventana.SizeChanged += (s, e) => Centrar();
    }
}