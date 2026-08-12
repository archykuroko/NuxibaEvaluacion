# EVALUACIÓN TÉCNICA NUXIBA

**Prueba:** DESARROLLADOR JR\
**Nombre:** Steven Arturo Escárcega Hernández

------------------------------------------------------------------------

# Solución desarrollada

La solución fue desarrollada utilizando ASP.NET Core Web API con .NET 8,
Entity Framework Core y SQL Server.

El proyecto implementa los tres ejercicios solicitados:

1.  API RESTful para la administración de registros de login y logout.
2.  Consultas SQL para el cálculo de tiempos de sesión.
3.  Endpoint para generar y descargar un reporte CSV con las horas
    trabajadas por usuario.

------------------------------------------------------------------------

## Tecnologías utilizadas

-   .NET 8
-   ASP.NET Core Web API
-   Entity Framework Core 8
-   SQL Server 2019
-   Docker
-   Swagger / OpenAPI
-   SQL Server Management Studio
-   Git

------------------------------------------------------------------------

# Configuración y ejecución

## 1. Levantar SQL Server con Docker

Es necesario tener Docker instalado y en ejecución.

Ejecutar:

``` powershell
docker run -e "ACCEPT_EULA=Y" `
  -e "SA_PASSWORD=YourStrong!Passw0rd" `
  -p 1433:1433 `
  --name sqlserver `
  -d mcr.microsoft.com/mssql/server:2019-latest
```

Para verificar que el contenedor se encuentra ejecutándose:

``` powershell
docker ps
```

Si el contenedor ya fue creado previamente y se encuentra detenido:

``` powershell
docker start sqlserver
```

------------------------------------------------------------------------

## 2. Conexión a SQL Server

Se puede utilizar SQL Server Management Studio (SSMS) o Azure Data
Studio.

Datos de conexión utilizados para el contenedor:

-   Servidor: `localhost,1433`
-   Tipo de autenticación: SQL Server Authentication
-   Usuario: `sa`
-   Contraseña: la configurada al crear el contenedor

La base de datos utilizada por la aplicación es:

``` text
NuxibaDB
```

------------------------------------------------------------------------

## 3. Configurar la cadena de conexión

En `appsettings.json` se utiliza una cadena de conexión con el nombre
`DB`.

Ejemplo:

``` json
{
  "ConnectionStrings": {
    "DB": "Server=localhost,1433;Database=NuxibaDB;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
  }
}
```

Para una implementación productiva las credenciales no deberían
almacenarse directamente en el repositorio; se recomienda utilizar
variables de entorno, User Secrets o un administrador de secretos.

------------------------------------------------------------------------

## 4. Restaurar dependencias

Desde el directorio del proyecto:

``` bash
dotnet restore
```

------------------------------------------------------------------------

## 5. Crear la base de datos mediante Entity Framework Core

El proyecto incluye las migraciones de Entity Framework Core necesarias.

Desde Visual Studio se puede utilizar la Consola del Administrador de
Paquetes:

``` powershell
Update-Database
```

Esto crea o actualiza el esquema de `NuxibaDB` de acuerdo con las
migraciones incluidas en el proyecto.

Las tablas utilizadas por la solución son:

-   `ccloglogin`
-   `ccUsers`
-   `ccRIACat_Areas`

Los datos proporcionados para la evaluación deben cargarse en estas
tablas tomando como referencia el archivo `CCenterRIA.xlsx`.
-----------------------------------------------------------------------
## 6. Carga de datos iniciales

El repositorio incluye una carpeta `SQL` con los scripts necesarios para cargar los datos utilizados durante la evaluación.

La información proporcionada originalmente para la prueba fue preparada en scripts SQL para facilitar la reproducción del entorno y evitar la carga manual de los registros.

Dentro de la carpeta se incluyen:

- Un script de **seed** para insertar la información inicial en las tablas:
  - `ccUsers`
  - `ccRIACat_Areas`
  - `ccloglogin`

- Los **queries correspondientes al Ejercicio 2**, utilizados para:
  - Obtener el usuario con mayor tiempo total logueado.
  - Obtener el usuario con menor tiempo total logueado.
  - Calcular el promedio de tiempo de logueo por usuario y mes.

