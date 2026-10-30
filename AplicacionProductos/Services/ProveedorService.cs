using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

public class ProveedorService
{
    private readonly ApiClient _api;
    public ProveedorService(ApiClient api) => _api = api;

    public async Task<List<ProveedorDto>> GetAsync()
    {
        var pagina = await _api.GetAsync<PagedResult<ProveedorDto>>("api/Proveedores?pageSize=100");
        return pagina?.Items ?? new();
    }

    public Task<ProveedorDto?> CrearAsync(ProveedorGuardarRequest datos) => _api.PostAsync<ProveedorDto>("api/Proveedores", datos);
}
