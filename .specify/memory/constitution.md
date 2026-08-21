<!--
SYNC IMPACT REPORT
==================
Cambio de versión: 1.0.0 → 2.0.0 (MAJOR)

Razón del salto mayor: se redefine el principio de Arquitectura Limpia, cuya regla de
dependencias estaba enunciada al revés y por tanto era inaplicable como criterio de
revisión. Se añaden cuatro principios no negociables que todo trabajo existente y futuro
debe satisfacer. Ambos son cambios incompatibles con la interpretación de la v1.0.0.

Principios modificados:
- I. Código Limpio → I. Código Limpio (sin cambio semántico; se añade verificación)
- II. Cobertura de Tests ≥ 85% → IV. Cobertura de Pruebas Verificada (renumerado, con
  umbral por capa explícito y regla de correspondencia con criterios de aceptación)
- III. Arquitectura Limpia → II. Arquitectura Limpia y Regla de Dependencias
  (REDEFINIDO: la tabla de referencias permitidas reemplaza la cadena de flechas errónea)
- IV. Inmutabilidad y Efectos Secundarios → V. Inmutabilidad y Efectos Explícitos
- V. Observabilidad → VIII. Observabilidad (con prohibición explícita de registrar secretos)
- VI. Seguridad → VI. Seguridad y Autenticación Nativa (elevado a no negociable)
- VII. Performance → sección Restricciones Adicionales, con umbrales verificables
- VIII. Documentación → sección Restricciones Adicionales

Secciones añadidas:
- III. CQRS y Frontera de Casos de Uso
- VII. Trazabilidad y Auditoría Inalterable
- IX. Interfaz Accesible y Mobile-First
- X. Modelo de Datos como Fuente de Verdad
- Guía Operativa Normativa (vincula las skills del repositorio)
- Compuertas de Calidad (gates verificables para el Constitution Check del plan)

Secciones removidas: ninguna.

Plantillas y artefactos dependientes:
- ✅ .specify/templates/plan-template.md — sección Constitution Check completada con las
  diez compuertas verificables de esta constitución
- ✅ .specify/templates/tasks-template.md — corregido el enunciado que declaraba las
  pruebas como opcionales, incompatible con el principio IV
- ✅ .specify/templates/spec-template.md — sin cambios necesarios: la constitución no
  añade secciones obligatorias a la especificación
- ✅ .agents/skills/dotnet-clean-architecture/SKILL.md — coherente; es la guía operativa
  del principio II y III
- ✅ .agents/skills/ux-ui-optica/SKILL.md — coherente; es la guía operativa del principio IX
- ⚠ specs/001-auth-rbac-foundation/spec.md — pendiente de revalidar contra el principio X
  al generar su plan

TODO diferidos: ninguno.
-->

# Constitución del Sistema de Gestión para Óptica

Esta constitución gobierna todo el código, la documentación y las decisiones de diseño del
Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica. Cada principio es
verificable: si una regla no puede comprobarse en una revisión de código o en un proceso
automático, no pertenece aquí.

Interpretación de términos: **DEBE** expresa una obligación sin excepciones; **NO DEBE**
una prohibición absoluta; **DEBERÍA** una recomendación fuerte cuya omisión exige
justificación escrita en la revisión.

## Core Principles

### I. Código Limpio

Todo código DEBE poder leerse como prosa por alguien que no lo escribió.

- **Nombres que revelan intención.** NO DEBEN usarse nombres genéricos (`data`, `info`,
  `temp`, `Helper`, `Manager` sin dominio) ni abreviaturas crípticas.
- **Funciones pequeñas.** Máximo 30 líneas por función, y cada una hace una sola cosa.
- **SOLID.** Responsabilidad única, abierto/cerrado, sustitución de Liskov, segregación de
  interfaces e inversión de dependencias.
- **DRY.** La lógica duplicada DEBE extraerse a una función o tipo compartido en cuanto
  aparece por segunda vez.
- **Comentarios que explican el por qué.** El *qué* lo dice el código; un comentario que
  parafrasea la línea siguiente DEBE eliminarse.
