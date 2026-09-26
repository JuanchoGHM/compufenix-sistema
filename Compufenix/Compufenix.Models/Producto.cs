using System.ComponentModel.DataAnnotations;

namespace Compufenix.Models;

public class Producto
{
    [Key]
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal PrecioUnitario { get; set; }
    public DateTime FechaIncorporacion { get; set; } = DateTime.Now;

    // No se guarda en la base de datos: se calcula solo, para mostrar en pantalla
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool EstadoAlerta => StockActual < StockMinimo;

}