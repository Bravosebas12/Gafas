# Implementation Plan: Fundación de Solución y Autenticación con Control de Acceso

**Branch**: `001-auth-rbac-foundation` | **Date**: 2026-08-20 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-auth-rbac-foundation/spec.md`

---

## Summary

Esta feature levanta la solución desde cero y entrega el eje de seguridad del que dependen las
otras 25 features: ingreso con credenciales propias validadas contra SQL, sesión con token de
acceso de 15 minutos y credencial de renovación de 8 horas, bloqueo automático tras cinco intentos
fallidos, y autorización por rol verificada en el servidor.

El enfoque técnico se apoya en tres decisiones que condicionan todo lo demás. Primera: el esquema
de base de datos ya existe y manda, de modo que el modelo de código se somete a él y no hay
migraciones ni tablas nuevas (principio X, compuerta G10). Segunda: las credenciales de sesión
viajan en cookies `HttpOnly` en lugar de exponerse a JavaScript, lo que hace que el render en
servidor y las islas WebAssembly de [ADR-001](../../docs/adr/ADR-001-modelo-presentacion-blazor-web-app.md)
compartan la misma sesión sin código adicional y que un XSS no pueda robarla. Tercera: la
auditoría se escribe desde un interceptor de persistencia, en la misma transacción que la
operación auditada, lo que satisface la compuerta G7 aquí y deja el mecanismo listo para las
features siguientes.

Detalle completo de cada decisión y de las alternativas descartadas en [research.md](./research.md).

---

## Technical Context

**Language/Version**: C# 14 sobre .NET 10

**Primary Dependencies**: Blazor Web App con MudBlazor (presentación), MediatR (orquestación de
casos de uso), FluentValidation (validadores de comando y consulta), EF Core 10 sin migraciones
(persistencia), `Microsoft.AspNetCore.Authentication.JwtBearer` (validación del token de acceso),
`TimeProvider` de la biblioteca base (fuente de tiempo)

**Storage**: SQL Server. Esquema existente y **congelado** en
[Scripts/SQL/001_modelo_datos_optica.sql](../../Scripts/SQL/001_modelo_datos_optica.sql); esta
feature consume `ADMINISTRACION_USUARIOS` (`Empleados`, `Roles`, `Usuarios`, `UsuariosRoles`,
`RefreshTokens`, `LoginAttempts`) y `AUDITORIA.LogUsuarios`. Base y login de aplicación creados
por los scripts `000` y `003`; roles sembrados por `002`

**Testing**: xUnit con medición de cobertura por coverlet (exigido por el principio IV),
`NetArchTest.Rules` para la prueba de arquitectura, `FakeTimeProvider` para vencimientos,
`Respawn` para aislar pruebas de integración contra una base de datos real. La suite de navegador de
la pantalla de ingreso usa **Playwright con NUnit** (`Microsoft.Playwright.NUnit`) en un proyecto
propio, fuera del cálculo de cobertura por capa; decisión D-13 y desviación registrada en
Complexity Tracking

**Target Platform**: aplicación web servida desde Windows con SQL Server; navegadores de
escritorio y móviles a partir de 360 píxeles de ancho

**Project Type**: aplicación web con Clean Architecture en cuatro capas y presentación dividida en
render de servidor más islas WebAssembly

**Performance Goals**: ingreso completo por debajo de 3 segundos (SC-001); diferencia de tiempo
entre usuario inexistente y contraseña incorrecta por debajo de 100 ms (SC-004); derivación de
clave entre 200 y 400 ms

**Constraints**: sin proveedores de identidad externos; sin secretos en el repositorio; sin
cambios de esquema; toda entrada y salida asíncrona con token de cancelación propagado hasta el
repositorio; marcas de tiempo en UTC sobre una única fuente; **contraseña de 8 a 12 caracteres**
(FR-003a), rango que se valida completo al establecerla y solo en su tope al ingresar, para no
romper el mensaje genérico único de FR-004

**Scale/Scope**: una sede, tres roles, del orden de 10 usuarios internos concurrentes. El alcance
de interfaz de esta feature es **una sola pantalla**, la de ingreso; el resto llega con sus
módulos

**Unknowns**: ninguno. Los tres marcadores de clarificación de la especificación quedaron
resueltos antes de este plan, y las decisiones técnicas abiertas se cerraron en
[research.md](./research.md) (D-01 a D-13, más D-01a sobre la longitud de la contraseña)

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Marca cada compuerta como PASA, FALLA o N/A. Una compuerta en FALLA bloquea el avance; si es
una desviación deliberada, DEBE registrarse en Complexity Tracking junto con la alternativa
simple que se descartó. Referencia: `.specify/memory/constitution.md` v2.0.0.

| # | Compuerta | Estado | Nota |
|---|---|---|---|
| G1 | Regla de dependencias entre capas respetada (Principio II) | PASA | Seis proyectos con las referencias exactas de la tabla del principio II. Prueba automatizada con `NetArchTest` (D-09), más la prohibición de EF Core, `IConfiguration`, `HttpContext` y `DateTime` en `Optica.Domain` |
| G2 | Casos de uso como comandos/consultas, sin lógica en controladores (III) | PASA | 7 comandos y 2 consultas bajo MediatR. Los endpoints solo reciben, despachan y mapean a HTTP; el contrato de cada uno está en [contracts/](./contracts/) |
| G3 | Plan de cobertura ≥85% global y ≥90% en dominio y aplicación (IV) | PASA | Umbrales declarados en `Directory.Build.props` y verificados por coverlet en el objetivo de pruebas; el proceso falla por debajo. Sin integración continua todavía, se ejecuta con un objetivo local documentado en [quickstart.md](./quickstart.md) |
| G4 | Cada criterio de aceptación de la spec tiene prueba prevista (IV) | PASA | Matriz de trazabilidad de los 32 escenarios de aceptación en [contracts/trazabilidad.md](./contracts/trazabilidad.md) |
| G5 | Sin proveedores de identidad externos ni secretos en código (VI) | PASA | Validador de configuración que falla el arranque ante cualquier proveedor externo declarado (FR-002) y prueba automática que lo verifica. Secreto de firma por gestor de secretos en desarrollo y variable de entorno en despliegue (D-08) |
| G6 | Reglas críticas validadas en el servidor (VI) | PASA | Autorización por política en cada endpoint, con pruebas que invocan la operación directamente contra el servidor sin pasar por la interfaz (SC-005) |
| G7 | Operaciones sensibles auditadas en la misma transacción (VII) | PASA | Interceptor de `SaveChanges` que escribe `AUDITORIA.LogUsuarios` dentro de la transacción de la operación (D-03), con prueba de integración que verifica el rollback conjunto |
| G8 | Log estructurado con correlación y sin datos sensibles (VIII) | PASA | Identificador de correlación por petición propagado a los handlers; filtro que impide registrar contraseñas, tokens y credenciales de renovación, con prueba que inspecciona la salida |
| G9 | Cuatro estados de vista, teclado, contraste y tokens de diseño (IX) | PASA | Aplica a la única pantalla de la feature. Los cuatro estados de la pantalla de ingreso se detallan más abajo; los colores provienen de los tokens de `docs/PLAN-MAQUETACION.md` mapeados al tema de MudBlazor |
| G10 | Sin cambios de esquema no propuestos sobre el modelo existente (X) | PASA | **Cero cambios de esquema.** El único punto que parecía exigir una columna nueva —detectar la reutilización de una credencial rotada— se resuelve con el estado de `REVOKED_AT`; análisis en D-05 |

**Resultado**: 10 de 10 compuertas en PASA. No hay violaciones que justificar, pero sí cuatro
desviaciones deliberadas respecto a documentos previos, registradas en Complexity Tracking.

**Re-evaluación posterior al diseño de Fase 1**: sin cambios. El diseño de entidades de
[data-model.md](./data-model.md) confirmó que ninguna regla de la especificación requiere una
columna que el esquema no tenga, y los contratos de [contracts/](./contracts/) confirmaron que
todos los endpoints se limitan a despachar casos de uso.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-auth-rbac-foundation/
├── spec.md              # Especificación (ya existente)
├── plan.md              # Este archivo
├── research.md          # Fase 0: decisiones técnicas D-01 a D-12
├── data-model.md        # Fase 1: entidades contra el esquema existente
├── quickstart.md        # Fase 1: puesta en marcha en máquina limpia
├── contracts/           # Fase 1: contratos de endpoints y trazabilidad
│   ├── auth-endpoints.md
│   ├── roles-endpoints.md
│   └── trazabilidad.md
├── checklists/
│   └── requirements.md  # Checklist de calidad de la especificación
└── tasks.md             # Fase 2, lo genera /speckit-tasks
```