- **Excepciones para lo excepcional.** NO DEBEN usarse excepciones como control de flujo.
  Todo mensaje de error DEBE ser descriptivo para quien lo lee en un log.

**Verificación**: revisión de código; ausencia de advertencias del compilador y del
analizador estático.

**Rationale**: este sistema lo mantendrá gente que no participó en su construcción. El
costo de un nombre malo se paga cada vez que alguien lee esa línea.

### II. Arquitectura Limpia y Regla de Dependencias (NO NEGOCIABLE)

Las dependencias apuntan **hacia el dominio**. El dominio no conoce a nadie.

| Proyecto | Puede referenciar |
|---|---|
| `Optica.Domain` | nada del proyecto; solo la biblioteca base de .NET |
| `Optica.Application` | `Domain`, `Shared` |
| `Optica.Infrastructure` | `Application`, `Domain`, `Shared` |
| `Optica.Shared` | nada del proyecto; DEBE compilar para WebAssembly |
| `Optica.API` | `Application`, `Shared`, e `Infrastructure` solo para registrar inyección de dependencias |
| `Optica.Client.Blazor` | `Shared` |

- `Optica.Domain` NO DEBE contener referencias a ORM, atributos de serialización,
  `IConfiguration`, `HttpContext` ni acceso directo al reloj del sistema. El tiempo entra
  por una abstracción y siempre en UTC.
- Cuando una capa interna necesite una capacidad de una externa, DEBE declararse una
  interfaz en la capa interna e implementarse en la externa. No existe otra vía.
- La estructura de carpetas DEBE respetar la definida en
  `HU/Historias_Tecnicas/ESPECIFICACION_TECNICA.md`.

**Verificación**: una prueba de arquitectura automatizada DEBE fallar la compilación ante
cualquier referencia que viole la tabla anterior.

**Rationale**: la v1.0.0 enunciaba las capas como una cadena que situaba la
infraestructura en el centro, lo que hacía imposible aplicar la regla en una revisión.
Una tabla de referencias permitidas es inequívoca y automatizable.

### III. CQRS y Frontera de Casos de Uso

Cada caso de uso DEBE existir como un comando o una consulta con su propio handler.

- Los comandos modifican estado y NO DEBEN devolver datos de lectura más allá del
  identificador de lo creado o afectado. Las consultas NO DEBEN modificar estado.
- Toda regla funcional de negocio DEBE vivir en `Optica.Application` o en
  `Optica.Domain`. NO DEBE haber lógica de negocio en controladores, en componentes de
  interfaz ni en el acceso a datos.
- Cada comando y consulta DEBE tener su validador, y la validación DEBE ejecutarse antes
  del handler.
- Los endpoints DEBEN limitarse a recibir la petición, despacharla y mapear el resultado
  a HTTP.

**Verificación**: revisión de código; ningún controlador ni componente contiene
condicionales de negocio.

**Rationale**: separar comandos de consultas permite optimizar las lecturas del dashboard
y del punto de venta sin arriesgar la consistencia de las escrituras.

### IV. Cobertura de Pruebas Verificada (NO NEGOCIABLE)

- Umbral global: **≥85%** de cobertura de línea en todo el código base.
- Umbral por capa crítica: **≥90%** en `Optica.Domain` y `Optica.Application`.
- DEBEN probarse: reglas de negocio, validaciones, transformaciones de datos, manejo de
  errores y casos borde. DEBE probarse explícitamente la concurrencia donde la
  especificación la mencione.
- Cada escenario de aceptación de una especificación DEBE tener al menos una prueba que le
  corresponda, y las pruebas DEBEN nombrarse por el comportamiento esperado.
- NO se requieren pruebas para constructores triviales, propiedades autoimplementadas,
  archivos de configuración ni código generado.
- Se mockean interfaces, NO tipos concretos, salvo excepción justificada por escrito.
- Marco de pruebas: xUnit para .NET; medición con coverlet.