### Ejecutar el seed

Después de crear la base de datos mediante las migraciones de Entity Framework Core, abrir el script de seed ubicado en la carpeta `SQL` utilizando SQL Server Management Studio (SSMS) o Azure Data Studio.

Verificar que la base seleccionada sea:

```sql
USE NuxibaDB;
GO
```

-----------------------------------------------------------------------

## 7. Ejecutar la API

Desde Visual Studio se puede ejecutar el proyecto utilizando el perfil
HTTPS.

También puede ejecutarse desde terminal:

``` bash
dotnet run
```

En ambiente de desarrollo, Swagger queda disponible en:

``` text
https://localhost:<puerto>/swagger/index.html
```

El puerto puede variar de acuerdo con la configuración local del
proyecto.

------------------------------------------------------------------------

# Ejercicio 1 - API RESTful de logins

La administración de los movimientos de login y logout se encuentra
implementada en `LoginsController`.

## Endpoints

  Método   Endpoint         Descripción
  -------- ---------------- -------------------------------------------------
  GET      `/logins`        Obtiene todos los movimientos de login y logout
  POST     `/logins`        Registra un nuevo login o logout
  PUT      `/logins/{id}`   Actualiza un movimiento existente
  DELETE   `/logins/{id}`   Elimina un movimiento
  GET      `/logins/{id}`   Obtiene un movimiento por su identificador

### GET /logins

Devuelve los registros existentes de la tabla `ccloglogin`.

Para las consultas de solo lectura se utiliza `AsNoTracking()` para
evitar el seguimiento innecesario de entidades por parte de Entity
Framework Core.

### POST /logins

El endpoint recibe un `CreateLoginDTO`, evitando que propiedades
administradas por la base de datos, como el identificador `Identity`,
sean proporcionadas directamente por el cliente.

Ejemplo:

``` json
{
  "userId": 1,
  "extension": 100,
  "tipoMov": 1,
  "fecha": "2026-08-11T12:00:00"
}
```

Se implementaron las siguientes validaciones:

-   `TipoMov` solamente puede contener `1` (login) o `0` (logout).
-   La fecha debe contener un valor válido.
-   No se permiten movimientos con fecha futura.
-   El usuario debe existir previamente en `ccUsers`.
-   No se permiten dos logins consecutivos para un mismo usuario.
-   No se permiten dos logouts consecutivos para un mismo usuario.
-   Un usuario sin movimientos anteriores no puede comenzar con un
    logout.

### PUT /logins/{id}

Permite modificar un registro existente utilizando `UpdateLoginDTO`.

Las validaciones de negocio se aplican antes de persistir los cambios.

### DELETE /logins/{id}

Elimina el registro identificado por el parámetro `id`.

Cuando el registro solicitado no existe, la API responde con el código
HTTP correspondiente.

------------------------------------------------------------------------

# Modelo de datos

Para trabajar con Entity Framework Core se definieron los modelos:

``` text
Models/
├── Area.cs
├── Login.cs
└── User.cs
```

El acceso a datos se centraliza mediante:

``` text
Data/
└── AppDbContext.cs
```

Los DTO utilizados por la API son:

``` text
DTOs/
├── CreateLoginDTO.cs
├── UpdateLoginDTO.cs
└── WorkedHoursReportDTO.cs
```

Para `ccloglogin` se agregó un identificador `Id` como llave primaria
`Identity`. Esto permite que Entity Framework Core identifique
individualmente cada registro y que los endpoints `PUT` y `DELETE`
puedan operar sobre un recurso específico.

------------------------------------------------------------------------

# Ejercicio 2 - Consultas SQL

Para calcular las sesiones se considera:

-   `TipoMov = 1`: login.
-   `TipoMov = 0`: logout.
-   Cada login se empareja con el siguiente movimiento cronológico del
    mismo usuario cuando dicho movimiento es un logout.
-   Las sesiones válidas se convierten a segundos mediante
    `DATEDIFF_BIG`.
