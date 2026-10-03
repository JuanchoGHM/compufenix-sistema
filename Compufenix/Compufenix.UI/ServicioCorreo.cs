using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Compufenix.UI;

public static class ServicioCorreo
{
    public static void EnviarCodigoRecuperacion(string correoDestino, string codigo)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.local.json", optional: false)
            .Build();

        var host = config["Smtp:Host"]
            ?? throw new InvalidOperationException("Falta configurar el correo (Smtp:Host) en appsettings.local.json.");
        var puerto = int.Parse(config["Smtp:Port"] ?? "587");
        var usuario = config["Smtp:Usuario"]
            ?? throw new InvalidOperationException("Falta configurar el correo (Smtp:Usuario).");
        var contrasena = config["Smtp:Contrasena"]
            ?? throw new InvalidOperationException("Falta configurar el correo (Smtp:Contrasena).");
        var remitente = config["Smtp:Remitente"] ?? usuario;

        var mensaje = new MimeMessage();
        mensaje.From.Add(MailboxAddress.Parse(remitente));
        mensaje.To.Add(MailboxAddress.Parse(correoDestino));
        mensaje.Subject = "Recuperación de contraseña - COMPUFENIX";
        mensaje.Body = new TextPart("plain")
        {
            Text = $"Tu código de recuperación es: {codigo}\n\n" +
                   "Este código vence en 15 minutos.\n" +
                   "Si no solicitaste este cambio, puedes ignorar este mensaje."
        };

        using var cliente = new SmtpClient();
        cliente.Connect(host, puerto, SecureSocketOptions.StartTls);
        cliente.Authenticate(usuario, contrasena);
        cliente.Send(mensaje);
        cliente.Disconnect(true);
    }
}