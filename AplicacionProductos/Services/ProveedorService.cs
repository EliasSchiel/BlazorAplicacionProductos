using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Proveedores (api/Proveedores, Admin / Operador). Eliminar es solo Admin.</summary>
public class ProveedorService
{
    private readonly ApiClient _api;
    public ProveedorService(ApiClient api) => _api = api;

    public Task<PagedResult<ProveedorDto>?> GetPaginaAsync(int page, int pageSize, string? busqueda = null)
    {
        var url = $"api/Proveedores?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(busqueda)) url += $"&busqueda={Uri.EscapeDataString(busqueda.Trim())}";
        return _api.GetAsync<PagedResult<ProveedorDto>>(url);
    }

    /// <summary>Lista corta para desplegables (ej: el alta de un ingreso).</summary>
    public async Task<List<ProveedorDto>> GetAsync()
    {
        var pagina = await _api.GetAsync<PagedResult<ProveedorDto>>("api/Proveedores?pageSize=100");
        return pagina?.Items ?? new();
    }

    public Task<ProveedorDto?> CrearAsync(ProveedorGuardarRequest datos) => _api.PostAsync<ProveedorDto>("api/Proveedores", datos);

    public Task ActualizarAsync(int id, ProveedorGuardarRequest datos) => _api.PutAsync($"api/Proveedores/{id}", datos);

    /// <summary>Solo Admin. La API responde 409 si el proveedor tiene ingresos registrados.</summary>
    public Task EliminarAsync(int id) => _api.DeleteAsync($"api/Proveedores/{id}");
}
