# FlotaVehicular — API de gestión de flota y mantenimiento de vehículos

Proyecto universitario independiente (primera entrega). API REST para el mantenimiento
preventivo/correctivo de una flota vehicular: categorías, vehículos, imágenes, repuestos,
proveedores y la relación repuesto–proveedor.

Arquitectura por capas basada en el mismo patrón del proyecto base (ERP9):
repositorios, servicios, DTOs, Mapster y envoltorio `Result<T>` — sin modificar el proyecto base.

## Stack

| Componente | Versión |
|---|---|
| .NET | 10 (net10.0) |
| ASP.NET Core Web API | 10 |
| Entity Framework Core | 10.0.13 |
| SQL Server | Local (`localhost`) |
| Mapster | mapeo Entity ↔ DTO |
| Scalar | documentación OpenAPI en desarrollo |

## Estructura de la solución

```
FlotaVehicular.slnx
├── FlotaVehicular.Domain          Entidades, DTOs y Result<T>
├── FlotaVehicular.Infrastructure  AppDbContext, configuraciones EF, repositorios, seeder, almacenamiento de imágenes
├── FlotaVehicular.Application     Interfaces de servicios, implementaciones y mapeos Mapster
└── FlotaVehicular.WebAPI          Program.cs, controladores, appsettings, wwwroot/uploads/vehicles
```

Cada módulo (categoría, vehículo, imagen, repuesto, proveedor, asociación) tiene:
entidad → configuración EF → interfaz/repositorio → interfaz/servicio → DTOs → mapeos → controlador.

