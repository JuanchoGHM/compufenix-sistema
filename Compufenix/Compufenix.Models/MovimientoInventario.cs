using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compufenix.Models;

public class MovimientoInventario
{
    [Key]
    public int IdMovimiento { get; set; }
    public int IdProducto { get; set; }
    public int? IdTicket { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;

    [ForeignKey("IdProducto")]
    public Producto? Producto { get; set; }

    [ForeignKey("IdTicket")]
    public Ticket? Ticket { get; set; }
}