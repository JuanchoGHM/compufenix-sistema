using System.Windows;
using Compufenix.Data;
using Microsoft.Extensions.Configuration;

namespace Compufenix.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ProbarConexion_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 1. Lee el archivo con la cadena de conexión
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.local.json", optional: false)
                .Build();

            var cadena = config.GetConnectionString("Compufenix")!;

            // 2. Crea el "encargado" y le pide contar los productos
            using var db = FabricaDbContext.Crear(cadena);
            int total = db.Productos.Count();

            // 3. Muestra el resultado
            MessageBox.Show($"¡Conexión exitosa! Productos en la base de datos: {total}");
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo conectar:\n" + ex.Message);
        }
    }
}