La carpeta `verificacion/` contiene los scripts reproducibles de pruebas (ver
[Cómo reproducir la verificación](#cómo-reproducir-la-verificación)).

## Requisitos

- .NET SDK 10
- SQL Server local (Express/Developer). Se usa autenticación de Windows.

## Configuración de la base de datos

La cadena de conexión vive en `FlotaVehicular.WebAPI/appsettings.json` (sin credenciales — autenticación de Windows):

```
Server=localhost;Database=FlotaVehicular;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False
```

Se puede sobrescribir sin tocar el código:

- **Variable de entorno** (recomendado para pruebas):
  ```powershell
  $env:ConnectionStrings__FlotaVehicular = 'Server=localhost;Database=FlotaVehicular_RevVerify;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False'
  ```
- **user-secrets** (recomendado si en el futuro hay credenciales de SQL):
  ```powershell
  cd FlotaVehicular.WebAPI
  dotnet user-secrets set "ConnectionStrings:FlotaVehicular" "Server=localhost;Database=FlotaVehicular;User Id=...;Password=...;TrustServerCertificate=True"
  ```

> `TrustServerCertificate=True` es necesario en el equipo porque el certificado autofirmado
> de SQL Server local no está en el almacén de confianza.

El almacén de imágenes se configura con `Storage:VehicleImagesPath` (por defecto
`wwwroot/uploads/vehicles`). Las pruebas usan una ruta temporal aparte.

## Migración — estado real

**La migración inicial existe y está aplicada.** Este es el estado verificado:

| Verificación | Resultado |
|---|---|
| Migraciones en el proyecto | Una sola: `FlotaVehicular.Infrastructure\Migrations\20261019020509_InitialCreate.cs` |
| Aplicada en la BD `FlotaVehicular` | Sí — `__EFMigrationsHistory` → `20261019020509_InitialCreate \| 10.0.13` |
| Deriva modelo ↔ migraciones | Ninguna — `dotnet ef migrations has-pending-model-changes` → *"No changes have been made to the model since the last migration."* |
| Datos de ejemplo | La semilla crea 10 vehículos, 10 categorías, 10 repuestos, 10 proveedores y 23 asociaciones. Las pruebas manuales pueden añadir registros. |

### Ajustes de esquema para el otro integrante

**No hay ningún ajuste pendiente.** Tras la creación de `InitialCreate` no se modificó el
esquema (la corrección de entidades `record class` → `class` descrita más abajo no altera
tablas, columnas ni índices: por eso `has-pending-model-changes` no reporta nada). Para
integrar la parte de técnicos/órdenes basta con:

```powershell
# Aplicar el esquema existente a la BD del compañero (idempotente)
dotnet ef database update --startup-project FlotaVehicular.WebAPI
```

y luego generar **nuevas migraciones encima** de `InitialCreate` para las tablas de los
módulos futuros:

```powershell
# NO recrear ni modificar 20261019020509_InitialCreate
dotnet ef migrations add <NombreDelModulo> --project FlotaVehicular.Infrastructure --startup-project FlotaVehicular.WebAPI
dotnet ef database update --startup-project FlotaVehicular.WebAPI
```

Notas de integración:

- El `AppDbContext` ya está configurado con `UseSeeding` / `UseAsyncSeeding`
  (`FleetSeeder`): la semilla corre automáticamente al aplicar la migración y es
  **idempotente** (verificado: dos `database update` seguidos sobre una BD nueva →
  siempre 10/10/10/10/23 registros).
- Las configuraciones EF de los 6 módulos de esta entrega ya están en `AppDbContext`
  (`VehicleCategory`, `Vehicle`, `VehicleImage`, `SparePart`, `Supplier`, `SparePartSupplier`
  con índices únicos, `decimal(18,2)` para dinero, identidades SQL Server y `CHECK` de stock).
- El arranque de la API **no** ejecuta migraciones ni `EnsureCreated` (patrón acordado).

## Ejecutar

```powershell
cd C:\Web\FlotaVehicular
dotnet build FlotaVehicular.slnx        # 0 errores / 0 advertencias
cd FlotaVehicular.WebAPI
dotnet run                              # http://localhost:5137
```

En desarrollo quedan disponibles:

- OpenAPI: `GET http://localhost:5137/openapi/v1.json`
- Scalar (UI): `http://localhost:5137/scalar`

## Semilla de datos (FleetSeeder)

Se ejecuta con la migración (ver arriba). Es **idempotente** y crea:

| Tabla | Registros |
|---|---|
| Categorías | 10 (Camión de carga, Camioneta, Automóvil de pasajeros, …) |
| Vehículos | 10 (5 Operativo, 2 EnMantenimiento, 1 FueraDeServicio, 2 DadoDeBaja) |
| Repuestos | 10 (REP-001 … REP-010; REP-009 queda por debajo del stock mínimo) |
| Proveedores | 10 (todos activos) |
| Asociaciones repuesto–proveedor | 23; los 10 repuestos tienen 2 o 3 proveedores |

## Convención de respuestas

Todas las respuestas (incluidos errores de validación automáticos de `[ApiController]`)
usan el mismo envoltorio:

```json
{
  "isSuccess": true,
  "status": 200,
  "title": "OK",
  "detail": "...",
  "data": { }
}
```

Códigos usados: `200` OK · `201` Created · `400` validación · `404` inexistente ·
`409` conflicto de integridad (duplicados, borrados con referencias) ·
`410` Gone (archivo de imagen borrado del disco) · `413` archivo mayor a 5 MB ·
`500` error interno con compensación (solo en fallo de BD durante reemplazo de imagen).

Los enums (`VehicleStatus`) se envían/reciben **como texto y de forma estricta**:
`"Operativo"`, `"EnMantenimiento"`, `"FueraDeServicio"`, `"DadoDeBaja"`.
`JsonStringEnumConverter` está configurado con `allowIntegerValues: false`, de modo que
`{"status":99}` o `{"status":"NoExiste"}` rechazan en el propio enlace JSON con `400`
(en forma `Result<T>`), y el servicio además valida con `Enum.IsDefined` como
defensa en profundidad.

Las listas devuelven `200` con arreglo vacío cuando el filtro no coincide (nunca 404).

## Endpoints

### Categorías — `/api/vehiclecategory`

| Método | Ruta | Notas |
|---|---|---|
| GET | `/api/vehiclecategory?search=cam` | búsqueda por nombre |
| GET | `/api/vehiclecategory/paged?pageNumber=1&pageSize=10` | paginación |
| GET | `/api/vehiclecategory/{id}` | incluye `vehicleCount` |
| POST | `/api/vehiclecategory` | 409 si el nombre ya existe |
| PUT | `/api/vehiclecategory/{id}` | |
| DELETE | `/api/vehiclecategory/{id}` | 409 si tiene vehículos (RESTRICT, incluso dados de baja) |

### Vehículos — `/api/vehicle`

| Método | Ruta | Notas |
|---|---|---|
| GET | `/api/vehicle` | filtros: `search` (código, placa, nombre, marca, modelo), `status`, `idVehicleCategory`, `includeDeactivated` |
| GET | `/api/vehicle/paged?pageNumber=1&pageSize=10` | mismos filtros + paginación |
| GET | `/api/vehicle/{id}` | incluye categoría, imágenes e `imageCount`; calcula `kmToNextPreventiveMaintenance` |
| POST | `/api/vehicle` | nace `Operativo`; 409 por código o placa duplicados; 400 por categoría inexistente, fecha futura o preventivo > km actual |
| PUT | `/api/vehicle/{id}` | 409 si está dado de baja |
| PUT | `/api/vehicle/{id}/status` | cuerpo `{"status":"EnMantenimiento"}`; valores fuera del enum → 400; `DadoDeBaja` → 400 (solo vía DELETE) |
| DELETE | `/api/vehicle/{id}` | baja **lógica**: estado `DadoDeBaja` + fecha; conserva categoría e imágenes; idempotente |

### Imágenes — `/api/vehicle/{vehicleId}/images`

Subida/consulta/reemplazo/borrado de **archivos reales** en el almacén configurable
(`Storage:VehicleImagesPath`; por defecto `wwwroot/uploads/vehicles`; protegido contra
path traversal).

| Método | Ruta | Notas |
|---|---|---|
| GET | `/api/vehicle/{id}/images` | lista (principal primero) |
| POST | `/api/vehicle/{id}/images` | `multipart/form-data`: campo `file` + `isMain` opcional; 201 |
| GET | `/api/vehicle/{id}/images/{imageId}/file` | descarga el binario original; 410 si el archivo ya no está en disco |
| PUT | `/api/vehicle/{id}/images/{imageId}` | reemplaza el archivo físico y conserva el registro (mismo Id) |
| DELETE | `/api/vehicle/{id}/images/{imageId}` | borra registro y archivo |

Validación de contenido (aplica en **subida y reemplazo**), en este orden:

1. Tamaño ≤ **5 MB** → si lo supera, `413`.
2. **Firma binaria real** (magic bytes): PNG, JPEG, WEBP o GIF → si no coincide, `400`
   *"firma de formato no reconocida"* (atrapa archivos de texto con extensión engañosa).
3. La firma debe coincidir con la **extensión y el content-type declarados** → si no, `400`
   (atrapa un JPEG declarado como `image/png`).
4. **Marca de fin de archivo** (p. ej. IEND en PNG, FFD9 en JPEG) → si falta, `400`
   *"está corrupto o incompleto"* (atrapa PNG truncados).

La primera imagen subida queda como principal; el reemplazo mantiene la coherencia de la
imagen principal y de `Vehicle.ImagePath`.

### Repuestos — `/api/sparepart`

| Método | Ruta | Notas |
|---|---|---|
| GET | `/api/sparepart?search=filtro&belowMinimumOnly=true` | filtros: `search` (código o nombre), `belowMinimumOnly` |
| GET | `/api/sparepart/paged` | paginación |
| GET | `/api/sparepart/below-minimum-stock` | repuestos con `currentStock < minStock` |
| GET | `/api/sparepart/{id}` | incluye proveedores (`supplierCount` + colección con precio y días) |
| POST | `/api/sparepart` | 409 por código duplicado; stock negativo → 400 (además, CHECK en BD) |
| PUT | `/api/sparepart/{id}` | |
| DELETE | `/api/sparepart/{id}` | 409 si tiene proveedores asociados |

### Proveedores — `/api/supplier`

| Método | Ruta | Notas |
|---|---|---|
| GET | `/api/supplier?search=norte&includeInactive=true` | solo activos; `includeInactive=true` incluye dados de baja; `search` por nombre |
| GET | `/api/supplier/paged` | paginación |
| GET | `/api/supplier/{id}` | incluye `sparePartCount` + colección |
| POST | `/api/supplier` | 409 por nombre duplicado |
| PUT | `/api/supplier/{id}` | |
| PUT | `/api/supplier/{id}/deactivate` | baja lógica; conserva sus repuestos |
| PUT | `/api/supplier/{id}/activate` | reactivación |
| DELETE | `/api/supplier/{id}` | 409 si tiene repuestos asociados |

### Repuesto – Proveedor — `/api/sparepartsupplier`

| Método | Ruta | Notas |
|---|---|---|
| GET | `/api/sparepartsupplier?idSparePart=1` | filtros: `idSparePart`, `idSupplier`, `search`; trae datos del repuesto y del proveedor |
| GET | `/api/sparepartsupplier/paged` | paginación global |
| GET | `/api/sparepartsupplier/{idSparePart}/{idSupplier}` | 404 si la pareja no existe |
| POST | `/api/sparepartsupplier` | 201; 409 si la pareja ya existe; 404 si el repuesto/proveedor no existe; 400 si el proveedor está desactivado |
| PUT | `/api/sparepartsupplier/{idSparePart}/{idSupplier}` | actualiza `offeredPrice` y `estimatedDeliveryDays` |
| DELETE | `/api/sparepartsupplier/{idSparePart}/{idSupplier}` | desasocia (no borra ni el repuesto ni el proveedor) |

El precio se almacena como `decimal(18,2)` en la base de datos.

---

## Solicitudes ejecutables

Todos los ejemplos usan `curl.exe` (incluido en Windows) contra `http://localhost:5137`
y están escritos para ejecutarse sobre una BD recién sembrada (los IDs creados en los
ejemplos son 11 en adelante: la semilla ocupa 1–10). Se indican el **cuerpo completo** y
el **resultado esperado** de cada solicitud; los estados listados fueron los obtenidos
en la verificación del 18/10/2026 (ver [Evidencia](#evidencia-de-verificación)).

### Categorías

```bash
# Crear → 201 con data.idVehicleCategory=11, vehicleCount=0
curl -X POST http://localhost:5137/api/vehiclecategory -H "Content-Type: application/json" \
  -d '{"name":"Categoria TEST-VERIFY","description":"creada por la bateria de verificacion","preventiveMaintenanceIntervalKm":10000}'

# Mismo nombre otra vez → 409 {"isSuccess":false,...,"detail":"Ya existe una categoría con el nombre 'Categoria TEST-VERIFY'."}
curl -X POST http://localhost:5137/api/vehiclecategory -H "Content-Type: application/json" \
  -d '{"name":"Categoria TEST-VERIFY","description":"x","preventiveMaintenanceIntervalKm":10000}'

# Editar → 200; GET después conserva description e intervalo (persistencia real)
curl -X PUT http://localhost:5137/api/vehiclecategory/11 -H "Content-Type: application/json" \
  -d '{"name":"Categoria TEST-VERIFY v2","description":"editada","preventiveMaintenanceIntervalKm":12000}'
curl http://localhost:5137/api/vehiclecategory/11
#   -> data.description="editada", data.preventiveMaintenanceIntervalKm=12000

# Búsqueda y paginación
curl "http://localhost:5137/api/vehiclecategory?search=TEST-VERIFY"     # 200, >= 1 resultados
curl "http://localhost:5137/api/vehiclecategory/paged?pageNumber=1&pageSize=3"
#   -> 200, data con 3 ítems, pageSize=3, totalPages >= 4 (10 categorías sembradas + 1 creada)

# GET inexistente -> 404
curl http://localhost:5137/api/vehiclecategory/9999

# DELETE con vehículos (incluso dados de baja) -> 409 (RESTRICT)
curl -X DELETE http://localhost:5137/api/vehiclecategory/1
# DELETE sin vehículos (la creada en el paso anterior) -> 200
curl -X DELETE http://localhost:5137/api/vehiclecategory/11
```

### Vehículos

```bash
# Crear -> 201, nace con status="Operativo"
curl -X POST http://localhost:5137/api/vehicle -H "Content-Type: application/json" \
  -d '{"code":"VER-900","licensePlate":"VER-900-A","name":"Vehiculo de verificacion","acquisitionDate":"2024-01-15","referenceValue":15000,"idVehicleCategory":1,"brand":"TestBrand","model":"ModelX","year":2023,"currentMileage":1000,"lastPreventiveMaintenanceMileage":800}'

# Código repetido (mismo cuerpo otra vez) -> 409 {"detail":"Ya existe un vehículo con el código 'VER-900'."}
curl -X POST http://localhost:5137/api/vehicle -H "Content-Type: application/json" \
  -d '{"code":"VER-900","licensePlate":"VER-900-A","name":"Vehiculo de verificacion","acquisitionDate":"2024-01-15","referenceValue":15000,"idVehicleCategory":1,"brand":"TestBrand","model":"ModelX","year":2023,"currentMileage":1000,"lastPreventiveMaintenanceMileage":800}'

# Código repetido con otra placa -> 409 {"detail":"Ya existe un vehículo con el código 'VER-900'."}
curl -X POST http://localhost:5137/api/vehicle -H "Content-Type: application/json" \
  -d '{"code":"VER-900","licensePlate":"VER-901-A","name":"Vehiculo de verificacion","acquisitionDate":"2024-01-15","referenceValue":15000,"idVehicleCategory":1,"brand":"TestBrand","model":"ModelX","year":2023,"currentMileage":1000,"lastPreventiveMaintenanceMileage":800}'

# Placa repetida con código nuevo -> 409 {"detail":"Ya existe un vehículo con la placa 'VER-900-A'."}
curl -X POST http://localhost:5137/api/vehicle -H "Content-Type: application/json" \
  -d '{"code":"VER-901","licensePlate":"VER-900-A","name":"x","acquisitionDate":"2024-01-15","referenceValue":1000,"idVehicleCategory":1,"brand":"Marca","model":"Modelo","year":2023,"currentMileage":100,"lastPreventiveMaintenanceMileage":50}'

# Categoría inexistente -> 400 {"detail":"La categoría 9999 no existe."}
curl -X POST http://localhost:5137/api/vehicle -H "Content-Type: application/json" \
  -d '{"code":"VER-902","licensePlate":"VER-902-A","name":"Vehiculo de verificacion","acquisitionDate":"2024-01-15","referenceValue":15000,"idVehicleCategory":9999,"brand":"TestBrand","model":"ModelX","year":2023,"currentMileage":1000,"lastPreventiveMaintenanceMileage":800}'

# Fecha de adquisición futura -> 400 {"detail":"La fecha de adquisición no puede ser futura."}
curl -X POST http://localhost:5137/api/vehicle -H "Content-Type: application/json" \
  -d '{"code":"FUT-1","licensePlate":"FUT-1-A","name":"Fecha futura","acquisitionDate":"2099-12-31","referenceValue":1000,"idVehicleCategory":1,"brand":"Marca","model":"Modelo","year":2024,"currentMileage":0,"lastPreventiveMaintenanceMileage":0}'

# Kilometraje preventivo > actual -> 400 {"detail":"El kilometraje del último mantenimiento preventivo no puede superar el kilometraje actual."}
curl -X POST http://localhost:5137/api/vehicle -H "Content-Type: application/json" \
  -d '{"code":"KM-1","licensePlate":"KM-1-A","name":"Km invalido","acquisitionDate":"2024-01-01","referenceValue":1000,"idVehicleCategory":1,"brand":"Marca","model":"Modelo","year":2024,"currentMileage":500,"lastPreventiveMaintenanceMileage":900}'

# Editar -> 200 y los campos mapeados persisten (verificado con GET posterior)
curl -X PUT http://localhost:5137/api/vehicle/11 -H "Content-Type: application/json" \
  -d '{"code":"VER-900","licensePlate":"VER-900-A","name":"Vehiculo de verificacion EDITADO","acquisitionDate":"2024-01-15","referenceValue":15000,"idVehicleCategory":1,"brand":"MarcaEditada","model":"ModelX","year":2023,"currentMileage":2222,"lastPreventiveMaintenanceMileage":800}'
curl http://localhost:5137/api/vehicle/11
#   -> data.name="Vehiculo de verificacion EDITADO", data.brand="MarcaEditada", data.currentMileage=2222

# Cambio de estado
curl -X PUT http://localhost:5137/api/vehicle/11/status -H "Content-Type: application/json" \
  -d '{"status":"FueraDeServicio"}'                              # -> 200
curl -X PUT http://localhost:5137/api/vehicle/1/status -H "Content-Type: application/json" \
  -d '{"status":99}'                                             # -> 400 (entero rechazado en el enlace JSON, forma Result<T>)
curl -X PUT http://localhost:5137/api/vehicle/1/status -H "Content-Type: application/json" \
  -d '{"status":"NoExiste"}'                                     # -> 400 (valor fuera del enum)
curl -X PUT http://localhost:5137/api/vehicle/1/status -H "Content-Type: application/json" \
  -d '{"status":"DadoDeBaja"}'                                   # -> 400 {"detail":"Para dar de baja un vehículo use DELETE /api/vehicle/{id} (baja lógica)."}
curl -X PUT http://localhost:5137/api/vehicle/9999/status -H "Content-Type: application/json" \
  -d '{"status":"Operativo"}'                                    # -> 404

# Filtros y paginación
curl "http://localhost:5137/api/vehicle?search=VER-900"                 # 200, encuentra el creado
curl "http://localhost:5137/api/vehicle?status=EnMantenimiento"         # 200 (puede traer lista vacía)
curl "http://localhost:5137/api/vehicle?idVehicleCategory=11"           # 200, solo vehículos de esa categoría
curl "http://localhost:5137/api/vehicle/paged?pageNumber=1&pageSize=4"
#   -> 200, 4 ítems; totalRecords == total de vehículos activos (excluye dados de baja)

# Baja lógica -> 200; la repetición es idempotente -> 200
curl -X DELETE http://localhost:5137/api/vehicle/11
curl -X DELETE http://localhost:5137/api/vehicle/11
curl http://localhost:5137/api/vehicle/11                          # -> 200, data.status="DadoDeBaja"
curl "http://localhost:5137/api/vehicle?search=VER-900"              # -> 200, lista vacía (excluido)
curl "http://localhost:5137/api/vehicle?search=VER-900&includeDeactivated=true"   # -> 200, lo muestra
curl -X PUT http://localhost:5137/api/vehicle/11/status -H "Content-Type: application/json" \
  -d '{"status":"Operativo"}'                                     # -> 409 (dato de baja no se reactiva por status)
```

### Imágenes

```bash
# Subida válida (PNG real) -> 201 con data.fileName, data.isMain=true (si es la primera)
curl -X POST http://localhost:5137/api/vehicle/2/images \
  -F "file=@verificacion/archivos-prueba/real.png;type=image/png" -F "isMain=true"

# JPEG y GIF reales -> 201
curl -X POST http://localhost:5137/api/vehicle/2/images \
  -F "file=@verificacion/archivos-prueba/real.jpg;type=image/jpeg" -F "isMain=false"
curl -X POST http://localhost:5137/api/vehicle/2/images \
  -F "file=@verificacion/archivos-prueba/real.gif;type=image/gif" -F "isMain=false"

# Texto plano con nombre .png -> 400 "firma de formato no reconocida"
curl -X POST http://localhost:5137/api/vehicle/2/images \
  -F "file=@verificacion/archivos-prueba/fake.png;type=image/png"

# PNG truncado (sin marca IEND) -> 400 "corrupto o incompleto"
curl -X POST http://localhost:5137/api/vehicle/2/images \
  -F "file=@verificacion/archivos-prueba/corrupt.png;type=image/png"

# JPEG declarado como image/png -> 400 (el contenido real no coincide con lo declarado)
curl -X POST http://localhost:5137/api/vehicle/2/images \
  -F "file=@verificacion/archivos-prueba/real.jpg;type=image/png"

# Archivo de 6 MB -> 413 Payload Too Large
curl -X POST http://localhost:5137/api/vehicle/2/images \
  -F "file=@verificacion/archivos-prueba/big.png;type=image/png"

# Vehículo inexistente -> 404
curl -X POST http://localhost:5137/api/vehicle/9999/images \
  -F "file=@verificacion/archivos-prueba/real.png;type=image/png"

# Reemplazo válido -> 200, conserva el mismo idVehicleImage; el archivo viejo se borra
# recién después de persistir en BD; la imagen principal sigue coherente con imagePath
curl -X PUT http://localhost:5137/api/vehicle/2/images/{imageId} \
  -F "file=@verificacion/archivos-prueba/real.jpg;type=image/jpeg"

# Reemplazo con contenido falso -> 400 y el archivo actual queda intacto
curl -X PUT http://localhost:5137/api/vehicle/2/images/{imageId} \
  -F "file=@verificacion/archivos-prueba/fake.png;type=image/png"

# Descarga del binario -> 200 image/png, bytes idénticos a los subidos
curl -o salida.png http://localhost:5137/api/vehicle/2/images/{imageId}/file

# Si el archivo se borró del disco -> 410 Gone
# {"detail":"El archivo de la imagen ya no está disponible en el servidor."}

# Borrar imagen -> 200 (registro y archivo)
curl -X DELETE http://localhost:5137/api/vehicle/2/images/{imageId}
```

Los archivos `fake.png`, `corrupt.png` y `big.png` (y los válidos) se generan con
`powershell -File verificacion/Nuevos-Archivos-Prueba.ps1` (no van en el repositorio:
`big.png` pesa 6 MB).

### Repuestos

```bash
# Crear -> 201
curl -X POST http://localhost:5137/api/sparepart -H "Content-Type: application/json" \
  -d '{"code":"VER-SP-1","name":"Repuesto verificacion","unitOfMeasure":"unidad","currentStock":5,"minStock":2}'

# Código duplicado -> 409 (mismo cuerpo que el alta)
curl -X POST http://localhost:5137/api/sparepart -H "Content-Type: application/json" \
  -d '{"code":"VER-SP-1","name":"Repuesto verificacion","unitOfMeasure":"unidad","currentStock":5,"minStock":2}'

# Stock negativo -> 400 con error por campo:
# {"detail":"Uno o más datos no son válidos.","data":{"CurrentStock":["El stock actual no puede ser negativo."]}}
curl -X POST http://localhost:5137/api/sparepart -H "Content-Type: application/json" \
  -d '{"code":"NEG-1","name":"Stock negativo","unitOfMeasure":"unidad","currentStock":-1,"minStock":0}'

# Editar -> 200; los campos mapeados persisten (verificado con GET posterior)
curl -X PUT http://localhost:5137/api/sparepart/11 -H "Content-Type: application/json" \
  -d '{"code":"VER-SP-1","name":"Repuesto verificacion EDITADO","unitOfMeasure":"litro","currentStock":7,"minStock":3}'

# Repuesto bajo mínimo (se crea con stock 1 y mínimo 5) -> 201; aparece en ambas listas
curl -X POST http://localhost:5137/api/sparepart -H "Content-Type: application/json" \
  -d '{"code":"VER-SP-2","name":"Repuesto bajo minimo","unitOfMeasure":"unidad","currentStock":1,"minStock":5}'
curl "http://localhost:5137/api/sparepart/below-minimum-stock"        # 200, incluye REP-009 y VER-SP-2
curl "http://localhost:5137/api/sparepart?belowMinimumOnly=true"      # 200, mismo efecto de filtro

# GET inexistente -> 404 {"detail":"El repuesto 9999 no existe."}
curl http://localhost:5137/api/sparepart/9999

# DELETE con proveedores asociados -> 409 (RESTRICT)
curl -X DELETE http://localhost:5137/api/sparepart/1
# (el mismo endpoint responde 200 sin asociaciones: se demuestra al final de la sección de asociaciones)
```

### Proveedores

```bash
# Crear -> 201
curl -X POST http://localhost:5137/api/supplier -H "Content-Type: application/json" \
  -d '{"name":"Proveedor VER-TEST","contact":"contacto@vertest.test","address":"Calle Test 123","generalConditions":"Neto 30 dias"}'

# Nombre duplicado -> 409 (mismo cuerpo que el alta)
curl -X POST http://localhost:5137/api/supplier -H "Content-Type: application/json" \
  -d '{"name":"Proveedor VER-TEST","contact":"contacto@vertest.test","address":"Calle Test 123","generalConditions":"Neto 30 dias"}'

# Editar -> 200; dirección y condiciones persisten (verificado con GET posterior)
curl -X PUT http://localhost:5137/api/supplier/11 -H "Content-Type: application/json" \
  -d '{"name":"Proveedor VER-TEST","contact":"editado@vertest.test","address":"Avenida Editada 45","generalConditions":"Contado 60 dias"}'

# Desactivar -> 200 con status=false; el listado por defecto lo excluye
curl -X PUT http://localhost:5137/api/supplier/11/deactivate
curl "http://localhost:5137/api/supplier"                              # 200, no incluye al 11
curl "http://localhost:5137/api/supplier?includeInactive=true"         # 200, sí lo incluye

# Reactivar -> 200 con status=true
curl -X PUT http://localhost:5137/api/supplier/11/activate

# GET inexistente -> 404 ; DELETE con repuestos asociados -> 409 (RESTRICT)
curl http://localhost:5137/api/supplier/9999
curl -X DELETE http://localhost:5137/api/supplier/1
# (el mismo endpoint responde 200 sin asociaciones: se demuestra al final de la sección de asociaciones)
```

### Asociaciones repuesto – proveedor

```bash
# Crear -> 201
curl -X POST http://localhost:5137/api/sparepartsupplier -H "Content-Type: application/json" \
  -d '{"idSparePart":11,"idSupplier":11,"offeredPrice":15.75,"estimatedDeliveryDays":7}'

# Misma pareja otra vez -> 409
curl -X POST http://localhost:5137/api/sparepartsupplier -H "Content-Type: application/json" \
  -d '{"idSparePart":11,"idSupplier":11,"offeredPrice":10,"estimatedDeliveryDays":3}'

# Repuesto inexistente -> 404/400 ; proveedor inexistente -> 404/400
curl -X POST http://localhost:5137/api/sparepartsupplier -H "Content-Type: application/json" \
  -d '{"idSparePart":9999,"idSupplier":11,"offeredPrice":10,"estimatedDeliveryDays":3}'
curl -X POST http://localhost:5137/api/sparepartsupplier -H "Content-Type: application/json" \
  -d '{"idSparePart":11,"idSupplier":9999,"offeredPrice":10,"estimatedDeliveryDays":3}'

# Proveedor desactivado -> 400 (se desactiva el 11, se intenta asociarlo a otro repuesto sembrado y se reactiva)
curl -X PUT http://localhost:5137/api/supplier/11/deactivate
curl -X POST http://localhost:5137/api/sparepartsupplier -H "Content-Type: application/json" \
  -d '{"idSparePart":2,"idSupplier":11,"offeredPrice":5,"estimatedDeliveryDays":2}'
#   -> 400 {"detail":"El proveedor 11 está desactivado; reactivelo antes de asociarlo."}
curl -X PUT http://localhost:5137/api/supplier/11/activate

# Actualizar precio y días -> 200 con los valores nuevos
curl -X PUT http://localhost:5137/api/sparepartsupplier/11/11 -H "Content-Type: application/json" \
  -d '{"offeredPrice":20.5,"estimatedDeliveryDays":10}'

# Filtros independientes y combinados
curl "http://localhost:5137/api/sparepartsupplier?idSparePart=11"     # 200, todos del repuesto 11
curl "http://localhost:5137/api/sparepartsupplier?idSupplier=11"      # 200, todos del proveedor 11
curl "http://localhost:5137/api/sparepartsupplier?idSparePart=11&idSupplier=11"   # 200, exactamente 1

# Coherencia de detalles: los contadores coinciden con las colecciones y traen
# offeredPrice + estimatedDeliveryDays de cada asociación
curl http://localhost:5137/api/sparepart/11     # data.supplierCount == data.suppliers.length
curl http://localhost:5137/api/supplier/11      # data.sparePartCount == data.spareParts.length

# Desasociar -> 200 ; repetir -> 404 (la pareja ya no existe)
curl -X DELETE http://localhost:5137/api/sparepartsupplier/11/11
curl -X DELETE http://localhost:5137/api/sparepartsupplier/11/11

# Ya sin asociaciones, los registros creados en este recorrido se pueden borrar -> 200
curl -X DELETE http://localhost:5137/api/sparepart/11
curl -X DELETE http://localhost:5137/api/supplier/11
```

---

## Revisión del código

- Los detalles de repuestos y proveedores incluyen sus asociaciones y condiciones.
- Los vehículos se dan de baja sin eliminar su historial.
- Editar un vehículo sin enviar `image` conserva la referencia actual.
- Las imágenes se validan con SkiaSharp: se comprueba el formato y se intenta decodificar
  el contenido. Se permiten JPEG, PNG, GIF y WebP, hasta 5 MB y 20 millones de píxeles.
- El archivo anterior se elimina después de guardar el reemplazo. Si falla el guardado,
  se descarta el archivo nuevo.
- Las claves únicas y las relaciones también están configuradas en SQL Server.

## Pruebas

Los scripts de `verificacion/` permiten revisar los módulos en una base de pruebas.
Sus resultados dependen de que la API apunte a esa misma base y de ejecutar las
baterías en el orden indicado. No se deben ejecutar contra la base usada para la entrega.

La comprobación posterior a la limpieza de comentarios se registra al final de este
archivo. El resultado de una batería no garantiza que se hayan probado todos los casos.

## Cómo reproducir la verificación

Todo se ejecuta contra una BD desechable y almacenamiento temporal; **no toca** la BD
`FlotaVehicular` ni `ERP9`.

```powershell
# 1. Crear la BD de prueba e aplicar la migración (la semilla corre sola; es idempotente)
sqlcmd -S localhost -C -E -Q "CREATE DATABASE FlotaVehicular_RevVerify"
$env:ConnectionStrings__FlotaVehicular = 'Server=localhost;Database=FlotaVehicular_RevVerify;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False'
cd C:\Web\FlotaVehicular
dotnet ef database update --project FlotaVehicular.Infrastructure --startup-project FlotaVehicular.WebAPI

# 2. Generar los archivos de prueba (real.png/jpg/gif, corrupt.png, fake.png, big.png)
powershell -ExecutionPolicy Bypass -File .\verificacion\Nuevos-Archivos-Prueba.ps1

# 3. Arrancar el API apuntando a la BD de prueba y a un almacén temporal
$env:Storage__VehicleImagesPath = "$env:TEMP\opencode\flota-imgs-verify"
cd FlotaVehicular.WebAPI
dotnet run --launch-profile http        # http://localhost:5137

# 4. Ejecutar las baterías (en orden: la 2 asume los datos que crea la 1)
powershell -ExecutionPolicy Bypass -File .\verificacion\Bateria1-Fixes.ps1   # 35/35
powershell -ExecutionPolicy Bypass -File .\verificacion\Bateria2-CRUD.ps1    # 63/63

# 5. Al terminar: eliminar la BD desechable y el almacén temporal
#    (la Batería 1 crea y elimina su trigger de prueba en la BD de prueba)
```

Notas:

- Los scripts usan PowerShell 5.1+ y `curl.exe` (para multipart); las respuestas se
  validan con `Invoke-WebRequest`.
- La Batería 1 necesita `sqlcmd` autenticado por Windows sobre
  `FlotaVehicular_RevVerify` (variable `$testDb` al inicio del script).
- Las baterías no son reentrantes sobre una BD ya usada (crean registros propios);
  para repetir la corrida, recrea la BD (paso 1).

## Organización del código

Se conserva la arquitectura de ERP9: los controladores reciben las solicitudes,
los servicios aplican las reglas y los repositorios consultan o guardan con EF Core.
Los DTO evitan devolver las entidades directamente y Mapster realiza los mapeos.
Los importes usan `decimal` y las respuestas usan `Result<T>`.

SkiaSharp se usa únicamente para leer y validar imágenes. En Windows se restaura con
NuGet. Para ejecutar en Linux también se necesitan sus componentes nativos de Linux.

## Pendientes de integración (otro integrante)

1. **Módulos de técnicos, especialidades y órdenes de trabajo**: se conectarán al mismo
   `AppDbContext` (`FlotaVehicular.Infrastructure.Data`) y pueden reutilizar el patrón
   repositorio/servicio/DTO/`Result<T>` ya presente. El esquema base ya está aplicado
   (ver [Migración — estado real](#migración--estado-real)): generen sus tablas como
   **nuevas migraciones encima de `InitialCreate`**.
2. **Excepciones globales**: queda pendiente el middleware de manejo global acordado;
   hoy los errores inesperados responden con la página de desarrollo de ASP.NET Core.
   Los errores controlados ya salen en formato `Result<T>` con su código HTTP.
3. Al agregar los módulos futuros, recordar que los enums se envían como texto
   (`JsonStringEnumConverter` ya está registrado en `Program.cs`, en modo estricto).

## Comprobación del 19/10/2026

- Compilación de la solución: 0 errores y 0 advertencias.
- 22 comprobaciones HTTP aprobadas: consultas de los seis módulos, paginación,
  detalles de asociaciones, stock bajo, referencias inexistentes y entradas inválidas.
- 12 pruebas dirigidas aprobadas: imágenes PNG/JPEG/GIF/WebP válidas, archivos
  falsos o incompletos, tamaño máximo y edición de vehículo conservando su imagen.
- Los casos de imagen y mapeo se probaron de forma aislada; la consulta de los
  catálogos se hizo contra SQL Server. En esta revisión no se repitió el CRUD
  completo ni la prueba de recuperación de archivos tras reiniciar.
- Las migraciones no cambiaron y no se modificaron los datos existentes.

Para ver los cambios al ejecutar desde Visual Studio, detener la API y volver a
iniciarla con F5.
