using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compufenix.Models;

public class Equipo
{
    [Key]
    public int IdEquipo { get; set; }
    public int IdCliente { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? NumeroSerie { get; set; }

    [ForeignKey("IdCliente")]
    public Cliente? Cliente { get; set; }
}