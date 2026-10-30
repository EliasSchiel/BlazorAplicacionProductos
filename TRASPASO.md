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
- `Services/*Service.cs`: uno por tema (Producto, Categoria, Usuario, Cliente, Proveedor, Ingreso, Venta, Reporte, Log). Solo arman la URL y llaman a `ApiClient`.
- `Services/LogService.cs`: manda los errores de la app a `POST api/Logs/app` (quedan en la tabla `LogErrores` con Origen = "App"). Los `catch` de cada pantalla lo usan, y `MauiProgram.cs` lo engancha a las excepciones no controladas.
- `MauiProgram.cs`: registra un único `HttpClient` y todos los servicios como singleton.
- `Components/Layout/`: `MainLayout` (proveedores de MudBlazor, barra y menú) y `NavMenu` (el menú cambia según el rol).
- Todas las páginas llaman a `Auth.InicializarAsync()` antes de decidir si redirigen a `/login`.

## Pantallas

| Ruta | Quién | Qué hace |
|---|---|---|
| `/login`, `/registro` | público | Login; registro de cliente (la API devuelve el token: queda logueado) |
| `/productos` | logueado | Catálogo con búsqueda, categoría y paginación. Staff: nuevo / editar / ver inactivos |
| `/productos/nuevo`, `/productos/{id}/editar` | staff | ABM de producto + subir / eliminar / marcar principal las imágenes (máx. 5 MB). Baja solo Admin |
| `/categorias` | staff | ABM de categorías (alta rápida también desde el formulario de producto) |
| `/clientes` | staff | ABM de fichas de clientes (eliminar solo Admin) |
| `/usuarios` | Admin | Alta de Admin / Operador, edición, baja lógica |
| `/ingresos` | staff | Registrar ingreso de mercadería (suma stock) + alta rápida de proveedor + historial |
| `/ventas` | todos | Staff: venta de mostrador, pagar (simulado), cancelar. Cliente: "Mis compras" |
| `/reportes` | staff | PDF de stock, stock bajo, ingresos y ventas; ver o enviar por mail |
| `/cuenta` | logueado | Cambiar la propia contraseña |

## Reglas de la API que conviene saber
- El **stock no se edita en el producto**: entra con un Ingreso y sale con una Venta (al crear la venta; cancelar la devuelve).
- Pago simulado: una tarjeta que termina en `0000` se rechaza (la API responde 422).
- Un usuario **Cliente** solo ve y crea sus propias ventas. Los usuarios Cliente se crean solo por el registro público (necesitan ficha de cliente).
- Bajas de productos y usuarios son **lógicas** (quedan inactivos).
- Imágenes: JPG / PNG / WEBP, máximo 5 MB.

## Detalles técnicos de MudBlazor en MAUI
- `_Imports.razor` define alias `Color` y `Size` de MudBlazor: sin ellos chocan con tipos de MAUI (`Microsoft.Maui.Graphics`).
- En `ProductoForm` se usa `OnParametersSetAsync` (no `OnInitialized`) porque `/productos/nuevo` y `/productos/{id}/editar` comparten el mismo componente.
- Las confirmaciones (dar de baja, eliminar, cancelar) son avisos en la misma pantalla, no diálogos.

## Estado de las pruebas
- Probado a mano en Windows: login, productos (alta, edición, imagen), categorías.
- **Escritos pero sin compilar ni probar al momento del commit:** registro, usuarios, clientes, mi cuenta, ingresos, ventas. Si algo no compila, empezar por ahí.

## Pendiente / ideas (por prioridad para la demo)
1. Compilar y probar las pantallas sin probar (ver arriba). Flujo completo sugerido: crear categoría -> producto -> proveedor + ingreso (sube stock) -> cliente -> venta -> pagar -> reporte de ventas por mail.
2. Probar en Android (emulador o celular): subida de imagen, abrir el PDF, `SecureStorage`.
3. Probar con un usuario **Operador** y con un **Cliente** que no vea lo que no le corresponde.
4. Pulido visual: tema y colores de MudBlazor (`MudThemeProvider` en `MainLayout`), vista de catálogo para el cliente con botón "Comprar".
5. Pantalla de Proveedores completa (hoy solo hay alta rápida desde Ingresos) y de Perfil del cliente (`api/Perfil`).
6. Opcionales: pantalla de Auditoría (`api/Auditoria`), reseteo de contraseña por Admin (falta el endpoint en la API).
7. Antes de entregar: apagar el acceso remoto a la base en MonsterASP y cambiar su contraseña.
