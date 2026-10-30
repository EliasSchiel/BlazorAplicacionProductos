namespace AplicacionProductos.Config;

public static class ApiConfig
{
    // Debe terminar en "/" para que las rutas relativas ("api/Auth/login") se resuelvan bien.
    // Produccion (MonsterASP, HTTPS: Android no pide ninguna configuracion extra).
    public const string BaseUrl = "https://appproductossg.runasp.net/";

    /// <summary>La API puede devolver la URL de una imagen absoluta o relativa: siempre se devuelve absoluta.</summary>
    public static string ResolverUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out _) ? url : new Uri(new Uri(BaseUrl), url.TrimStart('/')).ToString();
}
