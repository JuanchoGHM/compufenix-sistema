using Compufenix.Data;
using Compufenix.Models;

namespace Compufenix.Business;

// Un "paquetito" con los 4 números que muestra el Inicio
public record ResumenInicio(int Productos, int StockBajo, int TicketsAbiertos, int Clientes);

public class ServicioResumen
{
    private readonly CompufenixDbContext _db;

    public ServicioResumen(CompufenixDbContext db)
    {
        _db = db;
    }

    public ResumenInicio Obtener()
    {
        return new ResumenInicio(
            _db.Productos.Count(),
            _db.Productos.Count(p => p.StockActual < p.StockMinimo),
            _db.Tickets.Count(t => t.Estado != EstadoTicket.Entregado
                                && t.Estado != EstadoTicket.Cancelado),
            _db.Clientes.Count());
    }
}