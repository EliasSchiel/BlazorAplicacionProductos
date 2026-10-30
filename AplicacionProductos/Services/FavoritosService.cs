using System.Text.Json;

namespace AplicacionProductos.Services;

/// <summary>
/// Productos marcados con el corazon. Solo se guardan los ids en Preferences (no toca la API):
/// la pantalla de Favoritos trae los datos actualizados de cada producto al abrirse.
/// Igual que el carrito, se vacia al cerrar sesion.
/// </summary>
public class FavoritosService
{
    private const string KFavoritos = "favoritos";

    private readonly AuthService _auth;
    private List<int> _ids = new();
    private bool _cargado;
    private bool _estabaLogueado;

    /// <summary>Se dispara cuando se agrega o quita un favorito (para redibujar los corazones).</summary>
    public event Action? Cambio;

    public FavoritosService(AuthService auth)
    {
        _auth = auth;
        _auth.Cambio += OnSesionCambio;
    }

    public IReadOnlyList<int> Ids
    {
        get { Cargar(); return _ids; }
    }

    public int Cantidad => Ids.Count;

    public bool EsFavorito(int idProducto) => Ids.Contains(idProducto);

    /// <summary>Agrega o quita el producto. Devuelve true si quedo como favorito.</summary>
    public bool Alternar(int idProducto)
    {
        Cargar();
        var queda = !_ids.Remove(idProducto);
        if (queda) _ids.Insert(0, idProducto);   // el ultimo marcado aparece primero
        Guardar();
        return queda;
    }

    public void Quitar(int idProducto)
    {
        Cargar();
        if (_ids.Remove(idProducto)) Guardar();
    }

    public void Vaciar()
    {
        _ids = new();
        _cargado = true;
        try { Preferences.Default.Remove(KFavoritos); } catch { }
        Cambio?.Invoke();
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
            var json = Preferences.Default.Get(KFavoritos, string.Empty);
            if (!string.IsNullOrEmpty(json))
                _ids = JsonSerializer.Deserialize<List<int>>(json) ?? new();
        }
        catch
        {
            _ids = new();
        }
    }

    private void Guardar()
    {
        try { Preferences.Default.Set(KFavoritos, JsonSerializer.Serialize(_ids)); } catch { }
        Cambio?.Invoke();
    }
}
