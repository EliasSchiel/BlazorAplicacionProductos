namespace AplicacionProductos.Dtos;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public int IdUsuario { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? IdCliente { get; set; }
}

/// <summary>Body de POST api/Auth/registro (alta publica de un Cliente).</summary>
public class RegistroClienteRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}

/// <summary>Body de PUT api/Auth/cambiar-password</summary>
public class CambiarPasswordRequest
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}
