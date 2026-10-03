using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Compufenix.UI;

public class MonitorInactividad
{
    private readonly DispatcherTimer _temporizador;
    private readonly Window _ventana;

    public event Action? SesionExpirada;

    public MonitorInactividad(Window ventana, TimeSpan tiempoLimite)
    {
        _ventana = ventana;
        _temporizador = new DispatcherTimer { Interval = tiempoLimite };
        _temporizador.Tick += (s, e) =>
        {
            _temporizador.Stop();
            SesionExpirada?.Invoke();
        };

        // Cualquier actividad del usuario reinicia el conteo
        _ventana.PreviewMouseMove += (s, e) => Reiniciar();
        _ventana.PreviewMouseDown += (s, e) => Reiniciar();
        _ventana.PreviewKeyDown += (s, e) => Reiniciar();

        _temporizador.Start();
    }

    private void Reiniciar()
    {
        _temporizador.Stop();
        _temporizador.Start();
    }

    public void Detener()
    {
        _temporizador.Stop();
    }
}