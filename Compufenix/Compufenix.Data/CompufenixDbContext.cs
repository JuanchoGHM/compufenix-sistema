using Compufenix.Models;
using Microsoft.EntityFrameworkCore;

namespace Compufenix.Data;

public class CompufenixDbContext : DbContext
{
    public CompufenixDbContext(DbContextOptions<CompufenixDbContext> opciones)
        : base(opciones)
    {
    }

    // Un "cajón" por cada tabla de la base de datos
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Los enums se guardan como texto, igual que los ENUM de MySQL
        mb.Entity<Usuario>().Property(u => u.Rol).HasConversion<string>();
        mb.Entity<Ticket>().Property(t => t.Estado).HasConversion<string>();
        mb.Entity<MovimientoInventario>().Property(m => m.Tipo).HasConversion<string>();
    }
}