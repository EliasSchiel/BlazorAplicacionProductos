namespace AplicacionProductos.Dtos;

public class IngresoDto
{
    public int IdIngreso { get; set; }
    public DateTime Fecha { get; set; }
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? NumeroComprobante { get; set; }
    public string? Observaciones { get; set; }
    public decimal Total { get; set; }
    public List<DetalleIngresoDto> Detalles { get; set; } = new();
}

public class DetalleIngresoDto
{
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>Body de POST api/Ingresos: suma stock a cada producto de la lista.</summary>
public class IngresoCrearRequest
{
    public int IdProveedor { get; set; }
    public string? NumeroComprobante { get; set; }
    public string? Observaciones { get; set; }
    public List<IngresoItemRequest> Items { get; set; } = new();
}

public class IngresoItemRequest
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}
