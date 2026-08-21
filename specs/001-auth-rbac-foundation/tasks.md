# Tasks: Fundación de Solución y Autenticación con Control de Acceso

**Input**: Design documents from `specs/001-auth-rbac-foundation/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/)

**Tests**: Las tareas de prueba son **OBLIGATORIAS** en este proyecto. El principio IV de la
constitución (NO NEGOCIABLE) exige ≥85% de cobertura global y ≥90% en dominio y aplicación, y que
cada escenario de aceptación tenga al menos una prueba. La matriz de
[contracts/trazabilidad.md](./contracts/trazabilidad.md) fija 41 pruebas previstas; ninguna se
omite.

**Organization**: agrupadas por historia de usuario, para que cada una se implemente y se pruebe de
forma independiente.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: puede ejecutarse en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: historia a la que pertenece la tarea (US1 a US5)
- Cada descripción incluye la ruta exacta del archivo

## Path Conventions

Estructura real de la solución, fijada por
[ESPECIFICACION_TECNICA.md](../../HU/Historias_Tecnicas/ESPECIFICACION_TECNICA.md) y obligatoria
según el principio II. **Atención**: los nombres de carpeta llevan espacios (`1. Core`).

```text
src/1. Core/Optica.Domain/            src/2. Infrastructure/Optica.Infrastructure/
src/1. Core/Optica.Application/       src/2. Infrastructure/Optica.Shared/
src/3. Presentation/Optica.Web/       src/3. Presentation/Optica.Web.Client/
tests/Optica.Domain.Tests/            tests/Optica.Integration.Tests/
tests/Optica.Application.Tests/       tests/Optica.Architecture.Tests/
```

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: crear la solución y los diez proyectos con las referencias exactas del principio II.

- [x] T001 Crear `OpticaSolution.sln` en la raíz del repositorio con las carpetas de solución `1. Core`, `2. Infrastructure`, `3. Presentation` y `tests`
- [x] T002 Crear `Directory.Build.props` en la raíz con `TreatWarningsAsErrors`, `EnableNETAnalyzers`, `AnalysisLevel latest`, `Nullable enable` y `LangVersion` de C# 14
- [x] T003 [P] Crear proyecto de biblioteca `src/1. Core/Optica.Domain/Optica.Domain.csproj` sin ninguna referencia de paquete
- [x] T004 [P] Crear proyecto `src/1. Core/Optica.Application/Optica.Application.csproj` con MediatR y FluentValidation
- [x] T005 [P] Crear proyecto `src/2. Infrastructure/Optica.Infrastructure/Optica.Infrastructure.csproj` con EF Core 10 y el proveedor de SQL Server
- [x] T006 [P] Crear proyecto `src/2. Infrastructure/Optica.Shared/Optica.Shared.csproj` sin dependencias exclusivas de servidor, verificando que compila para WebAssembly (decisión D-10)
- [x] T007 Crear la Blazor Web App `src/3. Presentation/Optica.Web/Optica.Web.csproj` con MudBlazor y modo de render por componente
- [x] T008 Crear `src/3. Presentation/Optica.Web.Client/Optica.Web.Client.csproj` que referencie únicamente `Optica.Shared`; queda sin componentes en esta feature y existe para forzar la compilación a WebAssembly
- [x] T009 Declarar las referencias entre proyectos exactamente según la tabla del principio II en `.specify/memory/constitution.md`, sin ninguna referencia adicional
- [x] T010 [P] Crear los cuatro proyectos de prueba con xUnit y coverlet: `tests/Optica.Domain.Tests/`, `tests/Optica.Application.Tests/`, `tests/Optica.Integration.Tests/`, `tests/Optica.Architecture.Tests/`
- [x] T011 [P] Configurar los umbrales de cobertura en `Directory.Build.props` de modo que el proceso falle por debajo de 85% global y 90% en dominio y aplicación (compuerta G3)
- [x] T012 [P] Documentar las claves de configuración esperadas en `src/3. Presentation/Optica.Web/appsettings.json` **sin ningún valor secreto**, y añadir `.gitignore` para artefactos de compilación

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: infraestructura transversal sin la cual ninguna historia puede cerrar sus criterios.

**⚠️ CRÍTICO**: ninguna historia de usuario puede empezar hasta que esta fase esté completa.

- [x] T013 Implementar la prueba de arquitectura en `tests/Optica.Architecture.Tests/ReglaDeDependenciasTests.cs` con `NetArchTest.Rules`, un caso por fila de la tabla del principio II (compuerta G1)
- [x] T014 Añadir a `tests/Optica.Architecture.Tests/DominioAisladoTests.cs` las pruebas que prohíben en `Optica.Domain` los tipos de EF Core, `IConfiguration`, `HttpContext` y `DateTime`
- [ ] T015 [P] Crear los tipos base del dominio en `src/1. Core/Optica.Domain/Comun/`: resultado de operación, excepciones de dominio y tipos de valor, todos inmutables (principio V)
- [ ] T016 [P] Declarar las abstracciones de la capa de aplicación en `src/1. Core/Optica.Application/Abstracciones/`: repositorios, `IHasheadorDeContrasena`, `IEmisorDeToken` y unidad de trabajo
- [ ] T017 Registrar MediatR y los comportamientos de canal en `src/1. Core/Optica.Application/Comportamientos/`: validación previa al handler, log con identificador de correlación y transacción
- [ ] T018 Crear `src/2. Infrastructure/Optica.Infrastructure/Persistencia/OpticaDbContext.cs` **sin migraciones**, con las configuraciones mapeadas a las tablas existentes de `ADMINISTRACION_USUARIOS` y `AUDITORIA` (decisión D-03, principio X)
- [ ] T019 Implementar el interceptor de auditoría en `src/2. Infrastructure/Optica.Infrastructure/Persistencia/Interceptores/AuditoriaInterceptor.cs`, que escribe en `AUDITORIA.LogUsuarios` dentro de la misma transacción y **excluye de forma explícita** `PASSWORD_HASH`, `PASSWORD_SALT` y `TOKEN_HASH` (compuerta G7, regla R-A3)
- [ ] T020 Implementar el interceptor que puebla `USUARIO_CREACION`, `FECHA_CREACION`, `USUARIO_ACTUALIZACION` y `FECHA_ACTUALIZACION` desde `TimeProvider` y el usuario de la petición (principio VII)
- [ ] T021 [P] Registrar `TimeProvider` en la inyección de dependencias y prohibir `DateTime.Now` y `DateTime.UtcNow` mediante una regla del analizador (decisión D-07)
- [ ] T022 Implementar el validador de configuración en `src/2. Infrastructure/Optica.Infrastructure/Configuracion/ValidadorDeProveedoresExternos.cs`, que **falla el arranque** ante cualquier proveedor de identidad externo declarado (FR-002, compuerta G5)
- [ ] T023 Validar en el arranque que la clave de firma del token existe y mide al menos 32 bytes, fallando de forma explícita si no (decisión D-08)
- [ ] T024 [P] Configurar el log estructurado con identificador de correlación por petición y el filtro que impide registrar contraseñas, tokens y credenciales de renovación (compuerta G8)
- [ ] T025 [P] Implementar el middleware de errores que traduce fallos a `application/problem+json` con los códigos de los contratos
- [ ] T026 Crear el andamiaje de pruebas de integración en `tests/Optica.Integration.Tests/Infraestructura/`: fábrica de host, base de datos de pruebas y limpieza de estado con `Respawn` (decisión D-11)

**Checkpoint**: la solución compila, la prueba de arquitectura pasa y las pruebas de integración pueden hablar con SQL Server.

---

## Phase 3: User Story 1 - Ingreso al sistema con credenciales propias (Priority: P1) 🎯 MVP

**Goal**: un empleado ingresa con credenciales validadas contra la base propia, y ningún mensaje de error permite deducir si una cuenta existe.

**Independent Test**: crear un usuario en la base e intentar ingresar con credenciales correctas, incorrectas e inexistentes.

### Tests for User Story 1 (OBLIGATORIAS — constitución, Principio IV)

> Escribir estas pruebas primero y comprobar que fallan antes de implementar.

- [ ] T027 [P] [US1] Pruebas de las reglas R-U1 a R-U5 del usuario en `tests/Optica.Domain.Tests/Usuarios/UsuarioTests.cs`
- [ ] T028 [P] [US1] Pruebas del handler de ingreso para contraseña incorrecta y usuario desactivado, escenarios 1.2 y 1.4, en `tests/Optica.Application.Tests/Autenticacion/IniciarSesionHandlerTests.cs`
- [ ] T029 [P] [US1] Prueba de ida y vuelta del hash con caracteres no latinos, espacios y los bordes 8 y 12 del rango de FR-003a en `tests/Optica.Integration.Tests/Seguridad/HasheoDeContrasenaTests.cs`, verificando ausencia de corrupción y de truncamiento (FR-003b). **Ubicación corregida**: el hasheador se implementa en `Optica.Infrastructure` (T034) y `Optica.Domain.Tests` solo referencia `Optica.Domain`, así que la ruta anterior exigía una referencia que viola el principio II y hace fallar la compuerta G1. La ubicación idónea sería un proyecto `Optica.Infrastructure.Tests` propio; hasta que exista, vive aquí, que es el único proyecto de prueba que referencia infraestructura
- [ ] T030 [P] [US1] Pruebas de integración de los escenarios 1.1 y 1.6 en `tests/Optica.Integration.Tests/Autenticacion/IngresoTests.cs`, incluida la verificación de la fila en `LoginAttempts`
- [ ] T031 [P] [US1] Prueba de igualación de tiempos del escenario 1.3 en `tests/Optica.Integration.Tests/Autenticacion/EnumeracionDeCuentasTests.cs`, que comprueba SC-004 con diferencia inferior a 100 ms
- [ ] T032 [P] [US1] Prueba del escenario 1.5 en `tests/Optica.Integration.Tests/Configuracion/ProveedoresExternosTests.cs`: arrancar el host con un proveedor externo declarado debe lanzar excepción

### Implementation for User Story 1

- [ ] T033 [P] [US1] Crear las entidades `Empleado` y `Usuario` en `src/1. Core/Optica.Domain/Usuarios/`, con las reglas R-U1 a R-U5 y la baja lógica
- [ ] T034 [P] [US1] Implementar PBKDF2-HMAC-SHA256 con 600.000 iteraciones, salt de 128 bits y clave de 256 bits en `src/2. Infrastructure/Optica.Infrastructure/Seguridad/HasheadorPbkdf2.cs` (decisión D-01)
- [ ] T035 [US1] Añadir al hasheador la derivación señuelo de tiempo constante para usuarios inexistentes (decisión D-02, requisito de SC-004)
- [ ] T036 [US1] Implementar `RepositorioDeUsuarios` en `src/2. Infrastructure/Optica.Infrastructure/Persistencia/RepositorioDeUsuarios.cs`, asíncrono y con token de cancelación propagado
- [ ] T037 [US1] Implementar el comando de ingreso, su validador y su handler en `src/1. Core/Optica.Application/Autenticacion/Comandos/IniciarSesion/`, con el mensaje genérico único de FR-004. El validador aplica **solo el tope de 12 caracteres**, nunca el mínimo de 8: aplicar el mínimo aquí convertiría la longitud en un canal que distingue contraseña corta de credencial incorrecta (FR-003a, decisión D-01a)
- [ ] T038 [US1] Implementar el registro de todo intento en `LoginAttempts`, con `USUARIO_ID` nulo cuando la cuenta no existe (FR-006, FR-022, reglas R-I1 a R-I4)
- [ ] T039 [US1] Exponer `POST /api/auth/login` en `src/3. Presentation/Optica.Web/Endpoints/AutenticacionEndpoints.cs`, según [contracts/auth-endpoints.md](./contracts/auth-endpoints.md), limitado a recibir, despachar y mapear (principio III)
- [ ] T040 [P] [US1] Mapear los tokens de color y tipografía de `docs/PLAN-MAQUETACION.md` al tema de MudBlazor en `src/3. Presentation/Optica.Web/Componentes/TemaOptica.cs`, sin valores literales (principio IX)
- [ ] T041 [US1] Implementar la pantalla de ingreso en `src/3. Presentation/Optica.Web/Componentes/Paginas/Login.razor` con render en servidor, los **cuatro estados** de la tabla del plan, foco inicial en el campo de usuario, recorrido completo por teclado y mensaje de error con ícono además del color (compuerta G9). El campo de contraseña declara el **tope de 12 caracteres** de FR-003a en el propio control, de modo que el formulario no permita escribir más, y el de usuario el de 100. Es conveniencia para quien escribe, **no** control: el validador de servidor de T037 sigue siendo la única frontera de confianza. El mínimo de 8 NO se declara aquí, por el mismo motivo que en T037: revelaría la política

#### Pruebas de navegador de la pantalla de ingreso (decisión D-13)

Cierran los dos huecos que registra [qa/.../trazabilidad.md](../../qa/001-auth-rbac-foundation/trazabilidad.md): SC-001 sin cubrir, y la compuerta G9 apoyada solo en una carta de exploración manual. Los dieciséis casos están especificados en [qa/.../login-e2e-playwright.md](../../qa/001-auth-rbac-foundation/casos-de-prueba/login-e2e-playwright.md). Van después de T041 porque necesitan la pantalla construida.

- [ ] T041a [US1] Crear `tests/Optica.E2E.Tests/` con `Microsoft.Playwright.NUnit`, **excluido del cálculo de cobertura por capa** del principio IV y etiquetado para no correr en la compilación local rápida. **NUnit solo en este proyecto**: los cuatro existentes siguen en xUnit. Es una desviación deliberada del principio IV, registrada en Complexity Tracking del plan y justificada en D-13
- [ ] T041b [US1] Instalar los navegadores de Playwright en la máquina y documentar el paso en [quickstart.md](./quickstart.md), porque sin ellos el proyecto compila pero ninguna prueba corre. Derivar las clases de prueba de la clase base de página de `Microsoft.Playwright.NUnit`, que da contexto de navegador aislado por prueba y cierre determinista
- [ ] T041c [US1] Levantar el host de prueba sobre **HTTPS** y sembrar las cuentas `jperez`, `mlopez`, `bloqueo1` y `sinrol1` de la tabla de datos del documento de casos, reutilizando el andamiaje de T026. Sobre HTTP el navegador descarta las cookies `Secure` y todos los casos de sesión fallan por un motivo ajeno al que se prueba
- [ ] T041d [US1] Garantizar la limpieza de estado entre casos: el contador de intentos fallidos y `BLOQUEADO_HASTA` persisten en la fila del usuario, así que un caso que bloquea `bloqueo1` contamina al siguiente
- [ ] T041e [P] [US1] Implementar CP-E2E-01 y CP-E2E-02: ingreso correcto y medición de **SC-001**, con mediana de 5 corridas por debajo de 3 segundos
- [ ] T041f [P] [US1] Implementar CP-E2E-03 a CP-E2E-05: los **cuatro estados** de la pantalla, incluido que el estado de carga no admite un segundo envío
- [ ] T041g [P] [US1] Implementar CP-E2E-06: las cuatro causas de rechazo se ven idénticas en la pantalla, texto y atributos accesibles incluidos (FR-004)
- [ ] T041h [P] [US1] Implementar CP-E2E-07 a CP-E2E-09 y CP-E2E-15: lector de pantalla, recorrido por teclado con foco visible, 360 píxeles de ancho y contraste mínimo de 4.5 a 1 (compuerta G9)
- [ ] T041i [P] [US1] Implementar CP-E2E-10: **leer las cookies desde JavaScript y comprobar que no aparecen**. Es la única prueba de que el navegador respeta `HttpOnly`, no solo de que la cabecera lo declara
- [ ] T041j [P] [US1] Implementar CP-E2E-11 a CP-E2E-14 y CP-E2E-16: ausencia de proveedores externos en la pantalla (SC-010), bloqueo provocado con cinco envíos, usuario sin roles, el tope de 12 caracteres en el campo del formulario (FR-003a, hace verificable el control declarado en T041), y contraseña no expuesta en el documento ni en la dirección
- [ ] T041k [US1] Configurar la conservación de captura de pantalla, video y traza en cada fallo, por configuración de la integración de NUnit y no por código en cada prueba. Sin ellos un fallo en integración continua es irreproducible

**Checkpoint**: el ingreso funciona de extremo a extremo y no filtra la existencia de cuentas.

---

## Phase 4: User Story 2 - Sesión protegida con renovación y cierre (Priority: P1)

**Goal**: el empleado trabaja una jornada completa sin reescribir la contraseña, y una credencial robada deja de servir en cuanto se detecta su reutilización.

**Independent Test**: autenticarse, esperar el vencimiento del token, renovar, y comprobar que la credencial anterior queda inservible.

**Nota**: US1 y US2 son ambas P1 y se entregan juntas. El ingreso sin manejo de sesión no es operable.

### Tests for User Story 2 (OBLIGATORIAS)

- [ ] T042 [P] [US2] Pruebas de los escenarios 2.2 y 2.3 con `FakeTimeProvider` en `tests/Optica.Application.Tests/Autenticacion/RenovarSesionHandlerTests.cs`, sin esperas reales, **incluida** una prueba de que una cadena de rotaciones no extiende la sesión más allá de las 8 horas (FR-009a)
- [ ] T043 [P] [US2] Prueba del escenario 2.7 en `tests/Optica.Application.Tests/Autenticacion/ClaimsDelTokenTests.cs`, que falla si aparece cualquier claim no previsto
- [ ] T044 [P] [US2] Prueba del escenario 2.1 en `tests/Optica.Integration.Tests/Autenticacion/CookiesDeSesionTests.cs`, verificando `HttpOnly`, `Secure` y `SameSite=Strict` en ambas cookies
- [ ] T045 [P] [US2] Prueba del escenario 2.4 en `tests/Optica.Integration.Tests/Autenticacion/ReutilizacionDeCredencialTests.cs`: reutilizar una credencial rotada revoca toda la cadena del usuario
- [ ] T046 [P] [US2] Prueba del escenario 2.5 en `tests/Optica.Integration.Tests/Autenticacion/CierreDeSesionTests.cs`
- [ ] T047 [P] [US2] Prueba del escenario 2.6 en `tests/Optica.Integration.Tests/Persistencia/SinSecretosEnClaroTests.cs`, con consulta directa a `RefreshTokens` y `Usuarios` (SC-007)
- [ ] T048 [P] [US2] Prueba del caso borde de dos dispositivos usando la misma credencial en `tests/Optica.Integration.Tests/Autenticacion/RotacionConcurrenteTests.cs`: solo una rotación prospera

### Implementation for User Story 2

- [ ] T049 [P] [US2] Crear la entidad `CredencialDeRenovacion` en `src/1. Core/Optica.Domain/Autenticacion/`, con las reglas R-C1 a R-C5
- [ ] T050 [US2] Implementar el emisor y el validador del token en `src/2. Infrastructure/Optica.Infrastructure/Seguridad/EmisorDeTokenJwt.cs`, con vencimiento de 15 minutos y solo los claims de FR-015
- [ ] T051 [US2] Implementar `RepositorioDeCredenciales` con la rotación atómica condicionada a `WHERE REVOKED_AT IS NULL` y comprobación de filas afectadas (decisión D-05, regla R-C1). La fila nueva DEBE **heredar** el `EXPIRES_AT` de la consumida, nunca recalcularlo (FR-009a, regla R-C1a)
- [ ] T052 [US2] Implementar la detección de reutilización que revoca todas las credenciales activas del usuario cuando el hash presentado tiene `REVOKED_AT` poblado (FR-013, regla R-C3), auditada como `REVOCACION_CADENA`
- [ ] T053 [US2] Implementar la escritura y lectura de las cookies `optica_at` y `optica_rt` en `src/3. Presentation/Optica.Web/Autenticacion/CookiesDeSesion.cs`, con `HttpOnly`, `Secure` y `SameSite=Strict` (decisión D-04)
- [ ] T054 [US2] Configurar la autenticación por token leyendo el JWT desde la cookie `optica_at`, de modo que el render en servidor y las islas WebAssembly compartan la sesión
- [ ] T055 [P] [US2] Implementar el comando de renovación en `src/1. Core/Optica.Application/Autenticacion/Comandos/RenovarSesion/`, con techo absoluto de 8 horas desde la autenticación y herencia del vencimiento en cada rotación (FR-009, FR-009a)
- [ ] T056 [P] [US2] Implementar el comando de cierre de sesión en `src/1. Core/Optica.Application/Autenticacion/Comandos/CerrarSesion/`, idempotente y que revoca las credenciales activas (FR-014)
- [ ] T057 [P] [US2] Implementar la consulta de sesión actual en `src/1. Core/Optica.Application/Autenticacion/Consultas/ObtenerSesionActual/`, leyendo los roles del token y no de la base (FR-032)
- [ ] T058 [US2] Exponer `POST /api/auth/refresh`, `POST /api/auth/logout` y `GET /api/auth/session` en `AutenticacionEndpoints.cs`, según los contratos
- [ ] T059 [US2] Activar el token antifalsificación en todos los endpoints que modifican estado, consecuencia obligada de autenticar por cookie (decisión D-04)

**Checkpoint**: la sesión se sostiene ocho horas, rota en cada uso y se corta ante reutilización.

---

## Phase 5: User Story 3 - Bloqueo automático ante fuerza bruta (Priority: P2)

**Goal**: cinco intentos fallidos consecutivos bloquean la cuenta quince minutos, y el mensaje no revela ni que está bloqueada ni que existe.

**Independent Test**: emitir intentos fallidos sucesivos, verificar el bloqueo al quinto, el rechazo de la contraseña correcta durante la ventana y el desbloqueo automático al vencer.

### Tests for User Story 3 (OBLIGATORIAS)

- [ ] T060 [P] [US3] Pruebas de los escenarios 3.2 a 3.5 en `tests/Optica.Application.Tests/Autenticacion/BloqueoDeCuentaTests.cs`, con `FakeTimeProvider` para el vencimiento
- [ ] T061 [P] [US3] Prueba del escenario 3.1 en `tests/Optica.Integration.Tests/Autenticacion/AuditoriaDeBloqueoTests.cs`, verificando la fila en `AUDITORIA.LogUsuarios`
- [ ] T062 [P] [US3] Prueba del escenario 3.6 en `tests/Optica.Integration.Tests/Autenticacion/IntentosContraCuentaInexistenteTests.cs`: ninguna cuenta se crea ni se bloquea
- [ ] T063 [P] [US3] Prueba de concurrencia del escenario 3.7 en `tests/Optica.Integration.Tests/Autenticacion/ConteoConcurrenteDeIntentosTests.cs`: N intentos fallidos simultáneos dejan el contador exactamente en N y el bloqueo ocurre en el quinto (FR-023, exigido por el principio IV)

### Implementation for User Story 3

- [ ] T064 [P] [US3] Implementar la política de bloqueo en `src/1. Core/Optica.Domain/Autenticacion/PoliticaDeBloqueo.cs`: cinco fallos consecutivos, ventana y bloqueo de quince minutos, vencimiento evaluado por comparación de hora y no por proceso programado (FR-017, FR-018)
- [ ] T065 [US3] Implementar el incremento del contador y la decisión del bloqueo en **una sola sentencia `UPDATE` con `OUTPUT`** en `RepositorioDeUsuarios`, nunca leyendo y escribiendo por separado (decisión D-06, FR-023)
- [ ] T066 [US3] Registrar el bloqueo en auditoría con acción `BLOQUEO_CUENTA` dentro de la misma transacción (FR-021)
- [ ] T067 [US3] Integrar el bloqueo en el flujo de ingreso conservando el mensaje genérico idéntico, y reiniciar el contador tras un ingreso correcto (FR-019, FR-020, regla R-U4)

**Checkpoint**: la fuerza bruta queda contenida y el conteo es correcto bajo concurrencia.

---

## Phase 6: User Story 4 - Roles estrictos validados en el servidor (Priority: P2)

**Goal**: cada operación verifica el rol en el servidor; ocultar un control en la interfaz no autoriza nada.

**Independent Test**: asignar cada rol a un usuario distinto e invocar las operaciones protegidas directamente contra el servidor, sin pasar por la interfaz.

### Tests for User Story 4 (OBLIGATORIAS)

- [ ] T068 [P] [US4] Pruebas de los escenarios 4.7 y 4.8 en `tests/Optica.Application.Tests/Roles/ReemplazarRolesValidadorTests.cs`: dos roles autorizan, y un código fuera del conjunto cerrado se rechaza antes de tocar la base
- [ ] T069 [P] [US4] Pruebas de los escenarios 4.1 a 4.4 en `tests/Optica.Integration.Tests/Roles/MatrizDeAutorizacionTests.cs`, invocando siempre contra el servidor sin pasar por la interfaz (SC-005, compuerta G6)
- [ ] T070 [P] [US4] Pruebas de los escenarios 4.5 y 4.6 en `tests/Optica.Integration.Tests/Roles/CambioDeRolesTests.cs`, **incluida** una prueba de rollback que verifica que la auditoría se revierte junto con la operación (compuerta G7)
- [ ] T071 [P] [US4] Prueba del caso borde 4.9 en `tests/Optica.Integration.Tests/Roles/UltimoAdministradorTests.cs`: el último administrador no puede quitarse su propio rol
- [ ] T072 [P] [US4] Prueba del caso borde 4.10 en `tests/Optica.Integration.Tests/Roles/UsuarioSinRolesTests.cs`: ingresa bien, pero recibe 403 en toda operación protegida
- [ ] T073 [P] [US4] Prueba de FR-032 y FR-032a en `tests/Optica.Integration.Tests/Roles/VentanaDeRevocacionTests.cs`: el token vigente sigue sirviendo hasta vencer, y las credenciales de renovación ya no

### Implementation for User Story 4

- [ ] T074 [P] [US4] Crear las entidades `Rol` y `UsuarioRol` en `src/1. Core/Optica.Domain/Usuarios/`, con el conjunto cerrado `Administrador`, `Vendedor` y `Optometra`. Los roles se asignan a la **cuenta de usuario**, no al perfil de empleado (FR-029). **El código va sin tilde**; la tilde solo aparece en el nombre visible
- [ ] T075 [US4] Incluir los roles como claims en el token emitido, usando el código y no el nombre (FR-025)
- [ ] T076 [US4] Definir las políticas `SoloAdministrador` y `Autenticado` en `src/3. Presentation/Optica.Web/Autenticacion/PoliticasDeAutorizacion.cs`, autorizando si **cualquiera** de los roles del usuario satisface la política (FR-025, FR-031)
- [ ] T077 [P] [US4] Implementar la consulta de roles en `src/1. Core/Optica.Application/Roles/Consultas/ListarRoles/` y exponer `GET /api/roles`. **No existe** `POST /api/roles` y no debe existir (FR-024)
- [ ] T078 [US4] Implementar el comando de reemplazo del conjunto de roles en `src/1. Core/Optica.Application/Roles/Comandos/ReemplazarRoles/`, que en una sola transacción calcula el delta, verifica la regla del último administrador **sobre el estado final**, aplica los cambios, audita con antes y después, y revoca las credenciales activas del usuario afectado (FR-027 a FR-030, FR-032a)
- [ ] T079 [US4] Exponer `PUT /api/users/{id}/roles` con la política `SoloAdministrador`, según [contracts/roles-endpoints.md](./contracts/roles-endpoints.md)

**Checkpoint**: la autorización es efectiva en el servidor y el sistema no puede quedarse sin administradores.

---

## Phase 7: User Story 5 - Base técnica verificable (Priority: P3)

**Goal**: la solución compila limpia, el esquema se aplica desde cero, la cobertura se mide y falla bajo el umbral, y existe un primer administrador sin credenciales en el código.

**Independent Test**: clonar el repositorio en una máquina limpia, compilar, aplicar el esquema y ejecutar la suite con reporte de cobertura.

**Nota de alcance**: los escenarios 5.1 y 5.2 los cubren las tareas T002, T013 y T014 de las fases
1 y 2, porque son prerequisitos de todo lo demás. Aquí quedan las tareas que cierran la historia.

### Tests for User Story 5 (OBLIGATORIAS)

- [ ] T080 [P] [US5] Prueba del escenario 5.3 en `tests/Optica.Integration.Tests/Esquema/AplicacionDelEsquemaTests.cs`: aplicar `000`, `001` y `002` sobre una base vacía y verificar la existencia de las tablas de `ADMINISTRACION_USUARIOS` y `AUDITORIA`
- [ ] T081 [P] [US5] Prueba del escenario 5.5 en `tests/Optica.Integration.Tests/Herramientas/CrearPrimerAdminTests.cs`, que ejercita el comando de creación
- [ ] T082 [P] [US5] Prueba que verifica que `Optica.Shared` no contiene espacios de nombres exclusivos de servidor en `tests/Optica.Architecture.Tests/SharedCompatibleWasmTests.cs` (decisión D-10)

### Implementation for User Story 5

- [ ] T083 [US5] Implementar el comando `crear-admin` en `src/3. Presentation/Optica.Web/Herramientas/CrearPrimerAdministrador.cs`, que crea el empleado y su usuario con rol Administrador, pide la contraseña por **entrada interactiva** y no por argumento, reutiliza el hasheador de T034 y audita la acción como `CREAR_PRIMER_ADMIN` (FR-035, decisión D-12). **Valida el rango completo de 8 a 12 caracteres** antes de derivar el hash, rechazando con un mensaje que sí nombra la política, porque aquí no aplica el mensaje genérico de FR-004: quien crea el primer administrador es un operador con acceso a la máquina, no un solicitante anónimo (FR-003a)
- [ ] T084 [US5] Verificar que la prueba de arquitectura de T013 cubre las seis filas de la tabla del principio II y añadir los casos que falten (escenario 5.2, compuerta G1)
- [ ] T085 [US5] Ejecutar el reporte de cobertura y comprobar que el proceso falla por debajo de los umbrales, ajustando la configuración de coverlet si no lo hace (escenario 5.4, compuerta G3)

**Checkpoint**: el "Definition of Done" de la constitución es verificable con un comando.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: los casos borde transversales de la especificación y el cierre de las compuertas.

- [ ] T086 [P] Prueba del desfase de reloj en `tests/Optica.Integration.Tests/Tiempo/DesfaseDeRelojTests.cs`: los vencimientos se calculan en la aplicación y no mezclando `SYSUTCDATETIME()` con la hora del proceso (decisión D-07)
- [ ] T087 [P] Pruebas de petición sin credencial, con credencial malformada y con firma inválida en `tests/Optica.Integration.Tests/Autenticacion/TokensInvalidosTests.cs`: los tres casos responden 401 sin detalle del motivo (FR-016)
- [ ] T088 [P] Prueba del caso borde del empleado sin usuario en `tests/Optica.Domain.Tests/Usuarios/EmpleadoSinUsuarioTests.cs`. **Anotar** que "empleado dado de baja" no es representable: `Empleados` no tiene columna de baja lógica, y resolverlo pertenece a la feature RF-USR
- [ ] T089 [P] Prueba que inspecciona la salida de log y falla si aparece una contraseña, un token o una credencial de renovación (compuerta G8)
- [ ] T090 Completar las pruebas unitarias que falten hasta alcanzar 85% global y 90% en dominio y aplicación, medido con reporte y no por estimación
- [ ] T091 Revisar la pantalla de ingreso contra la checklist de la skill `ux-ui-optica`: cuatro estados, recorrido por teclado con foco visible, contraste 4.5:1, y usabilidad a 360 píxeles sin desplazamiento horizontal (compuerta G9)
- [ ] T092 [P] Crear el archivo de presentación de cada proyecto con propósito, preparación del entorno y uso básico (sección Documentación de la constitución)
- [ ] T093 Ejecutar de principio a fin [quickstart.md](./quickstart.md) en una máquina limpia y corregir cualquier paso que no funcione tal como está escrito
- [ ] T094 Verificar las diez compuertas de calidad del plan y registrar el resultado antes de cerrar la feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Fase 1)**: sin dependencias, empieza de inmediato
- **Foundational (Fase 2)**: depende de la Fase 1 y **bloquea todas** las historias
- **US1 y US2 (Fases 3 y 4)**: ambas P1. Dependen de la Fase 2 y se entregan **juntas**
- **US3 (Fase 5)**: depende de US1, porque cuenta intentos sobre un flujo de ingreso que ya existe
- **US4 (Fase 6)**: depende de US2 para los claims del token, y de US2 para la revocación de FR-032a
- **US5 (Fase 7)**: depende de US1 para reutilizar el hasheador en `crear-admin`
- **Polish (Fase 8)**: depende de todas las anteriores

### User Story Dependencies

A diferencia del caso habitual, estas historias **no son mutuamente independientes**: forman una
cadena de seguridad. El orden importa.

```text
Fase 1 Setup
   └── Fase 2 Foundational  (bloquea todo)
         ├── US1 Ingreso (P1) ──┬── US3 Bloqueo (P2)
         │                      └── US5 Base técnica (P3)
         └── US2 Sesión (P1) ───── US4 Roles (P2)
