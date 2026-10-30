using AplicacionProductos.Config;
using AplicacionProductos.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace AplicacionProductos
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();   // servicios de MudBlazor (Snackbar, Dialog, etc.)

            // Un unico HttpClient apuntando a la API publicada (BaseAddress termina en "/")
            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            });

            // Orden de dependencias: AuthService -> ApiClient -> servicios de cada pantalla
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<ProductoService>();
            builder.Services.AddSingleton<CategoriaService>();
            builder.Services.AddSingleton<UsuarioService>();
            builder.Services.AddSingleton<ClienteService>();
            builder.Services.AddSingleton<ProveedorService>();
            builder.Services.AddSingleton<IngresoService>();
            builder.Services.AddSingleton<VentaService>();
            builder.Services.AddSingleton<ReporteService>();
            builder.Services.AddSingleton<LogService>();
            builder.Services.AddSingleton<CarritoService>();
            builder.Services.AddSingleton<PerfilService>();
            builder.Services.AddSingleton<AuditoriaService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Red de seguridad: cualquier excepcion no controlada se informa a la API (Origen = "App")
            // El carrito se crea al arrancar para que escuche el logout desde el principio (y se vacie)
            app.Services.GetRequiredService<CarritoService>();

            var log = app.Services.GetRequiredService<LogService>();
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex) log.Reportar(ex, "UnhandledException");
            };
            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                log.Reportar(e.Exception, "UnobservedTaskException");
                e.SetObserved();
            };

            return app;
        }
    }
}
