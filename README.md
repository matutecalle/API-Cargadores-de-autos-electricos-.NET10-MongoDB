# API Cargadores EV

API REST para una red de estaciones de carga de autos eléctricos. Permite a los conductores buscar cargadores (por filtros o por cercanía), iniciar y finalizar sesiones de carga con cálculo de costo, y a los administradores monitorear la red y gestionar las estaciones.

Está construida con **ASP.NET Core (.NET 10)** y **MongoDB**, usa **JWT** para la autenticación e incluye un frontend web que consume la API.

---

## ¿Qué problema resuelve?

Un conductor de auto eléctrico necesita saber **dónde cargar**, **cuánto le va a costar** y **si el cargador está libre**. Un operador de la red necesita saber **cómo se está usando** su infraestructura.

Esta API cubre los dos lados:

| Rol | Qué puede hacer |
|---|---|
| **Visitante** | Buscar estaciones con filtros y encontrar las más cercanas a una ubicación. |
| **Conductor** (usuario registrado) | Iniciar una carga en una estación disponible, finalizarla y ver su historial con energía consumida y costo. |
| **Administrador** | Ver el estado de la red en tiempo real, estadísticas de uso y crear, editar o dar de baja estaciones. |

## Funcionalidades

- **Búsqueda con filtros y paginación**: por tipo de cargador, conector, operador, potencia mínima, costo máximo, estado y energía renovable.
- **Búsqueda por cercanía**: consulta geoespacial sobre un índice `2dsphere` de MongoDB. Devuelve las estaciones ordenadas por distancia a una coordenada, dentro de un radio en km.
- **Sesiones de carga**: al iniciar una sesión la estación pasa a "En uso". Al finalizarla se registra la energía entregada (informada por el cliente o estimada según la potencia de la estación y la duración), se calcula el costo con el precio por kWh y la estación vuelve a quedar disponible.
- **Autenticación y autorización**: registro y login con JWT, contraseñas hasheadas con BCrypt y endpoints protegidos por rol (`User` / `Admin`).
- **Panel de administración**: métricas en tiempo real y estadísticas calculadas con *aggregation pipelines* de MongoDB (sesiones por día, estaciones más usadas).
- **Carga inicial automática**: en el primer arranque importa 5000 estaciones desde un CSV y crea un usuario administrador.
- **Documentación interactiva** con Swagger, con soporte para probar endpoints protegidos con el token.
- **Frontend web** servido por la misma API (sin configuración de CORS).

## Tecnologías

| Área | Tecnología |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10) |
| Base de datos | MongoDB (driver oficial `MongoDB.Driver`) |
| Autenticación | JWT Bearer + BCrypt |
| Documentación | Swagger / OpenAPI (Swashbuckle) |
| Importación de datos | CsvHelper |
| Contenedores | Docker y Docker Compose |
| Frontend | HTML, CSS y JavaScript sin frameworks |

## Arquitectura

El código está organizado en capas, cada una con una responsabilidad:

```
Controllers   → reciben la request HTTP, validan la entrada y devuelven el código de estado
     │
Services      → lógica de negocio: reglas de las sesiones, cálculo de costos, estadísticas
     │
Repositories  → acceso a MongoDB: consultas, índices y aggregations
     │
MongoDB
```

Decisiones de diseño:

- **Inyección de dependencias**: los controllers y servicios dependen de interfaces (`IStationService`, `IStationRepository`, ...), no de implementaciones concretas. Esto desacopla las capas y facilita testear.
- **Repository pattern**: todo el acceso a datos está aislado en los repositorios. El resto de la aplicación no conoce los detalles de MongoDB.
- **DTOs**: los contratos de entrada y salida de la API están separados de los modelos que se guardan en la base.
- **Options pattern**: la configuración de MongoDB y JWT se carga en clases tipadas (`MongoDbSettings`, `JwtSettings`).
- **Secretos fuera del código**: el connection string de producción se configura con *user-secrets* o variables de entorno, nunca en el repositorio.

### Estructura del proyecto

```
├── Config/          Settings tipados y contexto de MongoDB
├── Controllers/     Endpoints HTTP
├── Data/            Dataset CSV de estaciones
├── DTOs/            Objetos de entrada y salida
├── Models/          Documentos de MongoDB
├── Repositories/    Acceso a datos
├── Services/        Lógica de negocio y carga inicial
├── wwwroot/         Frontend web
├── Program.cs       Configuración de la app (DI, JWT, Swagger)
├── Dockerfile
└── docker-compose.yml
```

## Cómo correrlo

### Opción 1: con Docker (recomendada)