### Source Code (repository root)

```text
OpticaSolution.sln
Directory.Build.props                      # Analizadores, advertencias como errores, umbrales de cobertura

src/
├── 1. Core/
│   ├── Optica.Domain/
│   │   ├── Usuarios/                      # Empleado, Usuario, Rol, UsuarioRol
│   │   ├── Autenticacion/                 # CredencialDeRenovacion, IntentoDeIngreso, PoliticaDeBloqueo
│   │   ├── Auditoria/                     # RegistroDeAuditoria
│   │   └── Comun/                         # Tipos de valor, resultados, excepciones de dominio
│   └── Optica.Application/
│       ├── Autenticacion/
│       │   ├── Comandos/                  # IniciarSesion, RenovarSesion, CerrarSesion
│       │   └── Consultas/                 # ObtenerSesionActual
│       ├── Roles/
│       │   ├── Comandos/                  # AsignarRol, QuitarRol
│       │   └── Consultas/                 # ListarRoles
│       ├── Abstracciones/                 # Interfaces de repositorio, hash, emisor de token, reloj
│       └── Comportamientos/               # Validación, log con correlación, transacción
├── 2. Infrastructure/
│   ├── Optica.Infrastructure/
│   │   ├── Persistencia/                  # DbContext, configuraciones, repositorios
│   │   ├── Persistencia/Interceptores/    # Auditoría y columnas de creación/actualización
│   │   ├── Seguridad/                     # PBKDF2, emisor y validador de JWT
│   │   └── Configuracion/                 # Validador que rechaza proveedores externos
│   └── Optica.Shared/                     # Contratos de petición y respuesta; compila para WebAssembly
└── 3. Presentation/
    ├── Optica.Web/
    │   ├── Endpoints/                     # /api/auth, /api/roles, /api/users/{id}/roles
    │   ├── Componentes/Paginas/           # Login (render en servidor)
    │   ├── Autenticacion/                 # Lectura del token desde cookie, políticas
    │   └── Herramientas/                  # Comando de creación del primer Administrador
    └── Optica.Web.Client/                 # Vacío en esta feature; existe para fijar la referencia a Shared

tests/
├── Optica.Domain.Tests/                   # Reglas de dominio y casos borde
├── Optica.Application.Tests/              # Handlers y validadores con dobles de prueba
├── Optica.Integration.Tests/              # Contra SQL Server real: transacciones, unicidad, concurrencia
└── Optica.Architecture.Tests/             # Compuerta G1
```

