using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Fichas de clientes (api/Clientes, Admin / Operador). Eliminar es solo Admin.</summary>
public class ClienteService
{
    private readonly ApiClient _api;
    public ClienteService(ApiClient api) => _api = api;

    public Task<PagedResult<ClienteDto>?> GetPaginaAsync(int page, int pageSize, string? busqueda = null)
    {
        var url = $"api/Clientes?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(busqueda)) url += $"&busqueda={Uri.EscapeDataString(busqueda.Trim())}";
        return _api.GetAsync<PagedResult<ClienteDto>>(url);
    }

    public Task<ClienteDto?> CrearAsync(ClienteGuardarRequest datos) => _api.PostAsync<ClienteDto>("api/Clientes", datos);

    public Task ActualizarAsync(int id, ClienteGuardarRequest datos) => _api.PutAsync($"api/Clientes/{id}", datos);

    /// <summary>La API responde 409 si el cliente tiene ventas registradas.</summary>
    public Task EliminarAsync(int id) => _api.DeleteAsync($"api/Clientes/{id}");
}
