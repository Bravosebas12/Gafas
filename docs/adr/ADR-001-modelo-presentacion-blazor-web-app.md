# ADR-001: Modelo de presentación con Blazor Web App unificada

**Estado:** Aceptado
**Fecha:** 2026-08-20
**Decide:** equipo de desarrollo, a solicitud del responsable del proyecto
**Afecta a:** `HU/Historias_Tecnicas/ESPECIFICACION_TECNICA.md`, `docs/RESUMEN_REQUERIMIENTOS.md`, `.specify/memory/constitution.md`, `specs/001-auth-rbac-foundation/spec.md`, las 26 historias técnicas y las skills del repositorio

## Contexto

El documento fuente `Especificacion_Requerimientos_Optica_Blazor_v1.1.pdf` fija el ecosistema
tecnológico como *".NET 10 & Blazor WebAssembly"* y dedica su sección 3.3 a la implementación
del frontend aprovechando el modelo de ejecución del lado del cliente. La estructura de
solución que propone separa `Optica.API` de `Optica.Client.Blazor`: un proyecto de API REST y
un cliente WebAssembly que la consume.

Esa separación tiene un costo que se paga en cada característica. Obliga a duplicar contratos
entre servidor y cliente, a mantener dos configuraciones de arranque y dos ciclos de
despliegue, y penaliza la primera carga con la descarga completa del runtime de .NET al
navegador, incluso en pantallas que solo muestran una tabla. Para una óptica de una sede, con
tres roles y sin necesidad de exponer la API a terceros, la separación no compra nada que el
negocio aproveche.

.NET 10 ofrece el modelo de Blazor Web App, que permite declarar el modo de render por
componente: render estático en servidor donde no hace falta interactividad, e interactividad
WebAssembly donde sí. Esto no existía como opción madura cuando se redactó el PDF v1.1.

## Decisión

La aplicación se implementa como **Blazor Web App unificada sobre .NET 10**, con modos de
render mixtos:

- **Render estático en servidor** para autenticación, listados, consultas, historial y
  administración. Primera carga rápida, sin descargar el runtime al navegador.
- **Render interactivo WebAssembly** para las pantallas que exigen interacción sin recargas:
  el punto de venta, los formularios de fórmula optométrica, los filtros en vivo y los
  gráficos del dashboard.

La capa de presentación pasa de dos proyectos a dos proyectos con roles distintos:

| Antes (PDF v1.1) | Ahora |
|---|---|
| `Optica.API` — API REST autónoma | `Optica.Web` — Blazor Web App: componentes en servidor **y** endpoints HTTP |
| `Optica.Client.Blazor` — cliente WASM completo | `Optica.Web.Client` — solo las islas interactivas WASM |

**Lo que no cambia.** Las islas WebAssembly siguen necesitando endpoints HTTP para operar, así
que las rutas `/api/...` y los contratos de petición y respuesta declarados en las 26 historias
técnicas siguen siendo válidos. Cambia dónde se hospedan, no que existan. Tampoco cambian
Clean Architecture, CQRS con MediatR, el modelo de datos, la autenticación nativa ni MudBlazor.

## Alternativas descartadas

**MVC clásico con controladores y vistas Razor, más componentes Blazor embebidos.** Habría
cumplido la letra de la petición de "aplicación MVC", pero mezcla dos modelos de programación
de interfaz en el mismo producto: Razor con recarga de página para la mayoría de pantallas, y
componentes Blazor para el POS. El equipo mantendría dos formas de escribir una pantalla, dos
sistemas de validación de formulario y dos maneras de manejar estado. El modelo de render
mixto de Blazor Web App entrega el mismo beneficio —servidor donde alcanza, cliente donde hace
falta— con un solo modelo de componentes.

**Mantener Blazor WebAssembly con API separada, como dice el PDF.** Es lo más fiel a la línea
base, pero paga el costo de la separación descrito arriba sin obtener el beneficio que la
justifica: no hay terceros que consuman la API, ni clientes móviles nativos previstos, ni
equipos independientes de frontend y backend.

## Consecuencias

**Favorables.** Un solo proyecto que arrancar, configurar y desplegar. Los contratos se
comparten por referencia de proyecto en `Optica.Shared`, no por duplicación. Primera carga
notablemente más rápida en las pantallas de consulta, que son la mayoría. El punto de venta
conserva la interactividad sin recargas y el estado en memoria, que es lo que el negocio
necesita de WebAssembly.

**Adversas.** Se introduce un concepto que el equipo debe entender bien: el modo de render de
cada componente. Un componente marcado como interactivo que dependa de un servicio solo
disponible en servidor falla en tiempo de ejecución, no de compilación. Por eso el principio
de la constitución sobre `Optica.Shared` compilando para WebAssembly deja de ser una
formalidad y pasa a ser una restricción que muerde.

**Riesgo asumido.** La decisión se desvía del documento fuente aprobado. Si el cliente exige
literalmente la arquitectura del PDF v1.1, revertir implica volver a separar la API y el
cliente. El costo de revertir es moderado mientras las capas de dominio, aplicación e
infraestructura permanezcan intactas, que es precisamente lo que la regla de dependencias de
la constitución garantiza. Ese es el argumento para tomar la decisión ahora, antes de escribir
código de presentación, y no después.

## Cumplimiento

- El modo de render DEBE declararse de forma explícita en cada componente de página. No se
  admite depender del valor por defecto.
- `Optica.Shared` NO DEBE recibir dependencias exclusivas de servidor. Una prueba de
  compilación para WebAssembly protege esta regla.
- Las reglas críticas se siguen validando en el servidor. Que un componente se ejecute en el
  navegador no lo convierte en frontera de confianza.
