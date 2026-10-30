using System.Text.Json;
using AplicacionProductos.Dtos;

namespace AplicacionProductos.Services;

/// <summary>Un renglon del carrito. Precio y stock son los ultimos que mostro la API (el precio final lo pone el servidor).</summary>
public class CarritoItem
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public int Cantidad { get; set; }
    public int Stock { get; set; }
    public decimal Subtotal => Precio * Cantidad;
}

/// <summary>
/// Carrito del cliente. Vive en memoria y se guarda en Preferences (sobrevive a cerrar la app).
/// Se vacia solo cuando se cierra la sesion (logout manual o token vencido) y despues de confirmar una compra.
/// </summary>
public class CarritoService
{
    private const string KCarrito = "carrito";

    private readonly AuthService _auth;
    private List<CarritoItem> _items = new();
    private bool _cargado;
    private bool _estabaLogueado;

    /// <summary>Se dispara cuando cambia el contenido (para el contador del icono del carrito).</summary>
    public event Action? Cambio;

    public CarritoService(AuthService auth)
    {
        _auth = auth;
        _auth.Cambio += OnSesionCambio;
    }

    public IReadOnlyList<CarritoItem> Items
    {
        get { Cargar(); return _items; }
    }

    public int CantidadTotal => Items.Sum(i => i.Cantidad);
    public decimal Total => Items.Sum(i => i.Subtotal);
    public bool EstaVacio => Items.Count == 0;

    public int CantidadDe(int idProducto) => Items.FirstOrDefault(i => i.IdProducto == idProducto)?.Cantidad ?? 0;

    /// <summary>Suma unidades de un producto (sin pasarse del stock). Devuelve la cantidad que quedo en el carrito.</summary>
    public int Agregar(ProductoDto p, int cantidad = 1)
    {
        Cargar();
        var item = _items.FirstOrDefault(i => i.IdProducto == p.IdProducto);
        if (item is null)
        {
            item = new CarritoItem { IdProducto = p.IdProducto };
            _items.Add(item);
        }
        CopiarDatos(item, p);
        item.Cantidad = Math.Clamp(item.Cantidad + cantidad, 1, Math.Max(p.Stock, 1));
        Guardar();
        return item.Cantidad;
    }

    public void CambiarCantidad(int idProducto, int cantidad)
    {
        Cargar();
        var item = _items.FirstOrDefault(i => i.IdProducto == idProducto);
        if (item is null) return;
        if (cantidad <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.Cantidad = item.Stock > 0 ? Math.Min(cantidad, item.Stock) : cantidad;
        }
        Guardar();
    }

    /// <summary>Refresca nombre, precio y stock con lo que devolvio la API. Si ya no alcanza el stock, baja la cantidad.</summary>
    public void Actualizar(ProductoDto p)
    {
        Cargar();
        var item = _items.FirstOrDefault(i => i.IdProducto == p.IdProducto);
        if (item is null) return;
        CopiarDatos(item, p);
        if (item.Cantidad > p.Stock) item.Cantidad = p.Stock;
        if (item.Cantidad <= 0 || !p.Activo) _items.Remove(item);
        Guardar();
    }

    public void Quitar(int idProducto) => CambiarCantidad(idProducto, 0);

    public void Vaciar()
    {
        _items = new();
        _cargado = true;
        try { Preferences.Default.Remove(KCarrito); } catch { }
        Cambio?.Invoke();
    }

    public List<VentaItemRequest> ArmarItemsVenta()
        => Items.Select(i => new VentaItemRequest { IdProducto = i.IdProducto, Cantidad = i.Cantidad }).ToList();

    // ---------- internos ----------

    private static void CopiarDatos(CarritoItem item, ProductoDto p)
    {
        item.Nombre = p.Nombre;
        item.Precio = p.Precio;
        item.Stock = p.Stock;
        item.ImagenUrl = p.ImagenPrincipalUrl;
    }

    // Solo se vacia cuando la sesion pasa de abierta a cerrada (no al arrancar la app sin sesion)
    private void OnSesionCambio()
    {
        if (_estabaLogueado && !_auth.EstaLogueado) Vaciar();
        _estabaLogueado = _auth.EstaLogueado;
    }

    private void Cargar()
    {
        if (_cargado) return;
        _cargado = true;
        try
        {
            var json = Preferences.Default.Get(KCarrito, string.Empty);
            if (!string.IsNullOrEmpty(json))
                _items = JsonSerializer.Deserialize<List<CarritoItem>>(json) ?? new();
        }
        catch
        {
            _items = new();   // datos corruptos o Preferences no disponible: se arranca con el carrito vacio
        }
    }

    private void Guardar()
    {
        try { Preferences.Default.Set(KCarrito, JsonSerializer.Serialize(_items)); } catch { }
        Cambio?.Invoke();
    }
}
