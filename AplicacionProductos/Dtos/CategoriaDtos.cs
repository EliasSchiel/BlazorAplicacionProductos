namespace AplicacionProductos.Dtos;

public class CategoriaDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

/// <summary>Body de POST (crear) y PUT (editar) api/Categorias.</summary>
public class CategoriaGuardarRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
