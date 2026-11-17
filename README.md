# Flota Vehicular

API REST para la gestión de una flota vehicular y sus solicitudes de mantenimiento. Permite administrar vehículos, categorías, imágenes, técnicos, especialidades, repuestos y proveedores, junto con las relaciones entre estos registros.

El contexto del sistema es el mantenimiento de vehículos. Cada unidad tiene su categoría, kilometraje actual y kilometraje del último mantenimiento preventivo. Las categorías definen el intervalo de mantenimiento en kilómetros.

## Tecnologías

| Componente | Tecnología |
|---|---|
| Plataforma | .NET 10 (`net10.0`) |
| API | ASP.NET Core Web API |
| Persistencia | Entity Framework Core 10.0.13 y SQL Server |
| Mapeo entre entidades y DTO | Mapster 10.0.13 |
| Documentación de la API | OpenAPI y Scalar |
| Validación de imágenes | SkiaSharp |

## Organización del proyecto

La solución está dividida en cuatro proyectos:

```text
FlotaVehicular.slnx
├── FlotaVehicular.Domain
│   ├── Entities
│   ├── DTO
│   ├── Exceptions
│   └── Validations
├── FlotaVehicular.Infrastructure
│   ├── Data
│   │   ├── Configurations
│   │   └── Seed
│   ├── Interfaces
│   ├── Implementation
│   └── Migrations
├── FlotaVehicular.Application
│   ├── Interfaces
│   ├── Implementation
│   └── Mapper
└── FlotaVehicular.WebAPI
    ├── Controllers
    ├── Handlers
    ├── Properties
    ├── Program.cs
    └── appsettings.json
```

- **Domain:** contiene las entidades, enumeraciones, DTO, excepciones y validaciones compartidas.
- **Infrastructure:** contiene el contexto de EF Core, las configuraciones de las entidades, las migraciones, los repositorios, el seeder y el almacenamiento local de imágenes.
- **Application:** contiene los servicios que aplican las reglas de negocio y los mapeos de Mapster.
- **WebAPI:** recibe las solicitudes HTTP, registra las dependencias y configura las respuestas y el manejo global de excepciones.

Los controladores utilizan interfaces de servicios. Los servicios trabajan con interfaces de repositorios y estos realizan las consultas y operaciones de persistencia mediante `AppDbContext`. Las respuestas se construyen con DTO y `Result<T>`.

## Requisitos

- .NET SDK 10.
- SQL Server en ejecución y una cuenta con permisos para crear o actualizar la base de datos.
- Herramienta `dotnet-ef` de la versión 10 para aplicar las migraciones.

Si no está instalada la herramienta de Entity Framework:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
```

## Configuración y ejecución

### Conexión a SQL Server

La conexión se configura en `FlotaVehicular.WebAPI/appsettings.json`, con la clave `ConnectionStrings:FlotaVehicular`:

```text
Server=localhost;Database=FlotaVehicular;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False
```

Esta configuración usa autenticación de Windows. Si SQL Server está en otra instancia, se debe ajustar `Server` antes de aplicar las migraciones.



### Preparar la base de datos

Desde la carpeta que contiene `FlotaVehicular.slnx`:

```powershell
dotnet restore FlotaVehicular.slnx
dotnet build FlotaVehicular.slnx
dotnet ef database update --project FlotaVehicular.Infrastructure --startup-project FlotaVehicular.WebAPI
```

El esquema se define con las entidades y sus configuraciones de EF Core, y se actualiza en SQL Server mediante migraciones. El proyecto incluye:

| Migración | Contenido |
|---|---|
| `InitialCreate` | Categorías, vehículos, imágenes, repuestos, proveedores y asociaciones repuesto-proveedor. |
| `AddTechniciansSpecialtiesOrders` | Técnicos, especialidades, asociaciones técnico-especialidad y órdenes de mantenimiento. |
| `AddTechnicianEmailUniqueIndex` | Índice único para el correo de los técnicos. |
| `AddOrderCreatedByUser` | Columna opcional `CreatedByUserId` en órdenes, con llave foránea a usuarios y eliminación `Restrict`. |

Al aplicar las migraciones también se ejecuta `FleetSeeder`, registrado mediante `UseSeeding` y `UseAsyncSeeding`. La API no aplica migraciones al iniciar.

### Iniciar la API

Desde la misma carpeta de la solución:

```powershell
dotnet run --project FlotaVehicular.WebAPI --launch-profile http
```

El perfil `http` utiliza el entorno `Development` y la dirección `http://localhost:5137`.

