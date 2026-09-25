using Compufenix.Data;
using Compufenix.Models;

namespace Compufenix.Business;

// Una fila del reporte de tickets por estado
public record FilaEstadoTicket(EstadoTicket Estado, int Cantidad);

public class ServicioReportes
{
    private readonly CompufenixDbContext _db;

    public ServicioReportes(CompufenixDbContext db)
    {
        _db = db;
    }

    // Productos con stock bajo, para el reporte de inventario
    public List<Producto> ProductosConStockBajo()
    {
        return _db.Productos
            .Where(p => p.StockActual < p.StockMinimo)
            .OrderBy(p => p.Nombre)
            .ToList();
    }

    public int TotalProductos() => _db.Productos.Count();
    public int TotalUnidadesEnStock() => _db.Productos.Sum(p => p.StockActual);

    // Cuántos tickets hay en cada estado
    public List<FilaEstadoTicket> TicketsPorEstado()
    {
        // Paso 1: se agrupa y cuenta directamente en la base de datos (MySQL sí sabe hacer esto)
        var agrupado = _db.Tickets
            .GroupBy(t => t.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToList();

        // Paso 2: ya con los datos en memoria, se arma la lista de FilaEstadoTicket
        return agrupado
            .Select(g => new FilaEstadoTicket(g.Estado, g.Cantidad))
            .OrderBy(f => f.Estado)
            .ToList();
    }

    // Suma de ingresos entre dos fechas (inclusive)
    public decimal IngresosEntre(DateTime desde, DateTime hasta)
    {
        var hastaFinDelDia = hasta.Date.AddDays(1).AddTicks(-1);

        return _db.Tickets
            .Where(t => t.FechaIngreso >= desde.Date && t.FechaIngreso <= hastaFinDelDia)
            .Sum(t => (decimal?)t.CostoTotal) ?? 0;
    }
}