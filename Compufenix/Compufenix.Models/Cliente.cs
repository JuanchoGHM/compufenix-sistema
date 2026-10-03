using System.ComponentModel.DataAnnotations;

namespace Compufenix.Models;

public class Cliente
{
    [Key]
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }

    // Un cliente puede tener varios equipos
    public List<Equipo> Equipos { get; set; } = new();
}