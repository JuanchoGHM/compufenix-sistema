using Compufenix.Data;
using Compufenix.Models;
using Microsoft.EntityFrameworkCore;

namespace Compufenix.Business;

public class ServicioTickets
{
    private readonly CompufenixDbContext _db;

    public ServicioTickets(CompufenixDbContext db)
    {
        _db = db;
    }

    // Lista de tickets con el equipo y el cliente ya cargados, el más reciente primero
    public List<Ticket> Buscar(string? texto = null)
    {
        var consulta = _db.Tickets
            .Include(t => t.Equipo!).ThenInclude(e => e.Cliente)
            .Include(t => t.Tecnico)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(tk =>
                tk.Equipo!.Cliente!.Nombre.Contains(t) ||
                tk.Equipo.Tipo.Contains(t));
        }

        return consulta.OrderByDescending(t => t.FechaIngreso).ToList();
    }

    public void Crear(Ticket ticket)
    {
        if (ticket.IdEquipo == 0)
            throw new ArgumentException("Debe seleccionar un equipo.");

        ticket.Estado = EstadoTicket.Recibido;
        ticket.FechaIngreso = DateTime.Now;

        _db.Tickets.Add(ticket);
        _db.SaveChanges();
    }


    // Cambia el estado de un ticket
    public void CambiarEstado(int idTicket, EstadoTicket nuevoEstado)
    {
        var ticket = _db.Tickets.Find(idTicket)
            ?? throw new InvalidOperationException("El ticket ya no existe.");

        ticket.Estado = nuevoEstado;
        _db.SaveChanges();
    }

    // Usa un repuesto en el ticket: descuenta el stock y suma el costo al ticket
    public void UsarRepuesto(int idTicket, int idProducto, int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.");

        var ticket = _db.Tickets.Find(idTicket)
            ?? throw new InvalidOperationException("El ticket ya no existe.");

        var producto = _db.Productos.Find(idProducto)
            ?? throw new InvalidOperationException("El producto ya no existe.");

        if (cantidad > producto.StockActual)
            throw new InvalidOperationException(
                $"No hay suficiente stock de \"{producto.Nombre}\". Disponible: {producto.StockActual}.");

        // 1. Descuenta el stock del producto
        producto.StockActual -= cantidad;

        // 2. Registra el movimiento de inventario, enlazado a este ticket
        _db.MovimientosInventario.Add(new MovimientoInventario
        {
            IdProducto = idProducto,
            IdTicket = idTicket,
            Tipo = TipoMovimiento.Salida,
            Cantidad = cantidad,
            Fecha = DateTime.Now
        });

        // 3. Suma el costo del repuesto al total del ticket
        ticket.CostoTotal += producto.PrecioUnitario * cantidad;

        // Las tres operaciones se guardan juntas
        _db.SaveChanges();
    }

    // Repuestos usados en un ticket, con el nombre del producto ya cargado
    public List<MovimientoInventario> ObtenerRepuestosUsados(int idTicket)
    {
        return _db.MovimientosInventario
            .Include(m => m.Producto)
            .Where(m => m.IdTicket == idTicket)
            .OrderByDescending(m => m.Fecha)
            .ToList();
    }

    public Ticket ObtenerPorId(int idTicket)
    {
        return _db.Tickets
            .Include(t => t.Equipo!).ThenInclude(e => e.Cliente)
            .Include(t => t.Tecnico)
            .First(t => t.IdTicket == idTicket);
    }

}