using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Ficha del cliente logueado (api/Perfil, solo rol Cliente). Usa los mismos DTOs que api/Clientes.</summary>
public class PerfilService
{
    private readonly ApiClient _api;
    public PerfilService(ApiClient api) => _api = api;

    public Task<ClienteDto?> GetAsync() => _api.GetAsync<ClienteDto>("api/Perfil");

    /// <summary>PUT api/Perfil. La API responde 400 si el DNI ya lo usa otro cliente.</summary>
    public Task ActualizarAsync(ClienteGuardarRequest datos) => _api.PutAsync("api/Perfil", datos);
}
