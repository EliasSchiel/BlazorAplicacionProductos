using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AplicacionProductos.Services;

/// <summary>Error de la API o de la conexion, con un mensaje listo para mostrarle al usuario.</summary>
public class ApiException : Exception
{
    public int StatusCode { get; }   // 0 = no hubo respuesta (sin conexion, timeout)
    public ApiException(int statusCode, string message, Exception? inner = null) : base(message, inner)
        => StatusCode = statusCode;
}

/// <summary>
/// Unico lugar que habla HTTP con la API: agrega el token, serializa JSON y traduce errores a ApiException.
/// Los servicios solo arman la URL y llaman aca.
/// </summary>
public class ApiClient
{
    // La API manda los enums como texto ("Pagada") y en camelCase
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ApiClient(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        using var resp = await EnviarAsync(HttpMethod.Get, url);
        return await resp.Content.ReadFromJsonAsync<T>(Json);
    }

    public async Task<byte[]> GetBytesAsync(string url)
    {
        using var resp = await EnviarAsync(HttpMethod.Get, url);
        return await resp.Content.ReadAsByteArrayAsync();
    }

    public async Task<TResp?> PostAsync<TResp>(string url, object body)
    {
        using var resp = await EnviarAsync(HttpMethod.Post, url, body);
        return await resp.Content.ReadFromJsonAsync<TResp>(Json);
    }

    /// <summary>POST sin body y sin contenido en la respuesta (ej: cancelar una venta).</summary>
    public async Task PostSinRespuestaAsync(string url)
    {
        using var resp = await EnviarAsync(HttpMethod.Post, url);
    }

    /// <summary>PUT con body JSON. La API responde 204 (sin contenido).</summary>
    public async Task PutAsync(string url, object body)
    {
        using var resp = await EnviarAsync(HttpMethod.Put, url, body);
    }

    /// <summary>PUT sin body (ej: marcar imagen principal).</summary>
    public async Task PutAsync(string url)
    {
        using var resp = await EnviarAsync(HttpMethod.Put, url);
    }

    public async Task DeleteAsync(string url)
    {
        using var resp = await EnviarAsync(HttpMethod.Delete, url);
    }

    /// <summary>POST multipart/form-data (subida de archivos).</summary>
    public async Task<TResp?> PostFormAsync<TResp>(string url, MultipartFormDataContent formulario)
    {
        using var resp = await EnviarAsync(HttpMethod.Post, url, contenido: formulario);
        return await resp.Content.ReadFromJsonAsync<TResp>(Json);
    }

    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string url, object? body = null, HttpContent? contenido = null)
    {
        using var req = new HttpRequestMessage(metodo, url);
        if (!string.IsNullOrEmpty(_auth.Token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);
        if (body is not null)
            req.Content = JsonContent.Create(body, body.GetType(), options: Json);
        else if (contenido is not null)
            req.Content = contenido;

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(0, "No se pudo conectar con el servidor. Revisá tu conexión a internet.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException(0, "El servidor tardó demasiado en responder.", ex);
        }

        if (resp.IsSuccessStatusCode) return resp;

        var mensaje = resp.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Tu sesión venció o no es válida. Iniciá sesión de nuevo.",
            HttpStatusCode.Forbidden => "No tenés permisos para esta acción.",
            _ => await LeerMensajeAsync(resp)
        };
        var status = (int)resp.StatusCode;
        resp.Dispose();

        if (status == 401) await _auth.LogoutAsync();   // token vencido: se limpia la sesion
        throw new ApiException(status, mensaje);
    }

    /// <summary>
    /// Saca el texto del error segun como responda la API: texto plano (BadRequest("...")),
    /// ProblemDetails (title/detail) o errores de validacion ({ errors: { Campo: ["..."] } }).
    /// </summary>
    public static async Task<string> LeerMensajeAsync(HttpResponseMessage resp)
    {
        var texto = (await resp.Content.ReadAsStringAsync()).Trim();
        if (texto.Length == 0) return $"Error {(int)resp.StatusCode} ({resp.ReasonPhrase}).";

        if (texto.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(texto);
                var root = doc.RootElement;

                if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                    return detail.GetString()!;

                // Ventas: BadRequest(new { errores = ["Stock insuficiente de ...", ...] })
                if (root.TryGetProperty("errores", out var lista) && lista.ValueKind == JsonValueKind.Array && lista.GetArrayLength() > 0)
                    return string.Join(" ", lista.EnumerateArray().Select(e => e.GetString()).Where(e => !string.IsNullOrWhiteSpace(e)));

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    foreach (var campo in errors.EnumerateObject())
                        if (campo.Value.ValueKind == JsonValueKind.Array && campo.Value.GetArrayLength() > 0)
                            return campo.Value[0].GetString() ?? "Datos inválidos.";

                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString()!;
            }
            catch (JsonException) { /* no era JSON: se devuelve el texto tal cual */ }
        }

        return texto.Trim('"');
    }
}
