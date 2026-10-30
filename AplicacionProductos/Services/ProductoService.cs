using System.Net.Http.Headers;
using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

public class ProductoService
{
    private readonly ApiClient _api;
    public ProductoService(ApiClient api) => _api = api;

    // ---------- Lectura (GET es publico) ----------

    // GET api/Productos?page=&pageSize=&busqueda=&idCategoria=&incluirInactivos=&soloDisponibles=&orden=
    /// <param name="orden">nombre (por defecto) | precio_asc | precio_desc | recientes</param>
    /// <param name="soloDisponibles">true = solo con stock (catalogo del cliente)</param>
    public Task<PagedResult<ProductoDto>?> GetPaginaAsync(int page, int pageSize, string? busqueda = null,
        int? idCategoria = null, bool incluirInactivos = false, bool soloDisponibles = false, string? orden = null)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(busqueda)) q.Add($"busqueda={Uri.EscapeDataString(busqueda.Trim())}");
        if (idCategoria is > 0) q.Add($"idCategoria={idCategoria}");
        if (incluirInactivos) q.Add("incluirInactivos=true");   // la API solo lo respeta si el usuario es Admin/Operador
        if (soloDisponibles) q.Add("soloDisponibles=true");
        if (!string.IsNullOrEmpty(orden)) q.Add($"orden={Uri.EscapeDataString(orden)}");
        return _api.GetAsync<PagedResult<ProductoDto>>("api/Productos?" + string.Join("&", q));
    }

    public Task<ProductoDto?> GetByIdAsync(int id) => _api.GetAsync<ProductoDto>($"api/Productos/{id}");

    /// <summary>GET api/Productos/stock-bajo (Admin / Operador): activos con stock en o debajo del minimo.</summary>
    public async Task<List<ProductoDto>> GetStockBajoAsync()
        => await _api.GetAsync<List<ProductoDto>>("api/Productos/stock-bajo") ?? new();

    // ---------- ABM (Admin / Operador) ----------

    public Task<ProductoDto?> CrearAsync(ProductoGuardarRequest datos) => _api.PostAsync<ProductoDto>("api/Productos", datos);

    public Task ActualizarAsync(int id, ProductoGuardarRequest datos) => _api.PutAsync($"api/Productos/{id}", datos);

    /// <summary>Baja logica (solo Admin): el producto queda inactivo, no se borra.</summary>
    public Task DarDeBajaAsync(int id) => _api.DeleteAsync($"api/Productos/{id}");

    // ---------- Imagenes ----------

    /// <summary>POST api/Productos/{id}/imagenes (multipart: campo "archivo").</summary>
    public Task<ProductoImagenDto?> SubirImagenAsync(int idProducto, Stream contenido, string nombreArchivo,
        string? contentType, bool esPrincipal = false)
    {
        var archivo = new StreamContent(contenido);
        archivo.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType)
            ? TipoPorExtension(nombreArchivo) : contentType);

        var form = new MultipartFormDataContent
        {
            { archivo, "archivo", nombreArchivo },
            { new StringContent(esPrincipal ? "true" : "false"), "esPrincipal" }
        };
        return _api.PostFormAsync<ProductoImagenDto>($"api/Productos/{idProducto}/imagenes", form);
    }

    public Task MarcarImagenPrincipalAsync(int idProducto, int idImagen)
        => _api.PutAsync($"api/Productos/{idProducto}/imagenes/{idImagen}/principal");

    public Task EliminarImagenAsync(int idProducto, int idImagen)
        => _api.DeleteAsync($"api/Productos/{idProducto}/imagenes/{idImagen}");

    private static string TipoPorExtension(string nombre) => Path.GetExtension(nombre).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg"
    };
}
