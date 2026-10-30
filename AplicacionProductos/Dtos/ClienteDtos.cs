namespace AplicacionProductos.Dtos;

public class ClienteDto
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public int? IdUsuario { get; set; }   // si tiene valor, el cliente tiene cuenta para entrar a la app
}

/// <summary>Body de POST (crear) y PUT (editar) api/Clientes.</summary>
public class ClienteGuardarRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
