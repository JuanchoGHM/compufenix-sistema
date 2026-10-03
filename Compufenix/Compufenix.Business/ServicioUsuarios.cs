using Compufenix.Data;
using Compufenix.Models;

namespace Compufenix.Business;

public class ServicioUsuarios
{
    private readonly CompufenixDbContext _db;

    public ServicioUsuarios(CompufenixDbContext db)
    {
        _db = db;
    }

    // ¿Hay al menos un usuario registrado?
    public bool ExisteAlgunUsuario()
    {
        return _db.Usuarios.Any();
    }

    // Crea un usuario nuevo, guardando la contraseña como hash
    public Usuario CrearUsuario(string nombre, string correo, string contrasena, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo))
            throw new ArgumentException("El correo es obligatorio.");

        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < 6)
            throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");

        correo = correo.Trim();

        if (_db.Usuarios.Any(u => u.Correo == correo))
            throw new InvalidOperationException("Ya existe un usuario con ese correo.");

        var usuario = new Usuario
        {
            Nombre = nombre.Trim(),
            Correo = correo,
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasena),
            Rol = rol,
            Activo = true
        };

        _db.Usuarios.Add(usuario);
        _db.SaveChanges();
        return usuario;
    }

    // Devuelve el usuario si el correo y la contraseña son correctos; si no, null
    public Usuario? IniciarSesion(string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(contrasena))
            return null;

        var correoLimpio = correo.Trim();
        var usuario = _db.Usuarios.FirstOrDefault(u => u.Correo == correoLimpio && u.Activo);

        if (usuario == null)
            return null;

        return BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaHash) ? usuario : null;
    }


    // Lista de todos los usuarios, el más reciente primero
    public List<Usuario> ObtenerTodos()
    {
        return _db.Usuarios.OrderByDescending(u => u.IdUsuario).ToList();
    }

    // Activa o desactiva a un usuario (no se elimina, para conservar el historial)
    public void CambiarActivo(int idUsuario, bool activo)
    {
        var usuario = _db.Usuarios.Find(idUsuario)
            ?? throw new InvalidOperationException("El usuario ya no existe.");

        usuario.Activo = activo;
        _db.SaveChanges();
    }

}