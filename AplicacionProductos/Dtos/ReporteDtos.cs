namespace AplicacionProductos.Dtos;

/// <summary>Body de POST api/Reportes/{tipo}/enviar</summary>
public class EnviarReporteRequest
{
    public string Email { get; set; } = string.Empty;
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Estado { get; set; }   // solo ventas: Pendiente | Pagada | Cancelada
}

public class MensajeResponse
{
    public string Mensaje { get; set; } = string.Empty;
}
