using System.Net.Http.Json;
namespace AplicacionProductos.Services;

/// <summary>
/// Informa los errores de la app a la API (POST api/Logs/app) para que queden en LogErrores con Origen = "App".
/// Nunca lanza excepciones: un error al reportar un error no puede romper la app.
/// </summary>
public class LogService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public LogService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    /// <summary>Version "dispara y olvida" para usar desde catch o desde handlers globales.</summary>
    public void Reportar(Exception ex, string? pantalla = null)
        => _ = ReportarAsync(string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message, ex.ToString(), pantalla);

    public async Task ReportarAsync(string mensaje, string? detalle = null, string? pantalla = null)
    {
        try
        {
            var body = new
            {
                mensaje = Recortar(mensaje, 2000),
                detalle = Recortar(detalle, 8000),
                pantalla = Recortar(pantalla, 300),
                plataforma = Recortar($"{DeviceInfo.Platform} {DeviceInfo.VersionString}", 50),
                versionApp = Recortar(AppInfo.VersionString, 20)
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "api/Logs/app")
            {
                Content = JsonContent.Create(body, options: ApiClient.Json)
            };
            // El endpoint es anonimo; si hay sesion, el token hace que el error quede asociado al usuario
            if (!string.IsNullOrEmpty(_auth.Token))
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var resp = await _http.SendAsync(req, cts.Token);
        }
        catch
        {
            // Sin conexion o API caida: no hay nada mas que hacer
        }
    }

    private static string? Recortar(string? texto, int max)
        => texto is null ? null : (texto.Length <= max ? texto : texto[..max]);
}