-   Posteriormente se agregan los tiempos de acuerdo con cada consulta.

Se utilizaron CTE (`WITH`) y la función de ventana `LEAD()` para obtener
el siguiente movimiento de cada usuario sin realizar ciclos manuales.

## 1. Usuario con mayor tiempo total logueado

``` sql
WITH Movimientos AS
(
    SELECT
        User_id,
        TipoMov,
        fecha,
        LEAD(TipoMov) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS SiguienteTipoMov,
        LEAD(fecha) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS FechaLogout
    FROM dbo.ccloglogin
),
Sesiones AS
(
    SELECT
        User_id,
        DATEDIFF_BIG(SECOND, fecha, FechaLogout) AS SegundosSesion
    FROM Movimientos
    WHERE TipoMov = 1
      AND SiguienteTipoMov = 0
      AND FechaLogout IS NOT NULL
),
Totales AS
(
    SELECT
        User_id,
        SUM(SegundosSesion) AS SegundosTotales
    FROM Sesiones
    GROUP BY User_id
)
SELECT TOP 1
    User_id,
    SegundosTotales,
    CONCAT(
        SegundosTotales / 86400, ' días, ',
        (SegundosTotales % 86400) / 3600, ' horas, ',
        (SegundosTotales % 3600) / 60, ' minutos, ',
        SegundosTotales % 60, ' segundos'
    ) AS TiempoTotal
FROM Totales
ORDER BY SegundosTotales DESC, User_id ASC;
```

Resultado obtenido con los datos cargados:

``` text
User_id: 92
Tiempo total: 361 días, 12 horas, 51 minutos, 7 segundos
```

------------------------------------------------------------------------

## 2. Usuario con menor tiempo total logueado

Se utiliza el mismo cálculo de sesiones, modificando el orden final:

``` sql
ORDER BY SegundosTotales ASC, User_id ASC;
```

Resultado obtenido:

``` text
User_id: 90
Tiempo total: 244 días, 0 horas, 43 minutos, 9 segundos
```

------------------------------------------------------------------------

## 3. Promedio de tiempo de logueo por usuario y mes

``` sql
WITH Movimientos AS
(
    SELECT
        User_id,
        TipoMov,
        fecha,
        LEAD(TipoMov) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS SiguienteTipoMov,
        LEAD(fecha) OVER (
            PARTITION BY User_id
            ORDER BY fecha
        ) AS FechaLogout
    FROM dbo.ccloglogin
),
Sesiones AS
(
    SELECT
        User_id,
        YEAR(fecha) AS [AÑO],
        MONTH(fecha) AS [MES],
        DATEDIFF_BIG(SECOND, fecha, FechaLogout) AS SegundosSesion
    FROM Movimientos
    WHERE TipoMov = 1
      AND SiguienteTipoMov = 0
      AND FechaLogout IS NOT NULL
),
PromedioMensual AS
(
    SELECT
        User_id,
        [AÑO],
        [MES],
        AVG(CAST(SegundosSesion AS DECIMAL(19, 2))) AS PromedioSegundos
    FROM Sesiones
    GROUP BY User_id, [AÑO], [MES]
)
SELECT
    User_id,
    [AÑO],
    [MES],
    PromedioSegundos,
    CONCAT(
        CAST(PromedioSegundos AS BIGINT) / 86400, ' días, ',
        (CAST(PromedioSegundos AS BIGINT) % 86400) / 3600, ' horas, ',
        (CAST(PromedioSegundos AS BIGINT) % 3600) / 60, ' minutos, ',
        CAST(PromedioSegundos AS BIGINT) % 60, ' segundos'
    ) AS TiempoPromedio
FROM PromedioMensual
ORDER BY User_id, [AÑO], [MES];
```

------------------------------------------------------------------------

# Ejercicio 3 - Generación del reporte CSV

La generación del reporte se encuentra implementada en
`ReportsController`.

## Endpoint

``` http
GET /reports/worked-hours/csv
```

El archivo generado contiene las columnas:

``` text
Login
NombreCompleto
Area
TotalHoras
```

El nombre completo se construye a partir de:

