# AplicacionProductos – estado y traspaso

App **.NET MAUI Blazor Hybrid** (.NET 10, MudBlazor) que consume la API ya publicada:
`https://appproductossg.runasp.net/` (la URL está en `AplicacionProductos/Config/ApiConfig.cs`, debe terminar en `/`).

Requiere **Visual Studio 2026** (la 2022 no admite .NET 10). Para probar rápido: perfil **Windows Machine**. Para la demo: Android.

## Cómo está organizado (quién habla con quién)

```
Components/Pages/*.razor  ->  Services/*Service.cs  ->  Services/ApiClient.cs  ->  API (HTTP + JWT)
                                      ^                         ^
                                 Dtos/*.cs (copias de los DTOs de la API)     AuthService (token)
```

- `Services/ApiClient.cs`: único lugar que hace HTTP. Agrega el token, serializa JSON, traduce errores de la API a `ApiException` (mensaje listo para mostrar). Si la API responde 401, cierra la sesión.
- `Services/AuthService.cs`: login / registro / logout. Guarda el token en `SecureStorage` (si no está disponible, queda en memoria). Expone `EstaLogueado`, `EsStaff`, `EsAdmin`, `Rol`, `Username`.
- `Services/*Service.cs`: uno por tema (Producto, Categoria, Usuario, Cliente, Proveedor, Ingreso, Venta, Reporte, Perfil, Auditoria, Log). Solo arman la URL y llaman a `ApiClient`.
- `Services/CarritoService.cs`: el carrito del cliente. No habla con la API: guarda los items en `Preferences` (clave `carrito`), avisa con el evento `Cambio` (contador del icono) y se vacía solo cuando la sesión pasa de abierta a cerrada (logout o 401) y al confirmar una compra. Se crea al arrancar en `MauiProgram` para que escuche el logout desde el principio.
- `Services/LogService.cs`: manda los errores de la app a `POST api/Logs/app` (quedan en la tabla `LogErrores` con Origen = "App"). Los `catch` de cada pantalla lo usan, y `MauiProgram.cs` lo engancha a las excepciones no controladas.
- `MauiProgram.cs`: registra un único `HttpClient` y todos los servicios como singleton.
- `Components/Layout/`: `MainLayout` (proveedores de MudBlazor, tema, barra con icono del carrito y menú) y `NavMenu` (el menú cambia según el rol, ver abajo).
- `Config/TemaApp.cs`: tema de MudBlazor (paleta y bordes). Los estilos propios (tarjetas del catálogo, carrito, etc.) están al final de `wwwroot/app.css`.
- Todas las páginas llaman a `Auth.InicializarAsync()` antes de decidir si redirigen a `/login`.

## Pantallas

| Ruta | Quién | Qué hace |
|---|---|---|
| `/` | todos | Staff: accesos rápidos y productos con stock bajo. Cliente o sin sesión: redirige a `/catalogo` |
| `/login`, `/registro` | público | Login; registro de cliente (la API devuelve el token: queda logueado). Aceptan `?volver=/ruta` (lo usa el carrito para volver al checkout) |
| `/catalogo` | público | Catálogo tipo tienda: tarjetas con foto y precio, búsqueda, categoría, orden, "Agregar" al carrito |
| `/catalogo/{id}` | público | Detalle: fotos, descripción, stock, cantidad, "Agregar al carrito" / "Comprar ahora" |
| `/carrito` | público | Carrito: cantidades, eliminar, total. Al entrar refresca precio y stock desde la API. Para seguir pide iniciar sesión |
| `/checkout` | Cliente | Crea la venta (`POST api/Ventas`) y la paga (`POST api/Ventas/{id}/pagar`). Si el pago se rechaza, se reintenta sobre la misma venta |
| `/perfil` | Cliente | Ver y editar su ficha de cliente (`api/Perfil`) |
| `/productos` | staff | Listado de administración con búsqueda, categoría y paginación: nuevo / editar / ver inactivos (un Cliente es redirigido al catálogo) |
| `/productos/nuevo`, `/productos/{id}/editar` | staff | ABM de producto + subir / eliminar / marcar principal las imágenes (máx. 5 MB). Baja solo Admin |
| `/categorias` | staff | ABM de categorías (alta rápida también desde el formulario de producto) |
| `/clientes` | staff | ABM de fichas de clientes (eliminar solo Admin) |
| `/usuarios` | Admin | Alta de Admin / Operador, edición, baja lógica |
| `/ingresos` | staff | Registrar ingreso de mercadería (suma stock) + alta rápida de proveedor + historial |
| `/proveedores` | staff | ABM de proveedores con búsqueda (eliminar solo Admin; la API no deja si tiene ingresos) |
| `/ventas`, `/mis-compras` | todos | Staff: venta de mostrador, pagar (simulado), cancelar. Cliente: "Mis compras" (pagar pendientes, cancelar pendientes) |
| `/auditoria` | Admin | Pestañas Acciones (`api/Auditoria`) y Errores (`api/Auditoria/errores`, origen API o App) con filtros; tocando una fila se ve el detalle |
| `/reportes` | staff | PDF de stock, stock bajo, ingresos y ventas; ver o enviar por mail |
| `/cuenta` | logueado | Cambiar la propia contraseña |

