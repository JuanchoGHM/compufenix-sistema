namespace Compufenix.Models;

public enum RolUsuario
{
    Administrador,
    Tecnico
}

public enum EstadoTicket
{
    Recibido,
    EnDiagnostico,
    EsperandoRepuesto,
    EnReparacion,
    Reparado,
    Entregado,
    Cancelado
}

public enum TipoMovimiento
{
    Entrada,
    Salida
}