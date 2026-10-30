using System.Net.Http.Json;
using System.Net;
using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>
/// Sesion del usuario: hace el login, guarda el token (SecureStorage) y lo expone al resto de la app.
/// Usa HttpClient directo (no ApiClient) porque ApiClient depende de este servicio.
/// </summary>
public class AuthService
{
    private const string KToken = "auth_token";
    private const string KUsuario = "auth_username";
    private const string KRol = "auth_rol";
    private const string KExpira = "auth_expira";

    private readonly HttpClient _http;
    private Task? _inicio;

    public string? Token { get; private set; }
    public string? Username { get; private set; }
    public string? Rol { get; private set; }

    public bool EstaLogueado => !string.IsNullOrEmpty(Token);
    public bool EsStaff => Rol is "Admin" or "Operador";
    public bool EsAdmin => Rol == "Admin";

    /// <summary>Se dispara cuando cambia la sesion (login / logout) para que el menu se redibuje.</summary>
    public event Action? Cambio;

    public AuthService(HttpClient http) => _http = http;

    /// <summary>Recupera la sesion guardada. Se puede llamar desde cualquier pagina: solo se ejecuta una vez.</summary>
    public Task InicializarAsync() => _inicio ??= RestaurarAsync();

    private async Task RestaurarAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(KToken);
            var expira = await SecureStorage.Default.GetAsync(KExpira);

            // Si el token ya vencio no tiene sentido restaurarlo
            if (!string.IsNullOrEmpty(token) &&
                DateTime.TryParse(expira, null, System.Globalization.DateTimeStyles.RoundtripKind, out var vence) &&
                vence.ToUniversalTime() > DateTime.UtcNow)
            {
                Token = token;
                Username = await SecureStorage.Default.GetAsync(KUsuario);
                Rol = await SecureStorage.Default.GetAsync(KRol);
            }
        }
        catch
        {
            // SecureStorage puede no estar disponible (ej: Windows sin empaquetar). La sesion queda solo en memoria.
        }
        Cambio?.Invoke();
    }

    public Task LoginAsync(string username, string password)
        => IniciarSesionAsync("api/Auth/login",
            new LoginRequest { Username = username, Password = password },
            "Usuario o contraseña incorrectos.");

    /// <summary>Registro publico de un Cliente: la API crea el usuario y devuelve el token, asi que queda logueado.</summary>
    public Task RegistrarAsync(RegistroClienteRequest datos)
        => IniciarSesionAsync("api/Auth/registro", datos, null);

    // Login y registro responden lo mismo (LoginResponse con el token): comparten este metodo
    private async Task IniciarSesionAsync(string url, object body, string? mensaje401)
    {
        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsync(url, JsonContent.Create(body, body.GetType(), options: ApiClient.Json));
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(0, "No se pudo conectar con el servidor. Revisá tu conexión a internet.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException(0, "El servidor tardó demasiado en responder.", ex);
        }

        using (resp)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var mensaje = resp.StatusCode == HttpStatusCode.Unauthorized && mensaje401 is not null
                    ? mensaje401
                    : await ApiClient.LeerMensajeAsync(resp);
                throw new ApiException((int)resp.StatusCode, mensaje);
            }

            var datos = await resp.Content.ReadFromJsonAsync<LoginResponse>(ApiClient.Json)
                        ?? throw new ApiException((int)resp.StatusCode, "Respuesta inválida del servidor.");

            Token = datos.Token;
            Username = datos.Username;
            Rol = datos.Rol;

            try
            {
                await SecureStorage.Default.SetAsync(KToken, datos.Token);
                await SecureStorage.Default.SetAsync(KUsuario, datos.Username);
                await SecureStorage.Default.SetAsync(KRol, datos.Rol);
                await SecureStorage.Default.SetAsync(KExpira, datos.Expira.ToString("O"));
            }
            catch { /* igual que arriba: sin almacenamiento seguro la sesion dura mientras la app este abierta */ }

            Cambio?.Invoke();
        }
    }

    public Task LogoutAsync()
    {
        Token = Username = Rol = null;
        try
        {
            SecureStorage.Default.Remove(KToken);
            SecureStorage.Default.Remove(KUsuario);
            SecureStorage.Default.Remove(KRol);
            SecureStorage.Default.Remove(KExpira);
        }
        catch { }
        Cambio?.Invoke();
        return Task.CompletedTask;
    }
}
