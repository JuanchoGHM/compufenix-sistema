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
}