# Phase 0 — Investigación y decisiones técnicas

**Feature**: 001-auth-rbac-foundation
**Fecha**: 2026-08-20
**Entrada**: [spec.md](./spec.md), [constitution.md](../../.specify/memory/constitution.md) v2.0.0, [ADR-001](../../docs/adr/ADR-001-modelo-presentacion-blazor-web-app.md)

Cada decisión resuelve un punto que la especificación deja abierto en el plano técnico. La
especificación no se modifica: aquí solo se elige el *cómo*.

---

## D-01 — Algoritmo de hash de contraseñas

**Decisión**: PBKDF2 con HMAC-SHA256, 600.000 iteraciones, salt aleatorio de 128 bits por
usuario, clave derivada de 256 bits. Implementado con `Rfc2898DeriveBytes` de la biblioteca base.

**Rationale**: el esquema existente ya fija la forma de almacenamiento en
`ADMINISTRACION_USUARIOS.Usuarios`: `PASSWORD_HASH varbinary(256)` y `PASSWORD_SALT
varbinary(128)`, es decir hash y salt en columnas separadas. Eso descarta de entrada los
formatos que empaquetan parámetros y salt en una sola cadena. PBKDF2 encaja sin tocar el
esquema, viene en la biblioteca base sin dependencias externas, y 600.000 iteraciones es la
recomendación vigente de OWASP para PBKDF2-HMAC-SHA256. El costo medido esperado ronda los
200–400 ms, holgadamente dentro del presupuesto de 3 segundos de SC-001.

**Alternativas descartadas**:

- **Argon2id**: es preferible en teoría por su resistencia a ataques con hardware dedicado, pero
  exige un paquete externo (`Konscious.Security.Cryptography`) y su salida natural es una cadena
  con parámetros embebidos, que no calza con las dos columnas binarias del esquema. Adoptarlo
  obligaría a proponer un cambio de esquema y a violar el principio X sin necesidad.
- **`ASP.NET Core Identity` con su `PasswordHasher`**: arrastra todo el modelo de datos de
  Identity, que duplicaría `Usuarios` y `Roles` ya modelados.
- **bcrypt**: límite de 72 bytes en la contraseña y también paquete externo.

**Nota de riesgo**: el número de iteraciones DEBE ser configurable y persistirse junto al hash en
una futura columna de versión de algoritmo. Hoy no existe esa columna; mientras el algoritmo sea
único no hace falta, pero un cambio futuro de algoritmo sí requerirá proponer el cambio de
esquema según el principio X.

---

## D-01a — Longitud de la contraseña: entre 8 y 12 caracteres

**Decisión**: rango de 8 a 12 caracteres, ambos inclusive (FR-003a). Decidido por el responsable
del proyecto el 2026-08-20.

**Rationale de la forma de aplicarlo**: el rango completo se valida donde la contraseña se
establece; en el ingreso solo se valida el tope. Enforzar también el mínimo en el ingreso
convertiría una contraseña corta en un 400, distinguible del 401 de credencial incorrecta, lo que
revela la política y abre una grieta en el mensaje genérico único que exige FR-004.

**Origen del valor anterior, y por qué era indefendible**: el contrato declaraba antes un tope de
256 caracteres. Ese número no venía del esquema —la contraseña no se almacena, solo su derivación
de tamaño fijo—, ni de la especificación, ni de ningún estándar. Con toda probabilidad se ancló en
la columna `PASSWORD_HASH varbinary(256)`, cuyos 256 son **bytes del hash** y no tienen relación
alguna con caracteres de contraseña. El análisis de valores límite de QA lo señaló como el único
límite del documento sin respaldo verificable.

**Desviación de los estándares, aceptada de forma explícita**: NIST SP 800-63B exige que el
verificador admita **al menos 64 caracteres**, y OWASP ASVS recomienda lo mismo. Un tope de 12
incumple ambos y, sobre todo, **impide las frases de contraseña**, que son la vía más simple para
que una persona tenga una credencial fuerte y memorable. El riesgo residual queda acotado por dos
mecanismos ya presentes en la feature: el bloqueo tras cinco intentos fallidos (FR-017) hace
impracticable el ataque en línea, y las 600.000 iteraciones de D-01 encarecen el ataque fuera de
línea si la base se filtrara.

**Lo que NO es una razón para acotar la longitud**: el costo de la derivación. En
PBKDF2-HMAC-SHA256 la contraseña actúa como clave HMAC y, si excede el tamaño de bloque, se
comprime con un único hash a 32 bytes antes de que corran las 600.000 iteraciones sobre esa clave
de tamaño fijo. Una contraseña de un megabyte añade un hash, no seiscientos mil. Ese argumento
aplicaría a bcrypt, no al algoritmo de D-01.

