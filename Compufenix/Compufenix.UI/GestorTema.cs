using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Compufenix.UI;

public static class GestorTema
{
    private static readonly string RutaPreferencia =
        Path.Combine(AppContext.BaseDirectory, "tema_usuario.txt");

    public static bool EsOscuro { get; private set; }

    private static readonly Dictionary<string, (string Claro, string Oscuro)> Colores = new()
    {
        ["FondoBrush"] = ("#F5F7FB", "#0F172A"),
        ["SuperficieBrush"] = ("#FFFFFF", "#1E293B"),
        ["TextoBrush"] = ("#0F172A", "#F1F5F9"),
        ["TextoSuaveBrush"] = ("#64748B", "#94A3B8"),
        ["BordeBrush"] = ("#E2E8F0", "#334155"),
        ["HoverSuaveBrush"] = ("#F1F5F9", "#334155"),
    };

    public static void CargarPreferenciaGuardada()
    {
        try
        {
            if (File.Exists(RutaPreferencia) && File.ReadAllText(RutaPreferencia).Trim() == "oscuro")
            {
                Aplicar(true);
            }
        }
        catch
        {
            // Si falla la lectura, simplemente arranca en modo claro (normal)
        }
    }

    public static void Alternar()
    {
        Aplicar(!EsOscuro);
        GuardarPreferencia();
    }

    // Reemplaza cada color por un pincel NUEVO (nunca modifica el que ya existe),
    // así nunca choca con que el anterior esté "congelado".
    private static void Aplicar(bool oscuro)
    {
        foreach (var (clave, valores) in Colores)
        {
            var hex = oscuro ? valores.Oscuro : valores.Claro;
            var color = (Color)ColorConverter.ConvertFromString(hex);

            Application.Current.Resources[clave] = new SolidColorBrush(color);
        }

        EsOscuro = oscuro;
    }

    private static void GuardarPreferencia()
    {
        try
        {
            File.WriteAllText(RutaPreferencia, EsOscuro ? "oscuro" : "claro");
        }
        catch
        {
            // Si falla el guardado, no es grave: solo no se recordará la próxima vez
        }
    }
}