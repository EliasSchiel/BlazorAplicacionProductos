namespace AplicacionProductos.Dtos;

/// <summary>Registro de GET api/Auditoria (la API devuelve la entidad Auditoria tal cual).</summary>
public class AuditoriaDto
{
    public int IdAuditoria { get; set; }
    public DateTime Fecha { get; set; }
    public int? IdUsuario { get; set; }
    public string Accion { get; set; } = string.Empty;    // Alta, Modificacion, Baja, Login, LoginFallido, EnvioReporte
    public string Entidad { get; set; } = string.Empty;   // Producto, Venta, Usuario...
    public string? IdEntidad { get; set; }
    public string? Detalle { get; set; }                  // JSON con los cambios
    public string? Ip { get; set; }
}

/// <summary>Registro de GET api/Auditoria/errores (tabla LogErrores).</summary>
public class LogErrorDto
{
    public int IdLogError { get; set; }
    public DateTime Fecha { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Excepcion { get; set; }    // tipo + stack trace (en los de la app incluye plataforma y version)
    public string? Ruta { get; set; }         // ruta de la API o pantalla de la app
    public string? MetodoHttp { get; set; }
    public int? IdUsuario { get; set; }
    public string Origen { get; set; } = string.Empty;   // "API" o "App"
}