**Alternativas descartadas**: 128 caracteres, el tope que OWASP considera aceptable, y 64, el
mínimo que NIST obliga a admitir. Se descartaron por decisión del responsable del proyecto.

**Punto de revisión**: si el sistema se expone a internet o se audita contra un marco de
cumplimiento, este tope debe revisarse. Cambiarlo es modificar un único valor: ni el esquema ni el
algoritmo dependen de la longitud de entrada.

---

## D-02 — Igualación de tiempos de respuesta (SC-004)

**Decisión**: cuando el nombre de usuario no existe, el flujo de autenticación ejecuta igualmente
una derivación PBKDF2 contra un hash y salt señuelo constantes, y descarta el resultado.

**Rationale**: SC-004 exige que la diferencia de tiempo entre "usuario inexistente" y "contraseña
incorrecta" quede por debajo de 100 ms, para que no se puedan enumerar cuentas. Sin el señuelo la
diferencia es justamente el costo del hash, entre 200 y 400 ms según D-01, es decir un canal
lateral perfectamente medible. Es la contramedida estándar y no requiere infraestructura.

**Alternativas descartadas**:

- **Retardo aleatorio**: añade ruido pero no elimina la diferencia de medias; con suficientes
  muestras el atacante la recupera.
- **Retardo fijo hasta un presupuesto total**: funciona, pero fija el tiempo de respuesta de todo
  login al peor caso y castiga la experiencia del usuario legítimo.

---

## D-03 — Acceso a datos

**Decisión**: EF Core 10 con configuraciones explícitas mapeadas a las tablas existentes.
**Migraciones deshabilitadas**: el esquema se aplica exclusivamente con los scripts numerados de
`Scripts/SQL/`.

**Rationale**: el principio X declara el script SQL como fuente de verdad, así que el flujo normal
de EF —modelo primero, migración después— queda invertido: aquí el modelo se somete al esquema.
Lo que EF sí aporta y pesa en esta feature es el interceptor de `SaveChanges`, que permite
escribir el registro de auditoría en la **misma transacción** que la operación auditada, lo cual
es exactamente lo que exige la compuerta G7. Construir eso a mano sobre un micro-ORM significaría
reimplementar el seguimiento de cambios para obtener el "valor antes" que pide el principio VII.

**Alternativas descartadas**:

- **Dapper solo**: más liviano y más directo sobre el esquema existente, pero obliga a capturar
  manualmente el estado previo de cada entidad para la auditoría, en cada caso de uso. El costo se
  paga en las 25 features siguientes, no solo en esta.
- **EF Core con migraciones**: contradice el principio X de forma directa.
- **EF Core para escrituras y Dapper para lecturas**: es el patrón que probablemente convenga
  cuando lleguen las consultas del dashboard y del POS. Se deja como puerta abierta, no se
  introduce ahora: esta feature no tiene ninguna consulta cuyo rendimiento lo justifique, y dos
  pilas de acceso a datos desde el día uno es complejidad sin problema que la respalde.

**Verificación de compatibilidad con el esquema**: las columnas `USUARIO_CREACION`,
`FECHA_CREACION`, `USUARIO_ACTUALIZACION` y `FECHA_ACTUALIZACION` existen en todas las tablas y se
poblarán desde el mismo interceptor, tomando el usuario del contexto de la petición y la hora de
`TimeProvider`.

---

## D-04 — Transporte de las credenciales de sesión

**Decisión**: el token de acceso y la credencial de renovación viajan en **cookies HttpOnly,
Secure, SameSite=Strict**. El token de acceso conserva el formato JWT firmado. El JavaScript de la
página nunca ve ninguna de las dos credenciales.

| Cookie | Contenido | Vida | Alcance |
|---|---|---|---|
| `optica_at` | JWT de acceso firmado | 15 minutos (FR-008) | `/` |
| `optica_rt` | Credencial opaca de 256 bits aleatorios | 8 horas (FR-009) | `/` |

**Rationale**: ADR-001 fija una Blazor Web App con render en servidor para la autenticación e
islas WebAssembly para el POS. Esos dos mundos necesitan la misma credencial: el render en
servidor la necesita en cada navegación, y las islas la necesitan en cada llamada a un endpoint.
La cookie es el único transporte que ambos obtienen sin código adicional, porque el navegador la
envía sola y el mismo origen sirve las páginas y los endpoints. Y al ser `HttpOnly`, una
vulnerabilidad de XSS en cualquier pantalla no permite robar la sesión, que es el fallo habitual
de guardar el token en `localStorage`.

