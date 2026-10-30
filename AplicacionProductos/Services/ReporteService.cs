using System.Globalization;
using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Reportes PDF de la API (solo Admin / Operador): ver el PDF o enviarlo por mail.</summary>
public class ReporteService
{
    private readonly ApiClient _api;
    public ReporteService(ApiClient api) => _api = api;

    /// <summary>Descarga el PDF, lo guarda en la cache de la app y lo abre (o abre el menu de compartir si no hay visor).</summary>
    public async Task AbrirAsync(string tipo, DateTime? desde, DateTime? hasta, string? estado)
    {
        var bytes = await _api.GetBytesAsync($"api/Reportes/{tipo}{ArmarQuery(tipo, desde, hasta, estado)}");

        var ruta = Path.Combine(FileSystem.CacheDirectory, $"reporte-{tipo}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
        await File.WriteAllBytesAsync(ruta, bytes);

        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest($"Reporte {tipo}", new ReadOnlyFile(ruta, "application/pdf")));
        }
        catch
        {
            // Sin visor de PDF instalado: se ofrece compartir / guardar el archivo
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Reporte {tipo}",
                File = new ShareFile(ruta, "application/pdf")
            });
        }
    }

    /// <summary>POST api/Reportes/{tipo}/enviar. Devuelve el mensaje de la API.</summary>
    public async Task<string> EnviarAsync(string tipo, string email, DateTime? desde, DateTime? hasta, string? estado)
    {
        var resp = await _api.PostAsync<MensajeResponse>($"api/Reportes/{tipo}/enviar", new EnviarReporteRequest
        {
            Email = email,
            Desde = desde,
            Hasta = hasta,
            Estado = string.IsNullOrEmpty(estado) ? null : estado
        });
        return resp?.Mensaje ?? "El reporte fue enviado.";
    }

    // Cada reporte usa solo los filtros que le corresponden
    private static string ArmarQuery(string tipo, DateTime? desde, DateTime? hasta, string? estado)
    {
        var q = new List<string>();
        if (tipo is "ingresos" or "ventas")
        {
            if (desde.HasValue) q.Add($"desde={desde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            if (hasta.HasValue) q.Add($"hasta={hasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        }
        if (tipo == "ventas" && !string.IsNullOrEmpty(estado)) q.Add($"estado={Uri.EscapeDataString(estado)}");
        return q.Count == 0 ? "" : "?" + string.Join("&", q);
    }
}
