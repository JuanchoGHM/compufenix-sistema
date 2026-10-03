using Compufenix.Data;
using Microsoft.Extensions.Configuration;

namespace Compufenix.UI;

public static class Configuracion
{
    // Crea la conexión a la base de datos leyendo appsettings.local.json
    public static CompufenixDbContext CrearDb()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.local.json", optional: false)
            .Build();

        var cadena = config.GetConnectionString("Compufenix")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Compufenix'.");

        return FabricaDbContext.Crear(cadena);
    }
}