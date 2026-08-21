# Quickstart — Puesta en marcha en una máquina limpia

**Feature**: 001-auth-rbac-foundation

Este documento describe el estado objetivo: cómo se pondrá en marcha la solución cuando las tareas
de la Fase 2 estén implementadas. Hoy la solución no existe todavía; los pasos 1 a 3 sí son
ejecutables, porque los scripts SQL ya están en el repositorio.

---

## Requisitos previos

- SDK de .NET 10
- SQL Server con autenticación mixta habilitada, si se va a usar el login de aplicación
- `sqlcmd`

> **Instancia.** Los comandos usan `-S localhost`, pero la instancia de referencia del proyecto es
> `(localdb)\MSSQLLocalDB` (SQL Server Express LocalDB), porque no exige permisos de administrador
> para nada: la arranca y reinicia el propio usuario, y ya trae autenticación mixta habilitada.
> Sustituye `-S localhost` por `-S '(localdb)\MSSQLLocalDB'` para trabajar contra ella. Ver
> `Scripts/SQL/README.md` para el detalle de ambas instancias.
>
> Añade `-f 65001` a todos los `sqlcmd`: los scripts están en UTF-8 y sin ese switch las tildes se
> corrompen al insertarse.

---

## 1. Crear la base de datos

Contra `master`, con autenticación de Windows de un administrador:

```bash
sqlcmd -S localhost -E -d master -i Scripts/SQL/000_crear_base_datos.sql
```

Crea `OpticaDB` si no existe y activa `READ_COMMITTED_SNAPSHOT`. Es idempotente.

## 2. Aplicar el esquema

```bash
sqlcmd -S localhost -E -d OpticaDB -i Scripts/SQL/001_modelo_datos_optica.sql
```

Crea las 40 tablas del modelo. Idempotente: cada tabla se crea solo si no existe.

**Este script es la fuente de verdad del modelo de datos** (principio X). No se modifica para
acomodar el código; el código se acomoda a él.

## 3. Sembrar los catálogos

```bash
sqlcmd -S localhost -E -d OpticaDB -i Scripts/SQL/002_seed_catalogos_merge.sql
```

Siembra estados de orden, métodos de pago, filtros de lente, tipos de identificación y **los tres
roles**: `Administrador`, `Vendedor`, `Optometra`. Sin este paso no se puede asignar ningún rol.

## 4. Crear el login de aplicación (opcional en desarrollo)

Solo si se prefiere conectar con usuario y contraseña de SQL en lugar de autenticación de Windows:

```bash
sqlcmd -S localhost -E -d master -i Scripts/SQL/003_login_aplicacion.sql -v Clave="<clave>"
```

Crea el login `optica_app` con permisos de lectura, escritura y ejecución, **deliberadamente sin
derechos de definición de esquema**: los cambios de estructura se aplican con los scripts numerados
usando una cuenta administrativa. La clave se pasa por variable y nunca queda en el archivo, como
exige el principio VI.

## 5. Configurar la conexión y los secretos

La cadena de conexión de desarrollo ya está en
`src/3. Presentation/Optica.Web/appsettings.Development.json`, apuntando a LocalDB con
autenticación integrada de Windows:

```
Server=(localdb)\MSSQLLocalDB;Database=OpticaDB;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=True;Application Name=Optica.Web
```

Está versionada porque **no contiene credenciales**: la identidad la aporta el usuario de Windows
que ejecuta el proceso. `appsettings.json` sólo declara la clave `ConnectionStrings:OpticaDB` vacía,
para que el contrato de configuración sea explícito y cada entorno la sobrescriba.

Lo que sí es secreto nunca va a un archivo versionado (principio VI, decisión D-08). Eso incluye la
clave de firma de tokens, y la cadena de conexión **sólo cuando lleva usuario y contraseña**, por
ejemplo si se decide correr contra `optica_app` en lugar de autenticación integrada:

