using Compufenix.Data;
using Compufenix.Models;
using Microsoft.EntityFrameworkCore;

namespace Compufenix.Business;

// Una fila del reporte de tickets por estado
public record FilaEstadoTicket(EstadoTicket Estado, int Cantidad);
public record FilaTicketAntiguo(int IdTicket, string Cliente, string Equipo,
    EstadoTicket Estado, DateTime FechaIngreso, int DiasAbierto);
public record FilaClienteTop(string Cliente, int CantidadTickets);

public record FilaTicketMes(string Mes, int Cantidad);

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

    // Los tickets abiertos (no entregados ni cancelados) que llevan más tiempo esperando
    public List<FilaTicketAntiguo> TicketsMasAntiguosSinResolver(int cantidad = 5)
    {
        var abiertos = _db.Tickets
            .Include(t => t.Equipo!).ThenInclude(e => e.Cliente)
            .Where(t => t.Estado != EstadoTicket.Entregado && t.Estado != EstadoTicket.Cancelado)
            .OrderBy(t => t.FechaIngreso)
            .Take(cantidad)
            .ToList();

        return abiertos.Select(t => new FilaTicketAntiguo(
            t.IdTicket,
            t.Equipo!.Cliente!.Nombre,
            t.Equipo.Tipo,
            t.Estado,
            t.FechaIngreso,
            (int)(DateTime.Now - t.FechaIngreso).TotalDays
        )).ToList();
    }

    // Los clientes con más tickets registrados (histórico)
    public List<FilaClienteTop> TopClientesPorTickets(int cantidad = 5)
    {
        // Primero se traen solo los nombres (esto sí lo traduce MySQL)
        var nombres = _db.Tickets
            .Include(t => t.Equipo!).ThenInclude(e => e.Cliente)
            .Select(t => t.Equipo!.Cliente!.Nombre)
            .ToList();

        // Y ya en memoria se agrupa y se cuenta
        return nombres
            .GroupBy(nombre => nombre)
            .Select(g => new FilaClienteTop(g.Key, g.Count()))
            .OrderByDescending(f => f.CantidadTickets)
            .Take(cantidad)
            .ToList();
    }

    // Cuántos tickets se recibieron en cada uno de los últimos meses (tendencia de atención)
    public List<FilaTicketMes> TicketsPorMes(int meses = 6)
    {
        var desde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-(meses - 1));

        // Se traen solo las fechas (esto sí lo traduce MySQL)
        var fechas = _db.Tickets
            .Where(t => t.FechaIngreso >= desde)
            .Select(t => t.FechaIngreso)
            .ToList();

        // Y ya en memoria se cuenta mes por mes
        var resultado = new List<FilaTicketMes>();
        for (int i = 0; i < meses; i++)
        {
            var mes = desde.AddMonths(i);
            int cantidad = fechas.Count(f => f.Year == mes.Year && f.Month == mes.Month);
            resultado.Add(new FilaTicketMes(mes.ToString("MMM yyyy"), cantidad));
        }

        return resultado;
    }
}