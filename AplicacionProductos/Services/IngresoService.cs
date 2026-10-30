using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Ingresos de mercaderia (api/Ingresos, Admin / Operador): cada ingreso suma stock a los productos.</summary>
public class IngresoService
{
    private readonly ApiClient _api;
    public IngresoService(ApiClient api) => _api = api;

    public Task<PagedResult<IngresoDto>?> GetPaginaAsync(int page, int pageSize)
        => _api.GetAsync<PagedResult<IngresoDto>>($"api/Ingresos?page={page}&pageSize={pageSize}");

    public Task<IngresoDto?> CrearAsync(IngresoCrearRequest datos) => _api.PostAsync<IngresoDto>("api/Ingresos", datos);
}
