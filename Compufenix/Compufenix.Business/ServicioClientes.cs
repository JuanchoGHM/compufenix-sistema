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