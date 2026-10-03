using System.ComponentModel.DataAnnotations;

namespace Compufenix.Models;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;
}