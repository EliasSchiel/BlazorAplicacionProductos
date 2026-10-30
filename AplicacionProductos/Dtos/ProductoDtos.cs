namespace AplicacionProductos.Dtos;

public class ProductoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool Disponible { get; set; }
    public bool StockBajo { get; set; }
    public int IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? ImagenPrincipalUrl { get; set; }
    public List<ProductoImagenDto> Imagenes { get; set; } = new();
    public bool Activo { get; set; }
}

public class ProductoImagenDto
{
    public int IdProductoImagen { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
    public int Orden { get; set; }
}

/// <summary>Body de POST (crear) y PUT (editar) api/Productos. El stock no se carga acá: entra con un Ingreso.</summary>
public class ProductoGuardarRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int StockMinimo { get; set; }
    public int IdCategoria { get; set; }
    public bool Activo { get; set; } = true;   // solo lo usa el PUT
}