Requisito: [Docker Desktop](https://www.docker.com/products/docker-desktop/).

Levanta la API y una base MongoDB en contenedores, sin instalar nada más:

```bash
git clone https://github.com/matutecalle/APICargadores.git
cd APICargadores
docker compose up --build
```

| | URL |
|---|---|
| Frontend | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

El primer arranque tarda un poco más porque importa las 5000 estaciones. Para detenerlo: `docker compose down` (agregá `-v` para borrar también los datos).

### Opción 2: con .NET

Requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download) y una base MongoDB, local o en [MongoDB Atlas](https://www.mongodb.com/atlas) (tiene un plan gratuito).

Por defecto la API se conecta a `mongodb://localhost:27017`. Para usar Atlas, guardá el connection string con *user-secrets* (queda en tu máquina, fuera del repositorio):

```bash
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://<usuario>:<password>@<cluster>/"
```

Después:

```bash
dotnet run --launch-profile http
```

| | URL |
|---|---|
| Frontend | http://localhost:5134 |
| Swagger | http://localhost:5134/swagger |

## Cómo usarlo

### Desde el frontend

1. Abrí el frontend. La pestaña **Explorar** lista las estaciones; usá los filtros para acotar la búsqueda.
2. En **Cercanos**, ingresá una coordenada (o usá tu ubicación) para ver las estaciones ordenadas por distancia.
3. Iniciá sesión con la cuenta de prueba o registrate. Desde una estación disponible, hacé clic en **Iniciar carga**.
4. En **Mis sesiones**, detené la carga: la API calcula la energía y el costo y la guarda en tu historial.
5. Con la cuenta admin aparece la pestaña **Administración**, con el estado de la red y las estadísticas.

### Desde Swagger

1. Ejecutá `POST /api/auth/login` con la cuenta de prueba y copiá el `token` de la respuesta.
2. Hacé clic en **Authorize** y pegá el token.
3. Ya podés probar los endpoints protegidos.

### Cuenta de prueba

| Email | Contraseña | Rol |
|---|---|---|
| `admin@cargadores.com` | `Admin123!` | Admin |

Los usuarios registrados desde `/api/auth/register` tienen rol `User`.

## Endpoints

### Autenticación

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/auth/register` | Registra un conductor y devuelve un JWT |
| `POST` | `/api/auth/login` | Inicia sesión y devuelve un JWT |

### Estaciones (públicos)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/stations` | Búsqueda con filtros y paginación |
| `GET` | `/api/stations/nearby?lat=&lng=&maxKm=&limit=` | Estaciones cercanas a una coordenada |
| `GET` | `/api/stations/{id}` | Detalle de una estación |

Filtros de `/api/stations`: `chargerType`, `connectorType`, `operator`, `minCapacityKw`, `maxCostPerKwh`, `status`, `renewableOnly`, `page`, `pageSize`.

### Sesiones de carga (requieren login)

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/sessions/start` | Inicia una sesión en una estación disponible |
| `POST` | `/api/sessions/{id}/stop` | Finaliza la sesión y calcula energía y costo |
| `GET` | `/api/sessions/history` | Historial del usuario |

### Administración (requieren rol Admin)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/admin/dashboard` | Estado de la red en tiempo real |
| `GET` | `/api/admin/stats/sessions-per-day?days=` | Sesiones y energía por día |
| `GET` | `/api/admin/stats/top-stations?limit=` | Estaciones más usadas |
| `POST` | `/api/admin/stations` | Crea una estación |
| `PUT` | `/api/admin/stations/{id}` | Edita una estación |
| `PATCH` | `/api/admin/stations/{id}/out-of-service` | Marca una estación como fuera de servicio |
| `DELETE` | `/api/admin/stations/{id}` | Elimina una estación |

### Códigos de respuesta

| Código | Cuándo |
|---|---|
| `200` / `201` / `204` | Operación exitosa |
| `400` | Datos de entrada inválidos |
| `401` | Falta el token o expiró |
| `403` | El usuario no tiene el rol necesario |
| `404` | El recurso no existe |
| `409` | Conflicto de negocio (por ejemplo, la estación ya está en uso o el email ya está registrado) |

## Configuración

| Variable | Descripción | Valor por defecto (Development) |
|---|---|---|
| `MongoDb__ConnectionString` | Connection string de MongoDB | `mongodb://localhost:27017` |
| `MongoDb__DatabaseName` | Nombre de la base | `EvChargingDb` |
| `Jwt__Key` | Clave para firmar los tokens (mínimo 32 caracteres) | Clave de desarrollo |
| `Jwt__ExpiryMinutes` | Duración del token | `120` |
| `ASPNETCORE_ENVIRONMENT` | `Development` habilita Swagger | — |

En .NET, el doble guion bajo (`__`) de una variable de entorno equivale a una sección de `appsettings.json`: `MongoDb__ConnectionString` configura `MongoDb:ConnectionString`.

Para desplegar en producción (por ejemplo, en Render con el `Dockerfile`), hay que definir como mínimo `MongoDb__ConnectionString` y `Jwt__Key`. La clave de `appsettings.Development.json` es solo para desarrollo.

## Notas

- El dataset de estaciones (`Data/stations.csv`) es **sintético**: las direcciones y coordenadas no corresponden a lugares reales.
