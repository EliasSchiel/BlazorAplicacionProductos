using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>
/// Ventas (api/Ventas). Staff ve todas y vende en mostrador; un Cliente ve y crea solo las suyas.
/// Flujo: crear (queda Pendiente y descuenta stock) -> pagar (Pagada) o cancelar (devuelve stock).
/// </summary>
public class VentaService
{
    private readonly ApiClient _api;
    public VentaService(ApiClient api) => _api = api;

    public Task<PagedResult<VentaDto>?> GetPaginaAsync(int page, int pageSize, string? estado = null)
    {
        var url = $"api/Ventas?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(estado)) url += $"&estado={Uri.EscapeDataString(estado)}";
        return _api.GetAsync<PagedResult<VentaDto>>(url);
    }

    public Task<VentaDto?> CrearAsync(VentaCrearRequest datos) => _api.PostAsync<VentaDto>("api/Ventas", datos);

    public async Task<List<MetodoPagoDto>> GetMetodosPagoAsync()
        => await _api.GetAsync<List<MetodoPagoDto>>("api/Ventas/metodos-pago") ?? new();

    public async Task<PagoDto?> PagarAsync(int idVenta, PagoRequest datos)
    {
        try
        {
            return await _api.PostAsync<PagoDto>($"api/Ventas/{idVenta}/pagar", datos);
        }
        catch (ApiException ex) when (ex.StatusCode == 422)
        {
            // La API responde 422 cuando el pago simulado es rechazado (tarjeta terminada en 0000)
            throw new ApiException(422, "El pago fue rechazado. Probá con otra tarjeta u otro método de pago.", ex);
        }
    }

    public Task CancelarAsync(int idVenta) => _api.PostSinRespuestaAsync($"api/Ventas/{idVenta}/cancelar");
}
