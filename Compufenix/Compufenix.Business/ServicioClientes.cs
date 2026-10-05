using Compufenix.Data;
using Compufenix.Models;

namespace Compufenix.Business;

public class ServicioClientes
{
    private readonly CompufenixDbContext _db;

    public ServicioClientes(CompufenixDbContext db)
    {
        _db = db;
    }

    public List<Cliente> Buscar(string? texto = null)
    {
        var consulta = _db.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(c => c.Nombre.Contains(t));
        }

        return consulta.OrderBy(c => c.Nombre).ToList();
    }

    public void Guardar(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            throw new ArgumentException("El nombre del cliente es obligatorio.");

        cliente.Nombre = cliente.Nombre.Trim();

        if (cliente.IdCliente == 0)
        {
            _db.Clientes.Add(cliente);
        }
        else
        {
            var existente = _db.Clientes.Find(cliente.IdCliente)
                ?? throw new InvalidOperationException("El cliente ya no existe.");

            existente.Nombre = cliente.Nombre;
            existente.Telefono = cliente.Telefono;
            existente.Correo = cliente.Correo;
            existente.Direccion = cliente.Direccion;
        }

        _db.SaveChanges();
    }

    // Crea un cliente nuevo junto con su primer equipo, todo en un solo guardado:
    // si algo falla, no queda un cliente a medias.
    public Equipo GuardarConEquipo(Cliente cliente, Equipo equipo)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            throw new ArgumentException("El nombre del cliente es obligatorio.");

        if (string.IsNullOrWhiteSpace(equipo.Tipo))
            throw new ArgumentException("El tipo de equipo es obligatorio (por ejemplo: Laptop, Impresora).");

        static string? Limpiar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        cliente.IdCliente = 0;
        cliente.Nombre = cliente.Nombre.Trim();
        cliente.Telefono = Limpiar(cliente.Telefono);
        cliente.Correo = Limpiar(cliente.Correo);
        cliente.Direccion = Limpiar(cliente.Direccion);

        equipo.IdEquipo = 0;
        equipo.Tipo = equipo.Tipo.Trim();
        equipo.Marca = Limpiar(equipo.Marca);
        equipo.Modelo = Limpiar(equipo.Modelo);
        equipo.NumeroSerie = Limpiar(equipo.NumeroSerie);
        equipo.Cliente = cliente; // EF asigna el IdCliente al guardar

        _db.Clientes.Add(cliente);
        _db.Equipos.Add(equipo);
        _db.SaveChanges();

        return equipo;
    }

    public void Eliminar(int idCliente)
    {
        var cliente = _db.Clientes.Find(idCliente);
        if (cliente == null) return;

        bool tieneEquipos = _db.Equipos.Any(e => e.IdCliente == idCliente);
        if (tieneEquipos)
            throw new InvalidOperationException(
                "No se puede eliminar: este cliente tiene equipos registrados.");

        _db.Clientes.Remove(cliente);
        _db.SaveChanges();
    }

    // Cuántos equipos tiene registrados un cliente (para mostrarlo en la tabla)
    public int ContarEquipos(int idCliente)
    {
        return _db.Equipos.Count(e => e.IdCliente == idCliente);
    }
}