**Structure Decision**: se adopta literalmente la estructura de
[ESPECIFICACION_TECNICA.md](../../HU/Historias_Tecnicas/ESPECIFICACION_TECNICA.md), que el
principio II declara obligatoria, ya actualizada por ADR-001 a `Optica.Web` y
`Optica.Web.Client`. Los cuatro proyectos de prueba se separan por capa porque los umbrales de
cobertura del principio IV son distintos por capa y medirlos exige poder atribuirlos.

`Optica.Web.Client` queda sin componentes en esta feature. Se crea igualmente, y no por
anticipación: es lo que fuerza a compilar `Optica.Shared` para WebAssembly, que es la verificación
que pide el principio II y sobre la que ADR-001 advierte de forma expresa (D-10).

### Pantalla de ingreso — cuatro estados exigidos por el principio IX

| Estado | Comportamiento |
|---|---|
| Cargando | Botón deshabilitado con indicador de progreso mientras se valida; el formulario no acepta un segundo envío |
| Vacío | Estado inicial del formulario, con foco en el campo de usuario y sin mensajes de error |
| Error | Mensaje genérico único —"Usuario o contraseña incorrectos"— con ícono además del color, para no depender solo de él. Idéntico ante usuario inexistente, contraseña errada, cuenta inactiva y cuenta bloqueada (FR-004) |
| Con datos | Redirección a la pantalla principal; no hay estado con datos propiamente dicho en esta pantalla |

Recorrido por teclado: usuario, contraseña, mostrar u ocultar contraseña, ingresar. Foco visible en
los cuatro. Contraste verificado sobre los tokens de `docs/PLAN-MAQUETACION.md`.

---

## Complexity Tracking