```bash
cd "src/3. Presentation/Optica.Web"
dotnet user-secrets set "Jwt:SigningKey" "<al menos 32 bytes aleatorios>"
# Sólo si se usa el login SQL en vez de autenticación de Windows:
dotnet user-secrets set "ConnectionStrings:OpticaDB" "Server=localhost;Database=OpticaDB;User ID=optica_app;Password=<clave>;TrustServerCertificate=True"
```

El gestor de secretos tiene precedencia sobre `appsettings.Development.json`, así que basta
definirla ahí para sobrescribir la de autenticación integrada.

La aplicación **falla al arrancar** si falta la clave de firma o mide menos de 32 bytes. Es
deliberado: generar una clave temporal en silencio produce el escenario en que cada reinicio
invalida todas las sesiones, o peor, un despliegue firmando con una clave conocida.

## 6. Compilar y probar

```bash
dotnet build OpticaSolution.sln
dotnet test
```

La compilación trata las advertencias como errores. La suite incluye la prueba de arquitectura que
falla ante cualquier violación de la regla de dependencias.

Con reporte de cobertura y verificación de umbrales:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

El proceso falla por debajo de 85% global o de 90% en `Optica.Domain` y `Optica.Application`
(principio IV, compuerta G3).

Las pruebas de integración necesitan una base de datos de pruebas propia, creada con los mismos
scripts de los pasos 1 a 3. Su cadena de conexión también va en el gestor de secretos.

## 7. Crear el primer Administrador

El sistema arranca sin ningún usuario y el ingreso no es posible hasta que exista uno. No hay
usuario sembrado ni contraseña por defecto (decisión D-12):

```bash
cd "src/3. Presentation/Optica.Web"
dotnet run -- crear-admin --nombre "Ana" --apellido "Gómez" --usuario "agomez"
```

Pide la contraseña por entrada interactiva, no por argumento, para que no quede en el historial del
intérprete de órdenes. Crea el empleado y su usuario con rol `Administrador`, y registra la acción
en auditoría.

Se ejecuta **una sola vez** por instalación. A partir de ahí, los usuarios se crean desde la
aplicación, con la feature del módulo RF-USR.

## 8. Ejecutar la aplicación

```bash
dotnet run
```

La pantalla de ingreso queda en `/login`. El resto de pantallas llega con sus módulos: el alcance de
interfaz de esta feature es exactamente una pantalla.

---

## Verificación rápida de que todo quedó bien

| Comprobación | Resultado esperado |
|---|---|
| Ingreso con las credenciales del primer administrador | Acceso concedido, cookies `optica_at` y `optica_rt` presentes y marcadas `HttpOnly` |
| Ingreso con contraseña incorrecta | Mensaje "Usuario o contraseña incorrectos" |
| Ingreso con un usuario que no existe | **El mismo** mensaje, en un tiempo similar |
| Cinco intentos fallidos seguidos | Cuenta bloqueada; la contraseña correcta también se rechaza durante 15 minutos |
| Consulta a `SELECT PASSWORD_HASH, TOKEN_HASH ...` | Solo valores binarios; ninguna contraseña ni credencial en claro |
| `GET /api/roles` sin sesión | 401 |
| `GET /api/roles` con sesión de Vendedor | 403 |

---

## Problemas frecuentes

| Síntoma | Causa probable |
|---|---|
| El arranque falla diciendo que falta la clave de firma | Paso 5 pendiente. Es el comportamiento correcto, no un defecto |
| El ingreso responde 401 con credenciales que se saben correctas | La cuenta puede estar bloqueada por intentos previos. El mensaje es genérico a propósito (FR-004); confirmar en `LoginAttempts` y en `BLOQUEADO_HASTA` |
| No se puede asignar ningún rol | Paso 3 pendiente: la tabla `Roles` está vacía |
| `crear-admin` falla por clave foránea | El esquema del paso 2 no se aplicó completo; `Usuarios` exige un `EMPLEADO_ID` válido |
| Las pruebas de integración fallan todas a la vez | Falta la base de datos de pruebas o su cadena de conexión en el gestor de secretos |
