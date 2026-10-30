using System.Globalization;
using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Auditoria de acciones y log de errores (api/Auditoria, solo Admin).</summary>
public class AuditoriaService
{
    private readonly ApiClient _api;
    public AuditoriaService(ApiClient api) => _api = api;

    // GET api/Auditoria?page=&pageSize=&entidad=&accion=&desde=&hasta=
    public Task<PagedResult<AuditoriaDto>?> GetPaginaAsync(int page, int pageSize, string? entidad = null, string? accion = null,
        DateTime? desde = null, DateTime? hasta = null)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(entidad)) q.Add($"entidad={Uri.EscapeDataString(entidad)}");
        if (!string.IsNullOrWhiteSpace(accion)) q.Add($"accion={Uri.EscapeDataString(accion)}");
        AgregarFechas(q, desde, hasta);
        return _api.GetAsync<PagedResult<AuditoriaDto>>("api/Auditoria?" + string.Join("&", q));
    }

    // GET api/Auditoria/errores?page=&pageSize=&origen=&desde=&hasta=
    public Task<PagedResult<LogErrorDto>?> GetErroresAsync(int page, int pageSize, string? origen = null,
        DateTime? desde = null, DateTime? hasta = null)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(origen)) q.Add($"origen={Uri.EscapeDataString(origen)}");
        AgregarFechas(q, desde, hasta);
        return _api.GetAsync<PagedResult<LogErrorDto>>("api/Auditoria/errores?" + string.Join("&", q));
    }

    private static void AgregarFechas(List<string> q, DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue) q.Add($"desde={desde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        if (hasta.HasValue) q.Add($"hasta={hasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
    }
}
