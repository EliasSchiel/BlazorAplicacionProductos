using MudBlazor;

namespace AplicacionProductos.Config;

/// <summary>
/// Tema de MudBlazor para toda la app (se aplica en MainLayout).
/// Estilo "tienda online": barra amarilla, botones azules, precios y estados OK en verde.
/// </summary>
public static class TemaApp
{
    public const string Amarillo = "#FFE600";

    public static readonly MudTheme Principal = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#3483FA",
            Secondary = "#00A650",
            Tertiary = "#FF7733",
            Info = "#3483FA",
            Success = "#00A650",
            Warning = "#FF7733",
            Error = "#F23D4F",
            AppbarBackground = Amarillo,
            AppbarText = "#333333",
            Background = "#F5F5F5",
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#333333",
            DrawerIcon = "#3483FA",
            TextPrimary = "#333333",
            TextSecondary = "#737373",
            ActionDefault = "#555555"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "250px"
        }
    };
}
