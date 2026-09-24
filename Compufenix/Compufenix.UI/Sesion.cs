using Compufenix.Models;

namespace Compufenix.UI;

public static class Sesion
{
    // El usuario que inició sesión (null si nadie ha entrado)
    public static Usuario? UsuarioActual { get; set; }

    public static bool EsAdministrador =>
        UsuarioActual?.Rol == RolUsuario.Administrador;
}