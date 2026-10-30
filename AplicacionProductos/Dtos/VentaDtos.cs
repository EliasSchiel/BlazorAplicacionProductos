namespace AplicacionProductos.Dtos;

public class VentaDto
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public int IdCliente { get; set; }
    public string? ClienteNombre { get; set; }
    public string? UsuarioNombre { get; set; }
    public string Estado { get; set; } = string.Empty;   // Pendiente | Pagada | Cancelada
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();
    public List<PagoDto> Pagos { get; set; } = new();
}

public class DetalleVentaDto
{
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class PagoDto
{
    public int IdPago { get; set; }
    public DateTime Fecha { get; set; }
    public string? MetodoPago { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string CodigoTransaccion { get; set; } = string.Empty;
}

public class MetodoPagoDto
{
    public int IdMetodoPago { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Body de POST api/Ventas. Si compra un Cliente, la API ignora IdCliente y usa el suyo.</summary>
public class VentaCrearRequest
{
    public int? IdCliente { get; set; }
    public string? Observaciones { get; set; }
    public List<VentaItemRequest> Items { get; set; } = new();
}

public class VentaItemRequest
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
}

/// <summary>Body de POST api/Ventas/{id}/pagar (pago simulado: una tarjeta que termina en 0000 se rechaza).</summary>
public class PagoRequest
{
    public int IdMetodoPago { get; set; }
    public string? NumeroTarjeta { get; set; }
}