**Verificación**: compuerta automática en integración continua que falla el proceso por
debajo de cualquiera de los dos umbrales. Las pruebas no son opcionales en ninguna
característica.

**Rationale**: este sistema mueve dinero e inventario. Una regresión silenciosa en pagos
parciales o en el kárdex se descubre cuando el inventario ya no cuadra.

### V. Inmutabilidad y Efectos Explícitos

- Los objetos DEBEN ser inmutables por defecto: campos de solo lectura, y tipos de
  registro para objetos de transferencia y de valor.
- Las funciones DEBEN ser puras cuando no requieran estado mutable.
- Las colecciones DEBEN retornarse como tipos de solo lectura o copias defensivas.
- Un método que modifica estado externo DEBE documentarlo.

**Verificación**: revisión de código; ausencia de campos mutables públicos.

**Rationale**: el estado compartido mutable es la causa más común de defectos difíciles de
reproducir, y el punto de venta mantiene estado en el cliente durante toda la venta.

### VI. Seguridad y Autenticación Nativa (NO NEGOCIABLE)

- La autenticación DEBE validarse exclusivamente contra la base de datos SQL propia. NO
  DEBE integrarse Google, Microsoft, Facebook ni ningún proveedor de identidad externo.
- Las contraseñas DEBEN almacenarse con función de hash segura y salt por usuario. NO
  DEBEN almacenarse en claro ni con cifrado reversible.
- Todo input externo DEBE validarse antes de usarse.
- Toda consulta a base de datos DEBE ser parametrizada. La concatenación de SQL con datos
  de entrada está prohibida.
- NO DEBE haber secretos en el código fuente ni en archivos de configuración versionados.
- Toda regla crítica DEBE validarse en el servidor. Ocultar un control en la interfaz NO
  constituye control de acceso.
- Se aplica el principio de menor privilegio en cada operación.
- Los mensajes de error de autenticación DEBEN ser genéricos y no permitir deducir si una
  cuenta existe.

**Verificación**: revisión de seguridad por característica; pruebas automáticas que
comprueban el rechazo de proveedores externos y la autorización por rol invocada
directamente contra el servidor.

**Rationale**: la especificación de requerimientos lo exige de forma explícita, y una
brecha en un sistema que guarda historia clínica e identificación de clientes tiene
consecuencias legales, no solo técnicas.

### VII. Trazabilidad y Auditoría Inalterable

- Toda operación sensible —ingreso, bloqueo de cuenta, cambio de roles, movimiento de
  inventario, venta, pago, devolución, anulación, nota de crédito y cambio de fórmula
  optométrica— DEBE quedar registrada.
- Los registros de auditoría DEBEN guardar valor anterior y posterior, usuario responsable
  y marca de tiempo en UTC.
- Las tablas de auditoría y de kárdex son de solo inserción. NO DEBEN emitirse
  actualizaciones ni borrados sobre ellas.
- La baja de usuarios y productos DEBE ser lógica, nunca física.
- Toda tabla DEBE mantener sus columnas de usuario y fecha de creación y actualización.

**Verificación**: revisión de código y de esquema; pruebas que confirman la escritura del
registro de auditoría junto a la operación, en la misma transacción.

**Rationale**: sin traza no hay forma de resolver una disputa sobre un pago o un
descuadre de inventario, y la especificación exige logs inalterables.

### VIII. Observabilidad

- Toda operación de negocio DEBE emitir registro estructurado con identificador de
  correlación que permita seguir una petición de extremo a extremo.
- Niveles correctos: `Error` para fallos, `Warning` para situaciones inesperadas
  recuperables, `Information` para flujo normal.
- NO DEBEN registrarse contraseñas, tokens, credenciales de renovación ni datos personales
  sensibles, en ningún nivel.
- Las operaciones críticas DEBEN exponer métricas de latencia, volumen y tasa de error.

**Verificación**: revisión de código; inspección de logs en pruebas de integración.

**Rationale**: un fallo en el punto de venta ocurre con un cliente esperando. Sin logs
correlacionados el diagnóstico llega tarde.

