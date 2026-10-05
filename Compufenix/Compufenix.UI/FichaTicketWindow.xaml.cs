using System.Globalization;
using System.Windows;
using Compufenix.Business;
using Compufenix.Models;

namespace Compufenix.UI;

public partial class FichaTicketWindow : Window
{
    private int _idTicket;
    private int? _idMovimientoEditando; // null = agregando un repuesto nuevo

    private class OpcionProducto
    {
        public int IdProducto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    private class FilaRepuesto
    {
        public int IdMovimiento { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string FechaTexto { get; set; } = string.Empty;
    }

    public FichaTicketWindow(int idTicket)
    {
        InitializeComponent();
        _idTicket = idTicket;

        // Nunca más alta que la pantalla: si el contenido no cabe, se desplaza
        MaxHeight = SystemParameters.WorkArea.Height - 20;
        this.CentrarEnPantalla();

        CmbEstado.ItemsSource = Enum.GetValues<EstadoTicket>();

        CargarTodo();
    }

    private void CargarTodo()
    {
        using var db = Configuracion.CrearDb();
        var servicio = new ServicioTickets(db);
        var ticket = servicio.ObtenerPorId(_idTicket);

        TxtTitulo.Text = $"Ticket #{ticket.IdTicket} — {ticket.Equipo!.Cliente!.Nombre}";
        TxtSubtitulo.Text = $"{ticket.Equipo.Tipo} {ticket.Equipo.Marca} · " +
            $"Técnico: {(ticket.Tecnico != null ? ticket.Tecnico.Nombre : "Sin asignar")}";
        CmbEstado.SelectedItem = ticket.Estado;
        TxtManoObra.Text = ticket.CostoManoObra.ToString("0.00", CultureInfo.CurrentCulture);
        TxtCostoRepuestos.Text = (ticket.CostoTotal - ticket.CostoManoObra).ToString("C");
        TxtCostoManoObra.Text = ticket.CostoManoObra.ToString("C");
        TxtCostoTotal.Text = ticket.CostoTotal.ToString("C");

        var productos = new ServicioInventario(db).Buscar();
        CmbProducto.ItemsSource = productos.Select(p => new OpcionProducto
        {
            IdProducto = p.IdProducto,
            Descripcion = $"{p.Nombre} (Stock: {p.StockActual})"
        }).ToList();

        var repuestos = servicio.ObtenerRepuestosUsados(_idTicket);
        var filas = repuestos.Select(m => new FilaRepuesto
        {
            IdMovimiento = m.IdMovimiento,
            IdProducto = m.IdProducto,
            Cantidad = m.Cantidad,
            NombreProducto = m.Producto!.Nombre,
            Descripcion = $"{m.Cantidad} × {m.Producto!.Nombre}",
            FechaTexto = m.Fecha.ToString("dd/MM/yyyy HH:mm")
        }).ToList();

        ListaRepuestos.ItemsSource = filas;
        TxtSinRepuestos.Visibility = filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void GuardarEstado_Click(object sender, RoutedEventArgs e)
    {
        if (CmbEstado.SelectedItem is not EstadoTicket estado) return;

        using var db = Configuracion.CrearDb();
        new ServicioTickets(db).CambiarEstado(_idTicket, estado, Sesion.UsuarioActual?.IdUsuario);

        TxtEstadoGuardado.Visibility = Visibility.Visible;

        var temporizador = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        temporizador.Tick += (s, args) =>
        {
            TxtEstadoGuardado.Visibility = Visibility.Collapsed;
            temporizador.Stop();
        };
        temporizador.Start();
    }

    private void GuardarManoObra_Click(object sender, RoutedEventArgs e)
    {
        TxtManoObraMensaje.Visibility = Visibility.Collapsed;

        if (!decimal.TryParse(TxtManoObra.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal monto))
        {
            MostrarMensajeManoObra("El monto debe ser un número válido.", "#DC2626");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            new ServicioTickets(db).GuardarManoDeObra(_idTicket, monto);

            CargarTodo(); // refresca el desglose y el costo total
            MostrarMensajeManoObra("✓ Mano de obra guardada correctamente", "#16A34A");
        }
        catch (Exception ex)
        {
            MostrarMensajeManoObra(ex.Message, "#DC2626");
        }
    }

    private void MostrarMensajeManoObra(string mensaje, string color)
    {
        TxtManoObraMensaje.Text = mensaje;
        TxtManoObraMensaje.Foreground = (System.Windows.Media.Brush)
            new System.Windows.Media.BrushConverter().ConvertFromString(color)!;
        TxtManoObraMensaje.Visibility = Visibility.Visible;
    }

    private void UsarRepuesto_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;

        if (CmbProducto.SelectedItem is not OpcionProducto producto)
        {
            MostrarError("Debes seleccionar un producto.");
            return;
        }

        if (!int.TryParse(TxtCantidad.Text, out int cantidad))
        {
            MostrarError("La cantidad debe ser un número entero.");
            return;
        }

        try
        {
            using var db = Configuracion.CrearDb();
            var servicio = new ServicioTickets(db);

            if (_idMovimientoEditando is int idMovimiento)
                servicio.EditarRepuesto(idMovimiento, producto.IdProducto, cantidad);
            else
                servicio.UsarRepuesto(_idTicket, producto.IdProducto, cantidad);

            SalirModoEdicion();
            CargarTodo(); // refresca costo, historial y stock disponible en el ComboBox
        }
        catch (Exception ex)
        {
            MostrarError(ex.Message);
        }
    }

    private void EditarRepuesto_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not FilaRepuesto fila) return;

