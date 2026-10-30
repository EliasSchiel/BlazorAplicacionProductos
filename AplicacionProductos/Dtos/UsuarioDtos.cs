namespace AplicacionProductos.Dtos;

public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaAlta { get; set; }
}

/// <summary>Body de POST api/Usuarios (solo Admin).</summary>
public class UsuarioCrearRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = "Operador";   // Admin | Operador | Cliente
    public string? Email { get; set; }
}

/// <summary>Body de PUT api/Usuarios/{id}. El username y la contraseña no se cambian por acá.</summary>
public class UsuarioActualizarRequest
{
    public string Rol { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;
}