### IX. Interfaz Accesible y Mobile-First

- Toda vista DEBE implementar y mostrar explícitamente sus cuatro estados: cargando,
  vacío, error y con datos.
- Toda pantalla DEBE ser usable a 360 píxeles de ancho sin desplazamiento horizontal.
- Toda funcionalidad DEBE ser alcanzable por teclado, con foco visible y orden de
  tabulación coherente con el orden visual.
- El contraste DEBE cumplir 4.5:1 en texto normal y 3:1 en texto grande.
- El color NO DEBE ser el único portador de información: toda alerta lleva además ícono o
  texto.
- Las acciones destructivas DEBEN pedir confirmación que nombre la consecuencia.
- Los colores y las tipografías DEBEN provenir de los tokens definidos en
  `docs/PLAN-MAQUETACION.md`. NO DEBEN incrustarse valores literales.
- Los componentes DEBEN ser reutilizables, recibir datos por parámetro y comunicar por
  callback, sin consultar al servidor desde la capa de presentación.

**Verificación**: revisión de interfaz contra la checklist de la skill `ux-ui-optica`;
recorrido con teclado y comprobación de contraste por característica.

**Rationale**: el vendedor usa el sistema de pie, con prisa y a veces desde un dispositivo
pequeño. Una interfaz que solo funciona en escritorio con ratón no sirve al negocio.

### X. Modelo de Datos como Fuente de Verdad

- El esquema definido en `Scripts/SQL/001_modelo_datos_optica.sql` es la fuente de verdad
  del modelo de datos.
- NO DEBEN inventarse tablas ni columnas al implementar. Si una característica requiere un
  cambio de esquema, DEBE proponerse de forma explícita, justificarse y actualizarse el
  script antes de escribir el código que lo consume.
- Toda marca de tiempo DEBE persistirse y compararse en UTC, sobre una única fuente de
  tiempo.
- La integridad referencial DEBE declararse en la base de datos, no solo en la aplicación.

**Verificación**: revisión de la fase de diseño del plan contra el script de esquema.

**Rationale**: el modelo de 40 tablas ya está diseñado y validado. Que cada
característica lo extienda por su cuenta produce esquemas divergentes e irreconciliables.

## Restricciones Adicionales

### Rendimiento y Persistencia

- Toda consulta de filtrado o unión DEBE apoyarse en índices apropiados.
- La paginación es obligatoria en toda colección que pueda superar 100 elementos, y el
  límite del servidor y de la interfaz DEBEN coincidir.
- Toda operación de E/S DEBE ser asíncrona, con token de cancelación propagado hasta el
  repositorio.
- Las operaciones que afectan varias tablas —venta con movimiento de kárdex, devolución,
  cambio, anulación, nota de crédito— DEBEN ejecutarse en una transacción explícita.
- Las consultas del dashboard y del punto de venta DEBEN proyectar solo las columnas
  necesarias.
- Las relaciones costosas DEBEN cargarse de forma diferida salvo que el caso de uso las
  requiera siempre.

### Documentación

- Las APIs públicas DEBEN llevar documentación de sus miembros.
- Cada proyecto DEBE tener un archivo de presentación con propósito, preparación del
  entorno y uso básico.
- Las decisiones de arquitectura significativas DEBEN registrarse como registro de
  decisión, con contexto, alternativas descartadas y consecuencias.

### Guía Operativa Normativa

Las siguientes skills del repositorio son la aplicación operativa de esta constitución y
DEBEN consultarse antes de escribir código o interfaz. Ante una discrepancia, prevalece
la constitución y la skill DEBE corregirse.

- `.agents/skills/dotnet-clean-architecture/SKILL.md` — aplica los principios I, II, III,
  IV, V, VI, VII, VIII y X al código.
- `.agents/skills/ux-ui-optica/SKILL.md` — aplica el principio IX a la interfaz y la
  maquetación.
- `.kilo/skills/sql-server-expert/SKILL.md` y
  `.kilo/skills/sql-server-dba-data-model/SKILL.md` — aplican el principio X y la
  restricción de rendimiento y persistencia.