El JWT se conserva como formato del token de acceso —no se sustituye por una cookie de sesión
tradicional— porque FR-015 exige que transporte identificador y roles, y FR-032 se apoya
justamente en que la autorización se resuelve con lo que el token trae dentro, sin consultar la
base de datos en cada petición.

**Desviación respecto a la historia**: RF-LOG-01 dibuja `POST /api/auth/login` devolviendo "JWT
corto" en el cuerpo de la respuesta. Aquí el endpoint **no devuelve el token en el cuerpo**: emite
las cookies y responde solo con los metadatos de vigencia. Devolverlo en el cuerpo obligaría al
cliente a guardarlo en algún sitio accesible desde JavaScript, que es precisamente lo que esta
decisión evita. La ruta, el método y los códigos de estado de la historia se mantienen.

**Alternativas descartadas**:

- **Token en el cuerpo y almacenado en `localStorage`**: fiel a la letra de la historia, pero
  cualquier XSS entrega la sesión completa al atacante.
- **Cookie de autenticación clásica de ASP.NET Core, sin JWT**: sería más simple para el render en
  servidor, pero deja a las islas WebAssembly sin los roles en el cliente y contradice FR-007 a
  FR-016, que están escritos sobre un par token/credencial de renovación.
- **`SameSite=Lax`**: innecesario. La aplicación no recibe navegaciones entrantes de terceros que
  deban llegar autenticadas.

**Consecuencia**: al usar cookies, toda petición que modifique estado DEBE llevar token
antifalsificación. Se activa el antiforgery de ASP.NET Core en los endpoints de escritura.

---

## D-05 — Detección de reutilización de credencial rotada, sin cambiar el esquema

**Decisión**: la reutilización se detecta por el estado de la fila, sin columnas nuevas. Al
presentarse una credencial de renovación: si su hash existe y `REVOKED_AT` **no** es nulo, se
trata como reutilización de una credencial ya rotada y se revocan todas las credenciales activas
de ese usuario (FR-013).

**Rationale**: FR-011 exige rotar en cada uso y FR-013 exige revocar toda la cadena al detectar
reutilización. La implementación de libro añade una columna de encadenamiento del tipo
`REPLACED_BY_ID`, que **no existe** en `ADMINISTRACION_USUARIOS.RefreshTokens`. Resulta que no es
necesaria: la rotación marca `REVOKED_AT` en la fila consumida y crea una nueva; por tanto
encontrar una fila con `REVOKED_AT` poblado significa, inequívocamente, que alguien presentó una
credencial ya consumida o revocada. Eso basta para disparar la revocación en cascada por
`USUARIO_ID`, que es el alcance que pide FR-013.

Se pierde la capacidad de reconstruir el árbol de rotaciones para un análisis forense fino. Es un
costo aceptable frente a proponer un cambio de esquema, y la traza de intentos y auditoría cubre
la necesidad operativa.

**Confirmación del principio X**: esta feature **no requiere ningún cambio de esquema**. La
compuerta G10 queda en verde.

---

## D-06 — Conteo de intentos fallidos bajo concurrencia (FR-023)

**Decisión**: el incremento del contador y la evaluación del bloqueo se resuelven en **una sola
sentencia `UPDATE` atómica** con cláusula `OUTPUT`, no con el patrón leer-modificar-escribir desde
la aplicación.

**Rationale**: el caso borde de la especificación es explícito: dos intentos simultáneos sobre la
misma cuenta no deben contar de menos ni bloquear antes del quinto. Leer el contador, sumar uno en
memoria y guardar es una condición de carrera clásica que pierde incrementos. Una sentencia que
incrementa y decide el bloqueo en el mismo enunciado deja la serialización en manos del motor.
`READ_COMMITTED_SNAPSHOT` ya está activo en la base según `000_crear_base_datos.sql`, lo que hace
más probable, no menos, que el patrón ingenuo falle.

**Verificación**: prueba de integración que lanza N intentos fallidos concurrentes y comprueba que
el contador final es exactamente N y que el bloqueo ocurre en el quinto.

---

## D-07 — Fuente de tiempo

**Decisión**: `TimeProvider` de la biblioteca base, inyectado, siempre en UTC. Prohibido
`DateTime.Now` y `DateTime.UtcNow` en dominio y aplicación.