### Menú por rol (`NavMenu.razor`)
- **Sin sesión:** Catálogo, Carrito, Iniciar sesión, Crear cuenta.
- **Cliente:** Catálogo, Carrito, Mis compras, Mi perfil, Mi cuenta, Cerrar sesión.
- **Operador:** Inicio, Ventas, Clientes, Productos, Categorías, Ingresos, Proveedores, Reportes, Mi cuenta.
- **Admin:** lo del Operador + Usuarios y Auditoría y errores.
- El menú solo oculta: cada pantalla igual controla el rol y la API valida todos los permisos.

## Reglas de la API que conviene saber
- El **stock no se edita en el producto**: entra con un Ingreso y sale con una Venta (al crear la venta; cancelar la devuelve).
- Pago simulado: una tarjeta que termina en `0000` se rechaza (la API responde 422). La app pide número de tarjeta (13 a 19 dígitos) solo para los métodos "Tarjeta de débito / crédito".
- La auditoría la llena la API sola (altas, modificaciones y bajas de cada entidad + Login, LoginFallido, EnvioReporte). Solo guarda `IdUsuario`: la pantalla lo traduce a nombre con `api/Usuarios`.
- Un usuario **Cliente** solo ve y crea sus propias ventas. Los usuarios Cliente se crean solo por el registro público (necesitan ficha de cliente).
- Bajas de productos y usuarios son **lógicas** (quedan inactivos).
- Imágenes: JPG / PNG / WEBP, máximo 5 MB.

## Detalles técnicos de MudBlazor en MAUI
- `_Imports.razor` define alias `Color` y `Size` de MudBlazor: sin ellos chocan con tipos de MAUI (`Microsoft.Maui.Graphics`).
- En `ProductoForm` se usa `OnParametersSetAsync` (no `OnInitialized`) porque `/productos/nuevo` y `/productos/{id}/editar` comparten el mismo componente.
- Las confirmaciones (dar de baja, eliminar, cancelar) son avisos en la misma pantalla, no diálogos.
- El query string (`?volver=`) se lee a mano con `HttpUtility.ParseQueryString`, no con `[SupplyParameterFromQuery]` (no está garantizado en Blazor Hybrid). Solo se aceptan rutas internas que empiezan con `/`.
- En `Carrito.razor` el servicio se inyecta como `CarritoSvc`: una propiedad no puede llamarse igual que la clase de la página (`Carrito`).
- `MudDrawer` es `Responsive`: fijo en pantallas grandes, superpuesto en el celular (se cierra al navegar).
- Android: `Platforms/Android/Resources/values/colors.xml` tiene los colores del tema (barra de estado oscura para que se lean los iconos).

## Estado de las pruebas
- Probado a mano en Windows: login, productos (alta, edición, imagen), categorías.
- Revisado contra el código de la API (rutas, verbos, DTOs, filtros y permisos coinciden): registro, usuarios, clientes, mi cuenta, ingresos, ventas.
- **Escrito y revisado, pero sin compilar ni probar al momento del commit** (no se pudo compilar fuera de Visual Studio): catálogo, detalle, carrito, checkout, mis compras, perfil, proveedores, auditoría, tema y menú. Si algo no compila, empezar por esas pantallas (los puntos con más riesgo son APIs de MudBlazor: opciones del `Snackbar` con botón "Ver carrito", `MudTabs`, `ChildRowContent` de `MudTable`).
