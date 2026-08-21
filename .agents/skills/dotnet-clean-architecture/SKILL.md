---
name: dotnet-clean-architecture
description: Experto en .NET 10 con Clean Architecture, CQRS y MediatR para el sistema de óptica. Úsalo al crear o modificar código de Domain, Application, Infrastructure, API o pruebas xUnit, y al decidir en qué capa vive una responsabilidad.
---

# .NET Clean Architecture Skill

Actúa como arquitecto y desarrollador senior de .NET para el Sistema Integral de Gestión de Inventario y POS para Óptica.

## Contexto obligatorio

Antes de escribir código, ten presente lo siguiente. Si el archivo existe, léelo:

- `HU/Historias_Tecnicas/ESPECIFICACION_TECNICA.md` — estructura de solución y capas autorizadas.
- `Scripts/SQL/001_modelo_datos_optica.sql` — modelo de datos vigente, 40 tablas en 9 esquemas. Es la fuente de verdad: **no inventes tablas ni columnas nuevas**; si una historia parece requerirlas, dilo explícitamente en lugar de improvisar el esquema.
- `specs/<feature>/spec.md` y `plan.md` de la feature en curso.
- `.specify/memory/constitution.md` — constitución del proyecto. Ese archivo está en `.gitignore`, así que sus reglas vigentes se resumen abajo.

## Stack fijado

- .NET 10, C# con nullable reference types habilitados.
- Clean Architecture / Arquitectura Cebolla.
- CQRS: comandos y consultas separados, orquestados con MediatR.
- SQL Server propio. Autenticación nativa: **prohibido** integrar Google, Microsoft, Facebook o cualquier proveedor de identidad externo.
- Blazor WebAssembly con MudBlazor en presentación.
- xUnit para pruebas, coverlet para cobertura.

## Estructura de solución

```text
OpticaSolution/
├── src/
│   ├── 1. Core/
│   │   ├── Optica.Domain/
│   │   └── Optica.Application/
│   ├── 2. Infrastructure/
│   │   ├── Optica.Infrastructure/
│   │   └── Optica.Shared/
│   └── 3. Presentation/
│       ├── Optica.Web/           (Blazor Web App: componentes SSR + endpoints HTTP)
│       └── Optica.Web.Client/    (islas interactivas WebAssembly)
```

## Regla de dependencias

Las referencias apuntan **hacia el dominio**. Nunca al revés.

| Proyecto | Puede referenciar |
|---|---|
| `Optica.Domain` | nada del proyecto; solo BCL |
| `Optica.Application` | `Domain`, `Shared` |
| `Optica.Infrastructure` | `Application`, `Domain`, `Shared` |
| `Optica.Web` | `Application`, `Infrastructure` (solo para registrar DI), `Shared` |
| `Optica.Web.Client` | `Shared` |

Si necesitas que `Domain` o `Application` alcancen algo de infraestructura, declara una interfaz en la capa interna e impleméntala en la externa. Esa es la única vía.

## Qué vive en cada capa

**Optica.Domain** — Entidades, value objects, enumeraciones, reglas de negocio invariantes, excepciones de dominio, contratos de agregado. Sin EF Core, sin atributos de serialización, sin `IConfiguration`, sin `DateTime.Now`. El tiempo entra por una abstracción (`IClock`) y siempre en UTC.

**Optica.Application** — Un comando o consulta por caso de uso, con su handler de MediatR, su validador de FluentValidation y sus DTOs. Aquí viven las interfaces de repositorio y de servicios. Las reglas funcionales de las historias técnicas se implementan aquí, no en controladores.

**Optica.Infrastructure** — Persistencia SQL, repositorios, emisión y validación de tokens, hashing de contraseñas, auditoría, `IClock`, configuración técnica. Ninguna regla de negocio.

**Optica.Shared** — Contratos y DTOs compartidos entre el servidor y las islas WebAssembly, constantes, validaciones comunes. Debe poder compilar para WebAssembly, así que nada de dependencias de servidor. Esta restricción muerde de verdad: si aquí entra algo que solo existe en servidor, el fallo aparece en ejecución dentro del navegador, no al compilar.

**Optica.Web** — Blazor Web App. Cumple dos funciones. Como interfaz: enrutado, layout, componentes de render estático en servidor y configuración del tema de MudBlazor. Como backend: endpoints delgados que reciben la petición, la despachan por MediatR y mapean el resultado a HTTP. Cero lógica de negocio. La autorización se declara aquí y se valida en el servidor siempre, nunca confiando en que la UI oculte una opción.

**Optica.Web.Client** — Solo los componentes que necesitan interactividad en el navegador, ejecutados en WebAssembly: punto de venta, formularios de fórmula optométrica, filtros en vivo y gráficos. Componentes MudBlazor, contenedores de estado en memoria para el flujo POS y manejo explícito de estados de carga y error.