**Rationale**: el principio II prohíbe al dominio acceder al reloj del sistema y el principio X
exige UTC sobre una única fuente de tiempo. `TimeProvider` es la abstracción que ya trae la
plataforma, con `FakeTimeProvider` para pruebas, lo que permite verificar el vencimiento del
bloqueo de 15 minutos y el de las credenciales sin esperas reales en la suite. El caso borde del
desfase de reloj entre aplicación y base de datos se resuelve calculando siempre en la aplicación
y enviando el valor calculado, en lugar de mezclar `SYSUTCDATETIME()` con la hora del proceso.

---

## D-08 — Custodia del secreto de firma del JWT

**Decisión**: en desarrollo, gestor de secretos de usuario (`dotnet user-secrets`). En despliegue,
variable de entorno. Nunca en `appsettings.json` versionado. El arranque **falla** si el secreto
falta o mide menos de 32 bytes.

**Rationale**: el principio VI prohíbe secretos en código y en configuración versionada. Fallar el
arranque, en lugar de generar una clave temporal, evita el escenario silencioso en que cada
reinicio invalida todas las sesiones o, peor, un despliegue queda firmando con una clave conocida.

---

## D-09 — Prueba de arquitectura (compuerta G1)

**Decisión**: `NetArchTest.Rules` en un proyecto de pruebas dedicado, con un caso por fila de la
tabla de referencias permitidas del principio II, más una prueba que verifica que
`Optica.Domain` no referencia tipos de EF Core, `IConfiguration`, `HttpContext` ni `DateTime`.

**Rationale**: el principio II exige de forma literal que "una prueba de arquitectura automatizada
DEBE fallar la compilación" ante una violación. Una revisión humana no satisface el enunciado.

**Alternativa descartada**: analizador de Roslyn propio. Más potente y más caro de construir y
mantener; la tabla de dependencias es simple y estable.

---

## D-10 — Verificación de que `Optica.Shared` compila para WebAssembly

**Decisión**: `Optica.Shared` se declara con destinos múltiples y el proyecto
`Optica.Web.Client` lo referencia, de modo que la compilación de la solución ya ejercita el
destino WebAssembly. Además, una prueba de arquitectura prohíbe en `Optica.Shared` los espacios
de nombres exclusivos de servidor.

**Rationale**: ADR-001 advierte que este principio "deja de ser una formalidad y pasa a ser una
restricción que muerde". Si nadie compila `Shared` para WebAssembly, la violación aparece meses
después, en la feature del POS.

---

## D-11 — Pruebas de integración contra base de datos real

**Decisión**: base de datos de pruebas dedicada, creada con los mismos scripts numerados, con
cadena de conexión tomada del gestor de secretos. Limpieza de estado entre pruebas con `Respawn`.

**Rationale**: las reglas que esta feature debe probar —atomicidad del contador de intentos,
auditoría en la misma transacción, unicidad de nombre de usuario, integridad referencial— viven en
el motor de base de datos. Verificarlas contra un doble en memoria probaría el doble, no el
sistema. Usar los mismos scripts que producción garantiza que se prueba el esquema real.

**Alternativa descartada**: `Testcontainers` con una imagen de SQL Server. Es más reproducible y
mejor para integración continua, pero exige Docker disponible en cada máquina de desarrollo, y el
flujo actual del proyecto —`sqlcmd` contra `localhost` con autenticación de Windows, según los
scripts `000` y `003`— indica instancias locales instaladas. Se deja anotado como mejora para
cuando exista integración continua.

**Nota**: el proveedor en memoria de EF Core no se usa en ninguna prueba de esta feature, porque no
respeta restricciones de unicidad ni transacciones.

---

## D-12 — Primer Administrador (FR-035)

**Decisión**: comando de línea de órdenes del proyecto web, invocado a mano una sola vez, que crea
el empleado y su usuario con rol Administrador y exige la contraseña como argumento o por entrada
interactiva. No se siembra ningún usuario desde SQL ni desde el arranque de la aplicación.

**Rationale**: `Usuarios.EMPLEADO_ID` es obligatorio y con clave foránea a `Empleados`, así que el
primer administrador implica crear dos filas relacionadas. Hacerlo por SQL exigiría calcular el
hash PBKDF2 fuera de la aplicación, es decir duplicar D-01 en un script. Hacerlo desde el arranque
con una contraseña por defecto deja una credencial conocida en cualquier instalación olvidada.
Un comando explícito reutiliza el mismo servicio de hash del flujo normal y no deja rastro de la
contraseña en el repositorio.

