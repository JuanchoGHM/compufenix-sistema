using Microsoft.EntityFrameworkCore;

namespace Compufenix.Data;

public static class FabricaDbContext
{
    public static CompufenixDbContext Crear(string cadenaConexion)
    {
        var opciones = new DbContextOptionsBuilder<CompufenixDbContext>()
            .UseMySql(cadenaConexion, ServerVersion.AutoDetect(cadenaConexion))
            .UseSnakeCaseNamingConvention() // IdCliente en C# = id_cliente en MySQL
            .Options;

        return new CompufenixDbContext(opciones);
    }
}