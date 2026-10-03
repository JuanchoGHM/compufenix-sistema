using System.Globalization;
using System.Windows.Data;
using Compufenix.Models;

namespace Compufenix.UI;

public class ConvertidorEstadoTicket : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is EstadoTicket estado ? Textos.Mostrar(estado) : value ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}