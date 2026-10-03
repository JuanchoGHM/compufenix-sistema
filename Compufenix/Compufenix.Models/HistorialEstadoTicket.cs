using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compufenix.Models;

public class HistorialEstadoTicket
{
    [Key]
    public int IdHistorial { get; set; }
    public int IdTicket { get; set; }
    public EstadoTicket Estado { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int? IdUsuario { get; set; }

    [ForeignKey("IdTicket")]
    public Ticket? Ticket { get; set; }

    [ForeignKey("IdUsuario")]
    public Usuario? Usuario { get; set; }
}