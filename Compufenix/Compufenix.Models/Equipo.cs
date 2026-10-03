using System.ComponentModel.DataAnnotations;

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

    // Acceso directo al dueño del equipo
    public Cliente? Cliente { get; set; }
}