> Las diez compuertas están en verde. Las cuatro primeras filas no son violaciones de la
> constitución, sino desviaciones respecto a documentos previos del proyecto. Las dos últimas **sí
> son desviaciones normativas**: una del principio IV, que fija xUnit como marco de pruebas
> mientras la suite de navegador usa NUnit; y otra de los estándares de seguridad externos, por el
> tope de longitud de la contraseña. Se registran aquí porque el apartado de cumplimiento lo exige
> —"una desviación no registrada es un defecto"—, y cada una queda acotada a un alcance concreto.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| El endpoint de ingreso no devuelve el token de acceso en el cuerpo, contra el contrato dibujado en RF-LOG-01 | Devolverlo obliga al cliente a guardarlo donde JavaScript lo alcance; con cookie `HttpOnly` un XSS en cualquier pantalla no puede robar la sesión (D-04) | Seguir la letra de la historia deja la sesión expuesta a XSS, y el principio VI obliga al menor privilegio. La ruta, el método y los códigos de estado sí se respetan |
| Seis proyectos de código y cuatro de prueba para una feature con una sola pantalla | La estructura la fija el principio II como no negociable, y los umbrales de cobertura por capa exigen poder atribuir la medición a cada capa | Una solución de dos proyectos sería más simple hoy y habría que partirla en la feature 2, con las 25 restantes ya escritas encima |
| Se crea `Optica.Web.Client` sin ningún componente | Es el único mecanismo que fuerza la compilación de `Optica.Shared` para WebAssembly, verificación exigida por el principio II (D-10) | Esperar a la feature del POS significa descubrir la violación cuando `Shared` ya acumule dependencias de servidor, y con el POS bajo presión de entrega |
| Detección de reutilización de credencial de renovación sin columna de encadenamiento | El esquema no tiene `REPLACED_BY_ID` y el principio X prohíbe inventar columnas; el estado de `REVOKED_AT` es suficiente para el alcance de FR-013 (D-05) | Proponer un cambio de esquema por una capacidad forense que ninguna regla de la especificación pide |
| **NUnit en `tests/Optica.E2E.Tests/`, contra el "xUnit para .NET" del principio IV** | `Microsoft.Playwright.NUnit` es la integración oficial de Playwright para .NET: aporta el aislamiento de contexto de navegador por prueba, el cierre determinista y la captura de traza, video y pantalla por configuración. El aislamiento por prueba es justo la parte que, mal hecha, produce suites de navegador intermitentes (D-13) | Escribir el andamiaje a mano sobre xUnit mantiene la uniformidad de marco, pero nos obliga a mantener lo que el paquete oficial ya mantiene. La desviación queda acotada a un solo proyecto, que además está fuera del cálculo de cobertura: los cuatro proyectos de dominio, aplicación, integración y arquitectura siguen en xUnit sin excepción |
| **Tope de 12 caracteres en la contraseña, contra el mínimo de 64 que exige NIST SP 800-63B** | Decisión del responsable del proyecto, registrada en D-01a. El riesgo residual queda acotado por dos mecanismos ya presentes: el bloqueo a los cinco intentos fallidos (FR-017) hace impracticable el ataque en línea, y las 600.000 iteraciones de derivación (D-01) encarecen el ataque fuera de línea si la base se filtrara | Un tope de 128, que OWASP ASVS considera aceptable, o de 64, el mínimo que NIST obliga a admitir. Ambos se propusieron y se descartaron. El costo real de la desviación es que **impide las frases de contraseña**, que son la vía más simple para obtener una credencial fuerte y memorable. **Punto de revisión**: si el sistema se expone a internet o se audita contra un marco de cumplimiento, el tope debe subirse; es un único valor en FR-003a, porque ni el esquema ni el algoritmo dependen de la longitud de entrada |

---

## Fases de ejecución

### Fase 0 — Investigación: **completada**

Salida: [research.md](./research.md), con las decisiones D-01 a D-12 y sus alternativas
descartadas. Ningún marcador de clarificación quedó abierto.

### Fase 1 — Diseño y contratos: **completada**

Salidas:

- [data-model.md](./data-model.md) — las siete entidades de la especificación mapeadas a las
  tablas existentes, con reglas de validación, transiciones de estado y la verificación explícita
  de que no se requiere ningún cambio de esquema.
- [contracts/auth-endpoints.md](./contracts/auth-endpoints.md) — ingreso, renovación, cierre de
  sesión y sesión actual.
- [contracts/roles-endpoints.md](./contracts/roles-endpoints.md) — listado de roles y asignación
  y retiro por usuario.
- [contracts/trazabilidad.md](./contracts/trazabilidad.md) — matriz de los 32 escenarios de
  aceptación contra el tipo de prueba que los cubre, que es lo que sostiene la compuerta G4.
- [quickstart.md](./quickstart.md) — puesta en marcha en una máquina limpia, incluido el orden de
  los scripts SQL y la creación del primer Administrador.
- Contexto del agente actualizado en [CLAUDE.md](../../CLAUDE.md).

### Fase 2 — Tareas: **pendiente**

La genera `/speckit-tasks`. Orden de implementación sugerido, derivado de las prioridades de la
especificación y de las dependencias técnicas:

1. Andamiaje de la solución, `Directory.Build.props`, proyectos de prueba y compuerta G1. Habilita
   todo lo demás y hace verificable el "Definition of Done".
2. Dominio de usuarios y autenticación, con sus reglas y casos borde. Es donde vive el 90% de
   cobertura exigido.
3. Persistencia: contexto, configuraciones contra el esquema existente, repositorios e interceptor
   de auditoría.
4. Ingreso (US1) de extremo a extremo, con la pantalla y el señuelo de tiempo constante.
5. Sesión, renovación y cierre (US2), con la rotación y la revocación en cascada.
6. Bloqueo (US3), con la sentencia atómica y la prueba de concurrencia.
7. Autorización por rol y asignación de roles (US4).
8. Comando del primer Administrador y cierre de la base técnica (US5).

Las historias US1 y US2 son ambas P1 y se entregan juntas: el ingreso sin manejo de sesión no es
operable.
