using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compufenix.Models;

public class Ticket
{
    [Key]
    public int IdTicket { get; set; }
    public int IdEquipo { get; set; }
    public int? IdTecnico { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public EstadoTicket Estado { get; set; } = EstadoTicket.Recibido;
    public string? Diagnostico { get; set; }
    public decimal CostoTotal { get; set; }

    public Equipo? Equipo { get; set; }

    // El técnico es un Usuario; esta línea le explica a Entity Framework
    // que IdTecnico apunta a la tabla de usuarios
    [ForeignKey("IdTecnico")]
    public Usuario? Tecnico { get; set; }
}