using System.Text.RegularExpressions;
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

    // Revisa que el texto tenga la forma básica de un correo: algo@algo.algo
    private static bool EsCorreoValido(string correo)
    {
        return Regex.IsMatch(correo, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
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

        if (!EsCorreoValido(correo))
            throw new ArgumentException("El correo no tiene un formato válido (ejemplo: nombre@dominio.com).");

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

    // Devuelve el usuario si el correo y la contraseña son correctos; si no, null.
    // Lanza una excepción si la cuenta está bloqueada por demasiados intentos fallidos.
    public Usuario? IniciarSesion(string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(contrasena))
            return null;

        var correoLimpio = correo.Trim();
        var usuario = _db.Usuarios.FirstOrDefault(u => u.Correo == correoLimpio && u.Activo);

        if (usuario == null)
            return null;

        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.Now)
        {
            throw new InvalidOperationException(
                $"Cuenta bloqueada temporalmente por demasiados intentos fallidos. " +
                $"Intenta de nuevo después de las {usuario.BloqueadoHasta.Value:HH:mm}.");
        }

        bool contrasenaCorrecta = BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaHash);

        if (contrasenaCorrecta)
        {
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;
            _db.SaveChanges();
            return usuario;
        }

        usuario.IntentosFallidos++;

        if (usuario.IntentosFallidos >= 5)
        {
            usuario.BloqueadoHasta = DateTime.Now.AddMinutes(15);
            usuario.IntentosFallidos = 0;
            _db.SaveChanges();
            throw new InvalidOperationException(
                "Demasiados intentos fallidos. Esta cuenta quedó bloqueada por 15 minutos.");
        }

        _db.SaveChanges();
        return null;
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

    // Actualiza nombre, correo y rol; la contraseña solo se cambia si se envía una nueva
    public void Editar(int idUsuario, string nombre, string correo, RolUsuario rol, string? nuevaContrasena)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo))
            throw new ArgumentException("El correo es obligatorio.");

        if (!EsCorreoValido(correo))
            throw new ArgumentException("El correo no tiene un formato válido (ejemplo: nombre@dominio.com).");

        correo = correo.Trim();

        var usuario = _db.Usuarios.Find(idUsuario)
            ?? throw new InvalidOperationException("El usuario ya no existe.");

        if (_db.Usuarios.Any(u => u.Correo == correo && u.IdUsuario != idUsuario))
            throw new InvalidOperationException("Ya existe otro usuario con ese correo.");

        usuario.Nombre = nombre.Trim();
        usuario.Correo = correo;
        usuario.Rol = rol;

        if (!string.IsNullOrEmpty(nuevaContrasena))
        {
            if (nuevaContrasena.Length < 6)
                throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres.");

            usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
        }

        _db.SaveChanges();
    }

    // Elimina un usuario, con las mismas protecciones de seguridad del sistema
    public void Eliminar(int idUsuario, int idUsuarioActual)
    {
        if (idUsuario == idUsuarioActual)
            throw new InvalidOperationException("No puedes eliminar tu propio usuario mientras tienes la sesión abierta.");

        var usuario = _db.Usuarios.Find(idUsuario)
            ?? throw new InvalidOperationException("El usuario ya no existe.");

        bool tieneTickets = _db.Tickets.Any(t => t.IdTecnico == idUsuario);
        if (tieneTickets)
            throw new InvalidOperationException(
                "No se puede eliminar: este usuario tiene tickets asignados. Puedes desactivarlo en su lugar.");

        if (usuario.Rol == RolUsuario.Administrador)
        {
            int totalAdmins = _db.Usuarios.Count(u => u.Rol == RolUsuario.Administrador);
            if (totalAdmins <= 1)
                throw new InvalidOperationException("No se puede eliminar: debe existir al menos un administrador.");
        }

        _db.Usuarios.Remove(usuario);
        _db.SaveChanges();
    }


    // Genera un código de 6 dígitos, válido por 15 minutos. Devuelve null si no hay
    // ningún usuario activo con ese correo (así no revelamos si el correo existe).
    public string? GenerarCodigoRecuperacion(string correo)
    {
        var correoLimpio = correo.Trim();
        var usuario = _db.Usuarios.FirstOrDefault(u => u.Correo == correoLimpio && u.Activo);
        if (usuario == null) return null;

        var codigo = new Random().Next(100000, 999999).ToString();
        usuario.CodigoRecuperacion = codigo;
        usuario.CodigoExpira = DateTime.Now.AddMinutes(15);
        _db.SaveChanges();

        return codigo;
    }

    // Cambia la contraseña si el código coincide y no ha expirado
    public void RestablecerContrasena(string correo, string codigo, string nuevaContrasena)
    {
        if (string.IsNullOrEmpty(nuevaContrasena) || nuevaContrasena.Length < 6)
            throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres.");

        var correoLimpio = correo.Trim();
        var usuario = _db.Usuarios.FirstOrDefault(u => u.Correo == correoLimpio && u.Activo)
            ?? throw new InvalidOperationException("Correo o código incorrectos.");

        if (usuario.CodigoRecuperacion != codigo ||
            usuario.CodigoExpira == null || usuario.CodigoExpira < DateTime.Now)
        {
            throw new InvalidOperationException("El código es incorrecto o ha expirado. Solicita uno nuevo.");
        }

        usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
        usuario.CodigoRecuperacion = null;
        usuario.CodigoExpira = null;
        _db.SaveChanges();
    }

}