```

### Within Each User Story

- Las pruebas se escriben primero y deben fallar antes de implementar
- Dominio antes de persistencia, persistencia antes de casos de uso, casos de uso antes de endpoints
- Los endpoints solo reciben, despachan y mapean; nunca contienen lógica (principio III)

### Parallel Opportunities

- T003 a T006 y T010 a T012 en la Fase 1
- T015, T016, T021, T024 y T025 en la Fase 2
- Todas las pruebas de una misma historia, porque van en archivos distintos
- T027 a T032 (US1), T042 a T048 (US2), T060 a T063 (US3), T068 a T073 (US4)
- T086 a T089 y T092 en la Fase 8

---

## Parallel Example: User Story 1

```bash
# Las seis pruebas de US1 van en archivos distintos y se pueden escribir a la vez:
Task: "T027 Reglas del usuario en tests/Optica.Domain.Tests/Usuarios/UsuarioTests.cs"
Task: "T028 Handler de ingreso en tests/Optica.Application.Tests/Autenticacion/IniciarSesionHandlerTests.cs"
Task: "T029 Hash con caracteres extremos en tests/Optica.Integration.Tests/Seguridad/HasheoDeContrasenaTests.cs"
Task: "T030 Ingreso de extremo a extremo en tests/Optica.Integration.Tests/Autenticacion/IngresoTests.cs"
Task: "T031 Enumeración de cuentas en tests/Optica.Integration.Tests/Autenticacion/EnumeracionDeCuentasTests.cs"
Task: "T032 Proveedores externos en tests/Optica.Integration.Tests/Configuracion/ProveedoresExternosTests.cs"
```

---

## Implementation Strategy

### MVP: US1 más US2

El MVP de esta feature **no es solo US1**. Las dos historias son P1 y la especificación lo
justifica: un ingreso sin manejo de sesión obligaría a reescribir la contraseña cada quince
minutos, lo que hace inusable el punto de venta. El incremento entregable es ingresar y sostener
la sesión.

1. Fase 1 Setup
2. Fase 2 Foundational — bloquea todo lo demás
3. Fases 3 y 4, US1 y US2
4. **Parar y validar**: ingresar, trabajar ocho horas sin reescribir la contraseña, cerrar sesión
5. Demostrable

### Entrega incremental

1. Setup y Foundational → base verificable
2. US1 y US2 → ingreso y sesión (MVP)
3. US3 → la autenticación deja de estar expuesta a fuerza bruta
4. US4 → segregación de funciones; **desbloquea las 25 features restantes**
5. US5 → el "Definition of Done" pasa a ser verificable con un comando
6. Fase 8 → casos borde transversales y cierre de compuertas

### Riesgo de orden a tener presente

Mientras US4 no esté cerrada, el sistema es operable pero **sin segregación de funciones**: en la
práctica todos los usuarios pueden todo lo que exista hasta ese momento. Es aceptable porque hasta
entonces lo único que existe es la autenticación misma, pero **ninguna feature de módulo debe
empezar antes de que US4 esté cerrada**.

---

## Resumen

| Fase | Tareas | Pruebas | Implementación |
|---|---|---|---|
| 1 Setup | T001–T012 | — | 12 |
| 2 Foundational | T013–T026 | 2 | 12 |
| 3 US1 Ingreso (P1) | T027–T041 | 6 | 9 |
| 4 US2 Sesión (P1) | T042–T059 | 7 | 11 |
| 5 US3 Bloqueo (P2) | T060–T067 | 4 | 4 |
| 6 US4 Roles (P2) | T068–T079 | 6 | 6 |
| 7 US5 Base técnica (P3) | T080–T085 | 3 | 3 |
| 8 Polish | T086–T094 | 4 | 5 |
| **Total** | **94** | **32** | **62** |

Las 32 tareas de prueba cubren los 32 escenarios de aceptación numerados más los 9 casos borde de
la especificación, agrupados por archivo según la matriz de
[contracts/trazabilidad.md](./contracts/trazabilidad.md).

## Notes

- Las tareas marcadas [P] tocan archivos distintos y no dependen entre sí
- Comprobar que cada prueba falla antes de implementar lo que la hace pasar
- Commitear por tarea o por grupo lógico
- Las rutas de carpeta llevan espacios (`src/1. Core/`); citarlas entre comillas en el intérprete de órdenes
- El código del rol de optómetra es `Optometra` **sin tilde**; el nombre visible es `Optómetra`. Confundirlos rompe la autorización sin error de compilación
