namespace AplicacionProductos.Dtos;

public class ProveedorDto
{
    public int IdProveedor { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? Cuit { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
}

/// <summary>Body de POST api/Proveedores</summary>
public class ProveedorGuardarRequest
{
    public string RazonSocial { get; set; } = string.Empty;
    public string? Cuit { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
}
