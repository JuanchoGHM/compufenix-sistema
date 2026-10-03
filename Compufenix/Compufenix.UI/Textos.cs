using Compufenix.Models;

namespace Compufenix.UI;

public static class Textos
{
    public static string Mostrar(EstadoTicket estado) => estado switch
    {
        EstadoTicket.Recibido => "Recibido",
        EstadoTicket.EnDiagnostico => "En diagnóstico",
        EstadoTicket.EsperandoRepuesto => "Esperando repuesto",
        EstadoTicket.EnReparacion => "En reparación",
        EstadoTicket.Reparado => "Reparado",
        EstadoTicket.Entregado => "Entregado",
        EstadoTicket.Cancelado => "Cancelado",
        _ => estado.ToString()
    };
}