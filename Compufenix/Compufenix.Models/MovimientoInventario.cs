using System.ComponentModel.DataAnnotations;

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

    public Producto? Producto { get; set; }
    public Ticket? Ticket { get; set; }
}