using Compufenix.Data;
using Compufenix.Models;
using Microsoft.EntityFrameworkCore;

namespace Compufenix.Business;

public class ServicioEquipos
{
    private readonly CompufenixDbContext _db;

    public ServicioEquipos(CompufenixDbContext db)
    {
        _db = db;
    }

    // Equipos de un cliente en particular, el más reciente primero
    public List<Equipo> ObtenerDeCliente(int idCliente)
    {
        return _db.Equipos
            .Where(e => e.IdCliente == idCliente)
            .OrderByDescending(e => e.IdEquipo)
            .ToList();
    }


    // Todos los equipos con su cliente ya cargado (para elegir uno al crear un ticket)
    public List<Equipo> ObtenerTodosConCliente()
    {
        return _db.Equipos
            .Include(e => e.Cliente)
            .OrderBy(e => e.Cliente!.Nombre)
            .ToList();
    }

    public void Registrar(Equipo equipo)
    {
        if (string.IsNullOrWhiteSpace(equipo.Tipo))
            throw new ArgumentException("El tipo de equipo es obligatorio (por ejemplo: Laptop, Impresora).");

        equipo.Tipo = equipo.Tipo.Trim();

        _db.Equipos.Add(equipo);
        _db.SaveChanges();
    }

    public void Eliminar(int idEquipo)
    {
        var equipo = _db.Equipos.Find(idEquipo);
        if (equipo == null) return;

        bool tieneTickets = _db.Tickets.Any(t => t.IdEquipo == idEquipo);
        if (tieneTickets)
            throw new InvalidOperationException(
                "No se puede eliminar: este equipo ya tiene tickets de servicio registrados.");

        _db.Equipos.Remove(equipo);
        _db.SaveChanges();
    }
}