## Modos de render

Ver `docs/adr/ADR-001-modelo-presentacion-blazor-web-app.md`.

- Declara el modo de render **explícitamente** en cada componente de página. No dependas del valor por defecto.
- Render estático en servidor por defecto: autenticación, listados, consultas, historial y administración. Evita descargar el runtime al navegador para mostrar una tabla.
- Render interactivo WebAssembly solo donde la interacción sin recargas es el requisito: POS, fórmula optométrica, filtros en vivo, gráficos del dashboard.
- Un componente interactivo **no puede** inyectar servicios que solo existen en servidor, como un repositorio o un `DbContext`. Consume endpoints HTTP.
- Los endpoints que consumen las islas viven en `Optica.Web`. Las rutas y contratos declarados en las historias técnicas siguen vigentes; lo que cambió es dónde se hospedan.

## Convenciones de código

- Nombres que revelan intención. Prohibido `data`, `info`, `temp`, `Manager`, `Helper` genérico, y abreviaturas crípticas.
- Funciones de 20 a 30 líneas como máximo; cada una hace una sola cosa.
- SOLID y DRY. Si aparece lógica duplicada, extráela.
- Comentarios que explican el *por qué*; el *qué* lo dice el código.
- Inmutabilidad por defecto: `readonly`, `record` para DTOs y value objects, `IReadOnlyCollection` en retornos de colección.
- Excepciones para casos excepcionales, nunca como control de flujo. Mensajes descriptivos.
- `async`/`await` en toda operación de E/S, con `CancellationToken` propagado hasta el repositorio.
- Nomenclatura CQRS: `CrearOrdenVentaCommand` / `CrearOrdenVentaCommandHandler`, `ObtenerKardexQuery` / `ObtenerKardexQueryHandler`.

## Seguridad

- Todo input externo se valida antes de usarse, con FluentValidation en Application.
- Consultas parametrizadas siempre. Nunca concatenes SQL.
- Ningún secreto en el código ni en `appsettings.json` versionado: variables de entorno o gestor de secretos.
- Contraseñas con hash y salt por usuario. Nunca en claro ni con cifrado reversible.
- Principio de menor privilegio en cada operación.
- Mensajes de error que no filtran si una cuenta existe.

## Persistencia y rendimiento

- Índices apropiados en toda consulta de filtrado o join.
- Paginación **obligatoria** en colecciones que puedan superar 100 elementos.
- Transacciones explícitas en operaciones que afecten varias tablas: venta con movimiento de kárdex, devolución, cambio, anulación.
- Consultas de dashboard y POS optimizadas; proyecta solo las columnas necesarias.
- El esquema existente usa `bigint IDENTITY`, `datetime2(7)` en UTC y columnas de auditoría `USUARIO_CREACION`, `FECHA_CREACION`, `USUARIO_ACTUALIZACION`, `FECHA_ACTUALIZACION`. Respétalas al mapear.

## Auditoría y observabilidad

- Logging estructurado en toda operación de negocio, con identificador de correlación.
- Niveles correctos: `Error` para fallos, `Warning` para situaciones inesperadas recuperables, `Information` para flujo normal. Jamás registres contraseñas, tokens ni datos personales sensibles.
- Los esquemas `AUDITORIA` y `NOTIFICACIONES` ya existen. Las tablas de log guardan valor anterior y posterior; los registros de auditoría son inmutables, solo se insertan.

## Pruebas — no negociable

- Cobertura mínima: **85% global** y **90% en Domain y Application**. El proceso falla por debajo.
- Prueba obligatoriamente: lógica de negocio, validaciones, transformaciones, manejo de errores y casos borde.
- No hace falta probar constructores triviales, propiedades autoimplementadas, configuración ni código generado.
- Mockea interfaces, no tipos concretos.
- Nombra las pruebas por el comportamiento esperado: `Login_ConCuentaBloqueada_RechazaAunConPasswordCorrecta`.
- Cada escenario Given/When/Then de la spec debe tener al menos una prueba que le corresponda.
- Prueba explícitamente la concurrencia donde la spec la menciona: conteo de intentos fallidos, rotación de credenciales, movimientos de stock.

## Antes de considerar terminado

1. Compila sin errores ni advertencias.
2. Todas las pruebas pasan y la cobertura cumple el umbral, verificada con reporte.
3. Ninguna regla de negocio quedó en un controlador o en un componente Blazor.
4. Las reglas críticas se validan en el servidor, no solo en la UI.
5. Las operaciones sensibles quedan auditadas y con log estructurado.
6. Documentación XML en las APIs públicas.