        PanelError.Visibility = Visibility.Collapsed;
        _idMovimientoEditando = fila.IdMovimiento;

        CmbProducto.SelectedItem = (CmbProducto.ItemsSource as IEnumerable<OpcionProducto>)?
            .FirstOrDefault(p => p.IdProducto == fila.IdProducto);
        TxtCantidad.Text = fila.Cantidad.ToString();

        TxtTituloRepuesto.Text = "Editar repuesto";
        BtnGuardarRepuesto.Content = "Guardar cambios";
        BtnCancelarEdicion.Visibility = Visibility.Visible;
        TxtAyudaEdicion.Visibility = Visibility.Visible;
    }

    private void CancelarEdicion_Click(object sender, RoutedEventArgs e)
    {
        PanelError.Visibility = Visibility.Collapsed;
        SalirModoEdicion();
    }

    private void EliminarRepuesto_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not FilaRepuesto fila) return;

        bool confirmado = ConfirmDialog.Mostrar(
            this, "Quitar repuesto",
            $"Se quitará {fila.Cantidad} × {fila.NombreProducto} del ticket, " +
            "se devolverán las unidades al inventario y se restará su costo del total.",
            "Quitar");

        if (!confirmado) return;

        try
        {
            PanelError.Visibility = Visibility.Collapsed;

            using var db = Configuracion.CrearDb();
            new ServicioTickets(db).EliminarRepuesto(fila.IdMovimiento);

            if (_idMovimientoEditando == fila.IdMovimiento)
                SalirModoEdicion();

            CargarTodo();
        }
        catch (Exception ex)
        {
            MostrarError(ex.Message);
        }
    }

    // Vuelve el formulario al modo "agregar repuesto"
    private void SalirModoEdicion()
    {
        _idMovimientoEditando = null;
        CmbProducto.SelectedItem = null;
        TxtCantidad.Clear();
        TxtTituloRepuesto.Text = "Usar un repuesto";
        BtnGuardarRepuesto.Content = "+ Usar repuesto";
        BtnCancelarEdicion.Visibility = Visibility.Collapsed;
        TxtAyudaEdicion.Visibility = Visibility.Collapsed;
    }

    private void MostrarError(string mensaje)
    {
        TxtError.Text = mensaje;
        PanelError.Visibility = Visibility.Visible;
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}