-   `Nombres`
-   `ApellidoPaterno`
-   `ApellidoMaterno`

El total de horas se obtiene emparejando cronológicamente los
movimientos de cada usuario y acumulando la diferencia entre cada login
seguido de su logout.

El archivo generado se devuelve como:

``` text
reporte-horas-trabajadas.csv
```

con el tipo de contenido:

``` text
text/csv
```

## Descargar mediante Swagger

1.  Ejecutar la API.
2.  Abrir `/swagger/index.html`.
3.  Seleccionar `GET /reports/worked-hours/csv`.
4.  Presionar **Try it out**.
5.  Presionar **Execute**.
6.  Descargar o guardar el archivo devuelto por la respuesta.

## Probar mediante curl

Con la API ejecutándose, sustituir `<puerto>` por el puerto HTTPS
mostrado por la aplicación:

``` bash
curl -k "https://localhost:<puerto>/reports/worked-hours/csv" -o reporte-horas-trabajadas.csv
```

El parámetro `-o` guarda la respuesta directamente como archivo.

## Consideraciones del CSV

Para generar el archivo se utiliza `StringBuilder`.

Los valores de texto son escapados cuando contienen comas, comillas o
saltos de línea para conservar un CSV válido.

`TotalHoras` utiliza `CultureInfo.InvariantCulture` para garantizar el
uso del punto como separador decimal y evitar conflictos con la coma
utilizada para separar las columnas del archivo.

------------------------------------------------------------------------

# Consideraciones sobre los datos

Durante la revisión de los datos proporcionados se identificó que el
catálogo de áreas contiene valores repetidos para un mismo `IDArea`.

Debido a que no existe información adicional que permita determinar de
alguna manera cuál de las áreas duplicadas corresponde a cada
usuario, para la generación del CSV se toma la primera coincidencia
encontrada para dicho identificador.

Esta decisión se mantiene de forma determinística y evita inferir
información que no se encuentra disponible en el conjunto de datos
proporcionado.

------------------------------------------------------------------------

# Estructura del proyecto

``` text
Nuxiba.Api/
├── Controllers/
│   ├── LoginsController.cs
│   └── ReportsController.cs
├── Data/
│   └── AppDbContext.cs
├── DTOs/
│   ├── CreateLoginDTO.cs
│   ├── UpdateLoginDTO.cs
│   └── WorkedHoursReportDTO.cs
├── Migrations/
│   ├── InitialCreate
│   ├── AddUsersAndAreas
│   └── FixAreaNameColumn
├── Models/
│   ├── Area.cs
│   ├── Login.cs
│   └── User.cs
├── appsettings.json
├── Nuxiba.Api.http
└── Program.cs
```

------------------------------------------------------------------------

# Decisiones técnicas

## Uso de DTO

Los endpoints de escritura utilizan DTO para separar el contrato HTTP de
las entidades persistidas y evitar que el cliente controle propiedades
generadas por SQL Server.

## AsNoTracking

Se utiliza `AsNoTracking()` en consultas de solo lectura para evitar el
seguimiento de entidades cuando no es necesario modificar los
resultados.

## Cálculo de sesiones

Una sesión válida está formada por un movimiento `TipoMov = 1` seguido
cronológicamente por un movimiento `TipoMov = 0` del mismo usuario.

En SQL este comportamiento se implementa mediante `LEAD()`. Para el
reporte CSV se reproduce la misma regla desde la API agrupando y
ordenando los movimientos por usuario.

## Migraciones

El esquema se administra mediante migraciones de Entity Framework Core,
lo que permite reproducir la estructura de la base de datos a partir del
código del proyecto.

------------------------------------------------------------------------

# Ejecución rápida

Una vez configurada la cadena de conexión y cargados los datos:

``` bash
dotnet restore
dotnet run
```

Después se puede utilizar Swagger para probar:

``` text
GET    /logins
GET    /logins/{id}
POST   /logins
PUT    /logins/{id}
DELETE /logins/{id}

GET    /reports/worked-hours/csv
```

------------------------------------------------------------------------

# Autor

**Steven Arturo Escárcega Hernández**
