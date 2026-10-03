using Compufenix.Data;
using Compufenix.Models;

namespace Compufenix.Business;

public class ServicioInventario
{
    private readonly CompufenixDbContext _db;

    public ServicioInventario(CompufenixDbContext db)
    {
        _db = db;
    }

    // Lista de productos, opcionalmente filtrada por texto en el nombre
    public List<Producto> Buscar(string? texto = null)
    {
        var consulta = _db.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(t));
        }

        return consulta.OrderBy(p => p.Nombre).ToList();
    }

    // Crea o actualiza un producto, validando las reglas del negocio
    public void Guardar(Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.Nombre))
            throw new ArgumentException("El nombre del producto es obligatorio.");

        if (producto.StockActual < 0)
            throw new ArgumentException("El stock actual no puede ser negativo.");

        if (producto.StockMinimo < 0)
            throw new ArgumentException("El stock mínimo no puede ser negativo.");

        if (producto.PrecioUnitario < 0)
            throw new ArgumentException("El precio no puede ser negativo.");

        producto.Nombre = producto.Nombre.Trim();

        if (producto.IdProducto == 0)
        {
            // Es un producto nuevo
            _db.Productos.Add(producto);
        }
        else
        {
            // Es una edición: se busca el original y se actualizan sus datos
            var existente = _db.Productos.Find(producto.IdProducto)
                ?? throw new InvalidOperationException("El producto ya no existe.");

            existente.Nombre = producto.Nombre;
            existente.Categoria = producto.Categoria;
            existente.StockActual = producto.StockActual;
            existente.StockMinimo = producto.StockMinimo;
            existente.PrecioUnitario = producto.PrecioUnitario;
            existente.FechaIncorporacion = producto.FechaIncorporacion;
        }

        _db.SaveChanges();
    }

    // Elimina un producto, siempre que no tenga movimientos registrados
    public void Eliminar(int idProducto)
    {
        var producto = _db.Productos.Find(idProducto);
        if (producto == null) return;

        bool tieneMovimientos = _db.MovimientosInventario.Any(m => m.IdProducto == idProducto);
        if (tieneMovimientos)
            throw new InvalidOperationException(
                "No se puede eliminar: este producto ya tiene movimientos de inventario registrados.");

        _db.Productos.Remove(producto);
        _db.SaveChanges();
    }


    // Registra una entrada o salida de stock y actualiza el producto
    public void RegistrarMovimiento(int idProducto, TipoMovimiento tipo, int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.");

        var producto = _db.Productos.Find(idProducto)
            ?? throw new InvalidOperationException("El producto ya no existe.");

        if (tipo == TipoMovimiento.Salida && cantidad > producto.StockActual)
            throw new InvalidOperationException(
                $"No hay suficiente stock. Disponible: {producto.StockActual}.");

        producto.StockActual += tipo == TipoMovimiento.Entrada ? cantidad : -cantidad;

        _db.MovimientosInventario.Add(new MovimientoInventario
        {
            IdProducto = idProducto,
            Tipo = tipo,
            Cantidad = cantidad,
            Fecha = DateTime.Now
        });

        _db.SaveChanges();
    }

    // Historial de movimientos de un producto, del más reciente al más antiguo
    public List<MovimientoInventario> ObtenerMovimientos(int idProducto)
    {
        return _db.MovimientosInventario
            .Where(m => m.IdProducto == idProducto)
            .OrderByDescending(m => m.Fecha)
            .ToList();
    }
}