- [Documentación interactiva en Scalar](http://localhost:5137/scalar)
- [Especificación OpenAPI](http://localhost:5137/openapi/v1.json)

Scalar permite consultar los contratos de entrada y salida y realizar solicitudes a los endpoints. La documentación se habilita al ejecutar en desarrollo.

## Autenticación y roles

La API usa tokens JWT Bearer. El inicio de sesión es público; el resto de `/api` exige un token válido, excepto la descarga pública de imágenes. La clave se configura en `Jwt:SecretKey` (mínimo 32 bytes), con `Jwt:Issuer`, `Jwt:Audience` y `Jwt:ExpirationMinutes` (60). Para no versionar secretos se puede usar user-secrets o la variable `Jwt__SecretKey`.

### Endpoints de autenticación

| Método | Ruta | Acceso | Operación |
|---|---|---|---|
| POST | `/api/auth/login` | Público | Inicia sesión con `username` o `email` y `password`. |
| POST | `/api/auth/logout` | Autenticado | Cierra la sesión; el cliente descarta el token y la llamada queda en la bitácora. |
| GET | `/api/auth/me` | Autenticado | Devuelve el usuario vigente del token. |

### Respuesta del login

```json
{
  "isSuccess": true,
  "status": 200,
  "title": "OK",
  "detail": "Sesión iniciada.",
  "data": {
    "token": "eyJ...",
    "tokenType": "Bearer",
    "expiresAt": "2026-11-17T20:00:00Z",
    "expiresInSeconds": 3600,
    "user": {
      "idUser": 1,
      "fullName": "Administrador",
      "username": "admin",
      "email": "admin@flota.local",
      "role": "Administrador"
    }
  }
}
```

`data.user` nunca incluye el hash de la contraseña. Un usuario inexistente y una contraseña incorrecta devuelven el mismo 401: `Usuario o contraseña incorrectos.`

### Códigos 401 y 403

| Código | Situación | Detalle |
|---|---|---|
| 401 | Falta el token, es inválido, expiró o la sesión ya no es válida. | `Sesión no válida o expirada. Inicie sesión nuevamente.` |
| 401 | Credenciales incorrectas. | `Usuario o contraseña incorrectos.` |
| 401 | El usuario del token ya no existe o está inactivo. | `La sesión ya no es válida. Inicie sesión nuevamente.` |
| 403 | El rol autenticado no tiene permiso para la acción. | `No tiene permisos para realizar esta acción.` |
| 403 | La cuenta está desactivada al iniciar sesión. | `La cuenta está desactivada. Comuníquese con un administrador.` |

### Tabla de permisos

| Recurso | Consultar | Crear, editar, cambiar estado o eliminar |
|---|---|---|
| `/api/auth` | Cualquier autenticado (`me`); `login` es público | No aplica; `logout` es cualquier autenticado |
| `/api/vehicle` | Cualquier autenticado | Solo Administrador |
| `/api/vehicle/{id}/images` | Listado: cualquier autenticado; archivo: público | Solo Administrador |
| `/api/vehiclecategory` | Administrador o Coordinador | Solo Administrador |
| `/api/technician` | Administrador o Coordinador | Solo Administrador |
| `/api/specialty` | Administrador o Coordinador | Solo Administrador |
| `/api/technicianspecialty` | Administrador o Coordinador | Solo Administrador |
| `/api/sparepart` | Administrador o Coordinador | Solo Administrador |
| `/api/supplier` | Administrador o Coordinador | Solo Administrador |
| `/api/sparepartsupplier` | Administrador o Coordinador | Solo Administrador |
| `/api/user` | Solo Administrador | Solo Administrador |
| `/api/maintenanceorder` | Cualquier autenticado | POST: Administrador o Solicitante; cambio de estado: Administrador o Coordinador |

El Solicitante solo consulta las órdenes que creó: una orden ajena responde 404 con `La orden {id} no existe.` El creador se toma del claim del token, nunca del cuerpo; el listado y el detalle devuelven `createdByUserId` y `createdByUserName`.

## CORS

La política `Frontend` permite los orígenes de `Cors:AllowedOrigins` (`http://localhost:5173` y `http://localhost:3000` por defecto), cualquier encabezado y cualquier método HTTP. No se habilita el envío de credenciales: el token se envía en el encabezado `Authorization`.

## Bitácora de peticiones

Cada llamada a `/api` genera un archivo `.txt` en `FlotaVehicular.WebAPI/Logs`, configurable con `RequestLog:Directory`. La carpeta se crea automáticamente y está excluida de Git. El nombre sigue el patrón `yyyyMMdd_HHmmss_fff_METODO_ruta_estado_codigo.txt`, por ejemplo `20261030_194855_123_GET_api-vehicle-5_200_a1b2c3.txt`.

Cada archivo registra fecha y hora, TraceId, método, ruta, parámetros de consulta, código HTTP, resultado, duración en milisegundos, IP del cliente, usuario y rol (o `anónimo`) y, cuando falló, el `Result` de error o la excepción con su stack trace. En errores 500 el TraceId coincide con `data.traceId` de la respuesta.

No se registran el cuerpo de la petición ni el encabezado `Authorization`, por lo que no aparecen contraseñas ni tokens. Tampoco se registran Scalar, OpenAPI, archivos estáticos ni las consultas previas de CORS.

## Datos iniciales

`FleetSeeder` carga datos del contexto vehicular. Antes de insertar, busca los registros por sus nombres, códigos, identificación, correo o claves de asociación para evitar duplicarlos al ejecutar nuevamente la semilla.

En una base nueva se crean:

| Tabla | Registros |
|---|---:|
| Categorías de vehículos | 10 |
| Vehículos | 10 |
| Técnicos | 10 |
| Especialidades | 10 |
| Repuestos | 10 |
| Proveedores | 10 |
| Órdenes de mantenimiento | 10 |
| Asociaciones repuesto-proveedor | 23 |
| Asociaciones técnico-especialidad | 20 |

Los vehículos incluyen ejemplos de los cuatro estados: cinco operativos, dos en mantenimiento, uno fuera de servicio y dos dados de baja. Los técnicos incluyen registros disponibles, no disponibles e inactivos. El repuesto `REP-009` tiene stock inferior al mínimo.

Las órdenes iniciales incluyen distintos estados como datos de ejemplo. Las solicitudes creadas desde la API nacen en estado `Solicitada` y con costo total en cero.

Las imágenes se agregan mediante sus endpoints y se almacenan en disco; el seeder no carga archivos de imagen.

### Usuarios iniciales (solo desarrollo)

`FleetSeeder` también crea un usuario por rol si todavía no existe por nombre de usuario o correo. Las credenciales son solo para desarrollo y deben cambiarse con `PUT /api/user/{id}/password`:

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123!` | Administrador |
| `coordinador` | `Coord123!` | Coordinador/Técnico |
| `solicitante` | `Solic123!` | Solicitante/Consulta |

Los hash se generan con el mismo PBKDF2-SHA256 que utiliza la API para los usuarios creados desde los endpoints.

## Modelo de datos y relaciones

| Relación | Cardinalidad | Implementación |
|---|---|---|
| Categoría y vehículos | 1:N | Cada vehículo pertenece a una categoría. |
| Vehículo e imágenes | 1:N | Un vehículo puede tener varias imágenes y una principal. |
| Vehículo y órdenes | 1:N | Cada orden corresponde a un único vehículo. |
| Especialidad y órdenes | 1:N | Una orden puede indicar una especialidad requerida. |
| Técnicos y especialidades | N:N | `TechnicianSpecialty`, con nivel de certificación y fecha desde la que posee la especialidad. |
| Repuestos y proveedores | N:N | `SparePartSupplier`, con precio ofrecido, días estimados de entrega y fecha de asociación. |

Las tablas puente utilizan claves primarias compuestas para impedir asociaciones duplicadas. SQL Server también aplica los índices únicos y las restricciones de integridad configuradas en EF Core. Los importes monetarios se almacenan como `decimal(18,2)`.

## Módulos y reglas de negocio

### Vehículos y categorías

El vehículo registra código, placa, nombre, descripción, fecha de adquisición, valor de referencia, categoría e imagen. Los datos propios del contexto son marca, modelo, año, kilometraje actual y kilometraje del último mantenimiento preventivo.

- El código y la placa son únicos; se guardan sin espacios al inicio o al final y en mayúsculas. La semilla utiliza códigos como `VEH-0001` para identificar cada unidad independientemente de su placa.
- La categoría debe existir y su nombre es único.
- No se permite eliminar una categoría con vehículos asociados, incluidos los dados de baja.
- La fecha de adquisición no puede ser futura y el kilometraje del último mantenimiento preventivo no puede superar el actual.
- Los vehículos nuevos se registran como `Operativo`.
- Los estados disponibles son `Operativo`, `EnMantenimiento`, `FueraDeServicio` y `DadoDeBaja`.
- La baja es lógica: conserva el registro y sus relaciones, y guarda la fecha de baja. Un vehículo dado de baja no admite edición ni cambio de estado.
- El listado excluye los vehículos dados de baja, salvo que se utilice `includeDeactivated=true`. La consulta por ID permite recuperar su información.

La categoría guarda el intervalo preventivo en `preventiveMaintenanceIntervalKm`. Las consultas de vehículos devuelven `kmToNextPreventiveMaintenance`, calculado así:

```text
Intervalo de la categoría - (kilometraje actual - kilometraje del último preventivo)
```

Un resultado igual a cero indica que se alcanzó el intervalo y uno negativo indica que se superó.

### Imágenes de vehículos

Las imágenes se cargan como `multipart/form-data`, con el campo `file` y, en la carga, el campo opcional `isMain`. La primera imagen queda como principal. Al agregar otra con `isMain=true`, se actualiza la referencia principal del vehículo.

Se aceptan JPEG, PNG, GIF y WebP, con un tamaño máximo de 5 MB y hasta 20 millones de píxeles. Se comprueba que el contenido se pueda decodificar y corresponda con la extensión y el tipo de contenido declarado.

Los archivos se guardan en `FlotaVehicular.WebAPI/wwwroot/uploads/vehicles`, con sus referencias en SQL Server. La ubicación se configura con `Storage:VehicleImagesPath`.

Al reemplazar una imagen se conserva su ID. El archivo anterior se elimina después de guardar el reemplazo en la base de datos. Al eliminar la principal, se selecciona otra imagen del vehículo si existe.

### Técnicos y especialidades

Los técnicos registran nombre completo, identificación, correo, teléfono, disponibilidad, tarifa por hora y condición activa. La identificación y el correo son únicos, el correo debe tener un formato válido y la tarifa debe ser mayor que cero.

Cada especialidad tiene nombre y descripción. Un técnico puede tener varias especialidades y cada asociación guarda `certificationLevel` y `sinceDate`.

- No se puede registrar dos veces la misma pareja técnico-especialidad.
- Para crear una asociación, ambos registros deben existir y el técnico debe estar activo.
- Se puede actualizar la certificación o desasociar una especialidad.
- La consulta de disponibles por especialidad devuelve técnicos activos, disponibles y asociados a esa especialidad.
- Los técnicos se desactivan sin eliminar sus asociaciones y se pueden reactivar. `includeInactive=true` permite incluirlos en el listado.
- Las especialidades tienen nombre único. Su eliminación está restringida si tienen técnicos asociados o están referenciadas por órdenes.

### Repuestos y proveedores

Cada repuesto registra código, nombre, unidad de medida, stock actual y stock mínimo. El código es único y se guarda en mayúsculas. El stock actual y el mínimo no pueden ser negativos; estas restricciones también se aplican en SQL Server.

Los proveedores registran nombre, contacto, dirección, condiciones generales y condición activa. Su nombre es único. Se pueden desactivar y reactivar conservando las asociaciones existentes.

La relación repuesto-proveedor guarda `offeredPrice` y `estimatedDeliveryDays`. Permite consultar y actualizar las condiciones ofrecidas por cada proveedor.

- No se puede repetir la misma pareja repuesto-proveedor.
- Ambos registros deben existir y el proveedor debe estar activo para crear una asociación.
- No se permite eliminar físicamente un repuesto o proveedor mientras tenga asociaciones en la tabla puente.
- El detalle de cada repuesto incluye sus proveedores y el de cada proveedor incluye sus repuestos, con las condiciones de cada asociación.
- La consulta de stock bajo devuelve los repuestos cuyo `currentStock` es menor que `minStock`.

### Solicitudes de mantenimiento

Permite crear solicitudes y consultar su listado o detalle. La orden almacena código, vehículo, tipo de mantenimiento, especialidad requerida opcional, motivo, fechas de solicitud, programación y cierre, prioridad, costo total y estado.

- Los tipos de mantenimiento son `Preventivo`, `Correctivo` y `Emergencia`.
- Las prioridades son `Baja`, `Media`, `Alta` y `Critica`.
- La API genera el código con el formato `OM-yyyyMMdd-0001`, asigna la fecha de solicitud en UTC y registra el estado inicial `Solicitada` y el costo total en cero.
- El vehículo debe existir y no puede estar dado de baja ni fuera de servicio.
- Si el vehículo está en mantenimiento, solo se admite una solicitud de tipo `Emergencia`.
- La especialidad requerida debe existir cuando se envía.
- La fecha programada no puede ser anterior al día actual en UTC.

## Endpoints

Las rutas parten de `http://localhost:5137`. Los cuerpos de creación y edición se envían como JSON, excepto los archivos de imagen. Los campos de los contratos se pueden consultar en Scalar.

### Categorías

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/vehiclecategory` | Listar y buscar por nombre o descripción con `search`. |
| GET | `/api/vehiclecategory/paged` | Consultar con paginación y búsqueda. |
| GET | `/api/vehiclecategory/{id}` | Consultar detalle y vehículos asociados. |
| POST | `/api/vehiclecategory` | Registrar categoría. |
| PUT | `/api/vehiclecategory/{id}` | Actualizar categoría. |
| DELETE | `/api/vehiclecategory/{id}` | Eliminar una categoría sin vehículos. |

### Vehículos

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/vehicle` | Listar y filtrar vehículos. |
| GET | `/api/vehicle/paged` | Consultar con paginación y filtros. |
| GET | `/api/vehicle/{id}` | Consultar detalle, categoría e imágenes. |
| POST | `/api/vehicle` | Registrar vehículo. |
| PUT | `/api/vehicle/{id}` | Actualizar vehículo. |
| PUT | `/api/vehicle/{id}/status` | Cambiar a `Operativo`, `EnMantenimiento` o `FueraDeServicio`. |
| DELETE | `/api/vehicle/{id}` | Dar de baja lógicamente. |

Filtros: `search` por código, placa, nombre, marca o modelo; `idVehicleCategory`, `status` e `includeDeactivated`.

### Imágenes

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/vehicle/{vehicleId}/images` | Consultar las imágenes del vehículo. |
| POST | `/api/vehicle/{vehicleId}/images` | Cargar archivo con `file` e `isMain` opcional. |
| GET | `/api/vehicle/{vehicleId}/images/{imageId}/file` | Descargar el archivo. |
| PUT | `/api/vehicle/{vehicleId}/images/{imageId}` | Reemplazar el archivo con `file`. |
| DELETE | `/api/vehicle/{vehicleId}/images/{imageId}` | Eliminar imagen y archivo. |

### Técnicos

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/technician` | Listar y buscar por nombre, identificación o correo. |
| GET | `/api/technician/paged` | Consultar con paginación y búsqueda. |
| GET | `/api/technician/{id}` | Consultar detalle y especialidades. |
| GET | `/api/technician/available?idSpecialty={id}` | Consultar disponibles por especialidad. |
| POST | `/api/technician` | Registrar técnico. |
| PUT | `/api/technician/{id}` | Actualizar técnico. |
| PUT | `/api/technician/{id}/deactivate` | Desactivar técnico. |
| PUT | `/api/technician/{id}/activate` | Reactivar técnico. |

Filtros del listado: `search` e `includeInactive`.

### Especialidades

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/specialty` | Listar y buscar por nombre con `search`. |
| GET | `/api/specialty/paged` | Consultar con paginación y búsqueda. |
| GET | `/api/specialty/{id}` | Consultar detalle y técnicos asociados. |
| POST | `/api/specialty` | Registrar especialidad. |
| PUT | `/api/specialty/{id}` | Actualizar especialidad. |
| DELETE | `/api/specialty/{id}` | Eliminar una especialidad sin referencias. |

### Técnico-especialidad

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/technicianspecialty` | Listar asociaciones. |
| GET | `/api/technicianspecialty/paged` | Consultar con paginación y filtros. |
| GET | `/api/technicianspecialty/{idTechnician}/{idSpecialty}` | Consultar una asociación. |
| POST | `/api/technicianspecialty` | Asociar una especialidad con certificación y fecha. |
| PUT | `/api/technicianspecialty/{idTechnician}/{idSpecialty}` | Actualizar certificación y fecha. |
| DELETE | `/api/technicianspecialty/{idTechnician}/{idSpecialty}` | Desasociar una especialidad. |

Filtros: `idTechnician`, `idSpecialty` y `search`.

### Repuestos

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/sparepart` | Listar y filtrar repuestos. |
| GET | `/api/sparepart/paged` | Consultar con paginación y filtros. |
| GET | `/api/sparepart/below-minimum-stock` | Consultar stock bajo con paginación. |
| GET | `/api/sparepart/{id}` | Consultar detalle y proveedores asociados. |
| POST | `/api/sparepart` | Registrar repuesto. |
| PUT | `/api/sparepart/{id}` | Actualizar repuesto. |
| DELETE | `/api/sparepart/{id}` | Eliminar un repuesto sin proveedores asociados. |

Filtros: `search` por código o nombre y `belowMinimumOnly`.

### Proveedores

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/supplier` | Listar y buscar por nombre o contacto. |
| GET | `/api/supplier/paged` | Consultar con paginación y filtros. |
| GET | `/api/supplier/{id}` | Consultar detalle y repuestos asociados. |
| POST | `/api/supplier` | Registrar proveedor. |
| PUT | `/api/supplier/{id}` | Actualizar proveedor. |
| PUT | `/api/supplier/{id}/deactivate` | Desactivar proveedor. |
| PUT | `/api/supplier/{id}/activate` | Reactivar proveedor. |
| DELETE | `/api/supplier/{id}` | Eliminar un proveedor sin repuestos asociados. |

Filtros: `search` e `includeInactive`.

### Repuesto-proveedor

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/sparepartsupplier` | Listar asociaciones y sus condiciones. |
| GET | `/api/sparepartsupplier/paged` | Consultar con paginación y filtros. |
| GET | `/api/sparepartsupplier/{idSparePart}/{idSupplier}` | Consultar una asociación. |
| POST | `/api/sparepartsupplier` | Asociar un proveedor con precio y días de entrega. |
| PUT | `/api/sparepartsupplier/{idSparePart}/{idSupplier}` | Actualizar precio y días de entrega. |
| DELETE | `/api/sparepartsupplier/{idSparePart}/{idSupplier}` | Eliminar la asociación. |

Filtros: `idSparePart`, `idSupplier` y `search`.

### Órdenes de mantenimiento

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/maintenanceorder` | Listar y filtrar solicitudes. |
| GET | `/api/maintenanceorder/paged` | Consultar con paginación y filtros. |
| GET | `/api/maintenanceorder/{id}` | Consultar el detalle de una solicitud. |
| POST | `/api/maintenanceorder` | Crear una solicitud. |
| PUT | `/api/maintenanceorder/{id}/status` | Cambiar el estado de una orden. |

Filtros: `status`, `idVehicle`, `maintenanceType`, `priority` y `createdByUserId`.

El cambio de estado recibe, por ejemplo, `{"status":"Diagnosticada"}` y devuelve el detalle actualizado con HTTP 200. Las transiciones permitidas son:

| Estado actual | Estados siguientes permitidos |
|---|---|
| Solicitada | Diagnosticada, Cancelada |
| Diagnosticada | Aprobada, Rechazada, Cancelada |
| Aprobada | EnEjecucion, Cancelada |
| EnEjecucion | EnEsperaDeRepuesto, Completada, Cancelada |
| EnEsperaDeRepuesto | EnEjecucion, Cancelada |
| Completada, Rechazada, Cancelada | Ninguno: la orden está cerrada. |

Repetir el estado actual responde 200 sin volver a modificar la orden. Al completar, rechazar o cancelar se registra `closedDate` en UTC. Un estado ausente o inválido responde 400; una orden inexistente, 404; una transición no permitida o un cambio concurrente, 409. El estado y la fecha se actualizan juntos comprobando el estado anterior para no sobrescribir otra transición. Esta operación solo cambia la orden; no cambia el estado ni el kilometraje del vehículo. No requiere una migración.

Las pruebas específicas de este endpoint se ejecutan con `dotnet test verificacion/EstadosOrden.Tests/EstadosOrden.Tests.csproj --configuration Release`. Cubren las 64 combinaciones de estados, el flujo completo, la validación HTTP, la publicación en OpenAPI y la persistencia con protección ante cambios concurrentes. Usan una base SQLite temporal en memoria y un repositorio de prueba para HTTP; no modifican la base SQL Server configurada.

## Respuestas y validaciones

Las operaciones JSON utilizan el formato `Result<T>`:

```json
{
  "isSuccess": true,
  "status": 200,
  "title": "OK",
  "detail": "",
  "data": {}
}
```

El contenido de `data` depende de la operación. Las descargas de imágenes devuelven el archivo con su tipo de contenido.

| Código HTTP | Uso |
|---|---|
| 200 | Consulta, actualización, baja o eliminación correcta. |
| 201 | Registro o asociación creada. |
| 400 | Datos inválidos o incumplimiento de una regla de negocio. |
| 404 | Registro, asociación o recurso inexistente. |
| 405 | Método HTTP no permitido para la ruta. |
| 409 | Duplicados, referencias que impiden eliminar o conflictos de actualización. |
| 410 | El registro de imagen existe, pero su archivo no está disponible. |
| 413 | Archivo que supera el tamaño permitido. |
| 500 | Error interno. |

Los DTO validan campos obligatorios, longitudes, rangos y formatos. Los errores de validación incluyen el detalle por campo dentro de `data`. Los valores de enumeraciones enviados en JSON se representan como texto; se rechazan los números y los nombres que no pertenecen a la enumeración.

`GlobalExceptionHandler` centraliza las excepciones de la aplicación y de persistencia. Los conflictos de unicidad, integridad referencial y concurrencia se convierten en respuestas controladas. Los errores inesperados se registran en el log y devuelven un mensaje general con un identificador de seguimiento.

### Paginación

Los endpoints terminados en `/paged` reciben `pageNumber` y `pageSize`, además de los filtros de su módulo. Ambos parámetros tienen valores predeterminados de `1` y `10`, respectivamente. `pageNumber` debe ser mayor o igual a 1 y `pageSize` debe estar entre 1 y 50.

Las respuestas paginadas incluyen `pageNumber`, `pageSize`, `totalRecords` y `totalPages`, junto con la colección en `data`. Cuando una búsqueda o filtro no encuentra resultados, la consulta devuelve una colección vacía con código `200`.
