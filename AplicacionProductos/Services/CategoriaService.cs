using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

public class CategoriaService
{
    private readonly ApiClient _api;
    public CategoriaService(ApiClient api) => _api = api;

    // GET api/Categorias (publico). Por defecto solo las activas: son las que se ofrecen en desplegables y filtros.
    public async Task<List<CategoriaDto>> GetAsync(bool incluirInactivas = false)
    {
        var url = "api/Categorias?pageSize=100" + (incluirInactivas ? "&incluirInactivas=true" : "");
        var pagina = await _api.GetAsync<PagedResult<CategoriaDto>>(url);
        return pagina?.Items ?? new();
    }

    // ---------- ABM (Admin / Operador) ----------

    public Task<CategoriaDto?> CrearAsync(CategoriaGuardarRequest datos) => _api.PostAsync<CategoriaDto>("api/Categorias", datos);

    public Task ActualizarAsync(int id, CategoriaGuardarRequest datos) => _api.PutAsync($"api/Categorias/{id}", datos);

    /// <summary>Solo Admin. La API lo rechaza (409) si la categoria tiene productos: en ese caso se desactiva.</summary>
    public Task EliminarAsync(int id) => _api.DeleteAsync($"api/Categorias/{id}");
}