## Development Workflow

### Compuertas de Calidad

Estas compuertas alimentan el Constitution Check de cada plan de implementación. Una
característica no avanza a la fase siguiente con una compuerta en falso.

| # | Compuerta | Cómo se verifica |
|---|---|---|
| G1 | Sin violaciones de la regla de dependencias | Prueba de arquitectura automatizada |
| G2 | Casos de uso como comandos o consultas, sin lógica en controladores | Revisión de código |
| G3 | Cobertura ≥85% global y ≥90% en dominio y aplicación | Reporte de coverlet en integración continua |
| G4 | Cada criterio de aceptación tiene prueba correspondiente | Trazabilidad especificación ↔ prueba |
| G5 | Sin proveedores de identidad externos ni secretos en el código | Prueba automática y revisión |
| G6 | Reglas críticas validadas en el servidor | Prueba que invoca la operación sin pasar por la interfaz |
| G7 | Operaciones sensibles auditadas en la misma transacción | Prueba de integración |
| G8 | Log estructurado con correlación y sin datos sensibles | Revisión de logs en pruebas |
| G9 | Cuatro estados de vista, teclado, contraste y tokens | Checklist de la skill de interfaz |
| G10 | Sin cambios de esquema no propuestos | Revisión contra el script de modelo de datos |

### Definition of Done

Una tarea está completa cuando:

1. Compila sin errores ni advertencias, del compilador y del analizador.
2. Pasan todas las pruebas, las existentes y las nuevas.
3. La cobertura cumple los umbrales, verificada con reporte, no por estimación.
4. Las diez compuertas de calidad aplicables están en verde.
5. La revisión de código está aprobada por alguien distinto de quien escribió el código.
6. La documentación afectada está actualizada.

### Code Review Checklist

- [ ] ¿Los nombres revelan intención y las funciones hacen una sola cosa?
- [ ] ¿Hay duplicación que deba extraerse?
- [ ] ¿Alguna referencia viola la tabla de dependencias del principio II?
- [ ] ¿Quedó lógica de negocio en un controlador, un componente o el acceso a datos?
- [ ] ¿La cobertura cumple el umbral y cubre los casos borde y la concurrencia declarada?
- [ ] ¿Cada criterio de aceptación tiene su prueba?
- [ ] ¿Las reglas críticas se validan en el servidor?
- [ ] ¿Las operaciones sensibles quedan auditadas con valor anterior y posterior?
- [ ] ¿El log es estructurado, correlacionado y libre de datos sensibles?
- [ ] ¿Las consultas están parametrizadas y paginadas donde corresponde?
- [ ] ¿La interfaz maneja los cuatro estados, funciona con teclado y usa los tokens?
- [ ] ¿Se respetó el esquema existente sin inventar tablas ni columnas?

## Governance

Esta constitución prevalece sobre cualquier otra práctica, plantilla o skill del
repositorio. Ante una contradicción, gana la constitución y el otro artefacto DEBE
corregirse.

**Enmiendas.** Requieren justificación documentada, aprobación del equipo y plan de
migración si introducen un cambio incompatible. Toda enmienda DEBE actualizar la versión,
el reporte de impacto de sincronización al inicio de este archivo y las plantillas
dependientes en la misma entrega.

**Versionado.** Se aplica versionado semántico: MAYOR ante remoción o redefinición
incompatible de un principio o de una regla de gobierno; MENOR ante un principio o
sección nueva o materialmente ampliada; PARCHE ante aclaraciones y correcciones de
redacción sin cambio semántico.

**Cumplimiento.** Cada plan de implementación DEBE incluir su Constitution Check con las
diez compuertas. Cada revisión de código DEBE verificar el cumplimiento. Toda desviación
DEBE registrarse en la tabla de seguimiento de complejidad del plan, con la alternativa
más simple que se descartó y por qué. Una desviación no registrada es un defecto.

**Version**: 2.0.0 | **Ratified**: 2026-07-21 | **Last Amended**: 2026-08-20