**Alternativa descartada**: sembrar el usuario en `002_seed_catalogos_merge.sql`, junto a los
roles. Los roles sí son catálogo y ya están ahí, correctamente; una credencial no lo es.

---

## D-13 — Pruebas de navegador de la pantalla de ingreso (SC-001, compuerta G9)

**Decisión**: automatizar con **Playwright** los casos de extremo a extremo de la pantalla de
ingreso, en un proyecto de prueba propio `tests/Optica.E2E.Tests/`, excluido del cálculo de
cobertura por capa y etiquetado para no correr en la compilación local rápida. Los quince casos
están especificados en
[qa/001-auth-rbac-foundation/casos-de-prueba/login-e2e-playwright.md](../../qa/001-auth-rbac-foundation/casos-de-prueba/login-e2e-playwright.md).

**Rationale**: dos exigencias de esta feature no son verificables por debajo del navegador.

1. **SC-001** mide el recorrido completo desde el envío del formulario hasta ver la pantalla
   principal, en menos de 3 segundos. Incluye render, red y latencia del cliente. Medido en el
   servidor se mide otra cosa, y por eso la trazabilidad de QA lo tenía como el único criterio de
   éxito sin cubrir.
2. La **compuerta G9** exige los cuatro estados de la vista, recorrido por teclado con foco
   visible, contraste mínimo y ausencia de información portada solo por el color. Todo eso es
   estado de interfaz, no de respuesta HTTP. Hasta ahora se apoyaba únicamente en una carta de
   exploración manual.

**Alcance deliberadamente acotado**: solo la pantalla de ingreso, que es la única de la feature
según la propia evaluación de G9 en el plan. Los casos de reglas del usuario, del handler y del
hasheo **no** se pasan a Playwright: no son observables desde el navegador, su cobertura no es
atribuible a una capa y por tanto no sirve para los umbrales del principio IV, y costarían segundos
donde su equivalente unitario cuesta milisegundos.

**Dos límites del enfoque, explícitos**:

- **Playwright no puede mover el reloj del servidor.** Los vencimientos de esta feature —bloqueo a
  los 15 minutos, token de acceso a los 15, sesión a las 8 horas— siguen probándose en integración,
  donde el reloj se inyecta (D-07). Por navegador sí se **provoca** el bloqueo con cinco envíos,
  que no requiere avanzar el tiempo.
- **Playwright no mide la igualación de tiempos de SC-004.** Los 100 milisegundos se miden a nivel
  HTTP; añadir render y pintado introduce más varianza que la magnitud a medir.

**Marco de pruebas: NUnit, solo en este proyecto.** La suite de navegador usa
`Microsoft.Playwright.NUnit`, que es la integración oficial de Playwright para .NET. Aporta la clase
base que abre un contexto de navegador aislado por prueba, el cierre determinista al terminar, y la
captura de traza, video y captura de pantalla por configuración en lugar de por código. Escribir
ese andamiaje a mano sobre otro marco significa mantener nosotros lo que el paquete oficial ya
mantiene, y el aislamiento por prueba es justo la parte que, mal hecha, produce suites de navegador
intermitentes.

**Desviación del principio IV, registrada**: la constitución fija *"xUnit para .NET"* como marco de
pruebas. Esta decisión introduce NUnit **exclusivamente en `tests/Optica.E2E.Tests/`**; los cuatro
proyectos de dominio, aplicación, integración y arquitectura siguen en xUnit sin excepción. La
desviación queda registrada en Complexity Tracking del plan, según exige el apartado de
cumplimiento de la constitución. Es una desviación acotada a un proyecto que además está fuera del
cálculo de cobertura, así que no afecta a los umbrales del principio IV ni a la ejecución rápida.

Si se prefiere uniformidad de marco por encima del andamiaje oficial, la alternativa es escribir el
arranque y cierre del navegador a mano sobre xUnit —unas veinte líneas más el aislamiento por
prueba— y esa es la opción que esta decisión descarta.

**Requisitos de entorno**: host levantado sobre **HTTPS**, porque las cookies llevan `Secure` y
sobre HTTP el navegador las descarta; base de datos sembrada con las cuentas de prueba; y limpieza
de estado entre casos, porque el contador de intentos fallidos y el bloqueo persisten en la fila
del usuario. Reutiliza el andamiaje de D-11 y T026.

**Diagnóstico de fallos**: conservar captura de pantalla, video y traza de Playwright en cada fallo.
Sin ellos, un fallo en integración continua es irreproducible.
