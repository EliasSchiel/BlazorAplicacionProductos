using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Administracion de usuarios (api/Usuarios, solo Admin) y cambio de la propia contraseña.</summary>
public class UsuarioService
{
    private readonly ApiClient _api;
    public UsuarioService(ApiClient api) => _api = api;

    public Task<PagedResult<UsuarioDto>?> GetPaginaAsync(int page, int pageSize, string? rol = null)
    {
        var url = $"api/Usuarios?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(rol)) url += $"&rol={Uri.EscapeDataString(rol)}";
        return _api.GetAsync<PagedResult<UsuarioDto>>(url);
    }

    public Task<UsuarioDto?> CrearAsync(UsuarioCrearRequest datos) => _api.PostAsync<UsuarioDto>("api/Usuarios", datos);

    public Task ActualizarAsync(int id, UsuarioActualizarRequest datos) => _api.PutAsync($"api/Usuarios/{id}", datos);

    /// <summary>Baja logica: el usuario queda inactivo y ya no puede iniciar sesion.</summary>
    public Task DarDeBajaAsync(int id) => _api.DeleteAsync($"api/Usuarios/{id}");

    /// <summary>PUT api/Auth/cambiar-password (cualquier usuario logueado, sobre su propia cuenta).</summary>
    public Task CambiarPasswordAsync(string actual, string nueva)
        => _api.PutAsync("api/Auth/cambiar-password", new CambiarPasswordRequest { PasswordActual = actual, PasswordNueva = nueva });
}
