# Specification Quality Checklist: Fundación de Solución y Autenticación con Control de Acceso

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs) — *con excepción justificada, ver Nota 1*
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain — resueltos los 3 (FR-008, FR-009, FR-032) en la iteración 2
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded — RF-CFG-02 excluido por decisión explícita, ver Nota 2
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification — *ver Nota 1*

## Notas

### Nota 1 — Detalle técnico en la User Story 5 (excepción aceptada)

La User Story 5 y los requisitos FR-033 a FR-037 nombran capas de la solución, la ruta
`Scripts/SQL/001_modelo_datos_optica.sql` y umbrales de cobertura. Formalmente esto viola
"sin detalles de implementación".

Se acepta como excepción justificada, no como defecto, porque:

- El principio IV de la constitución (`.specify/memory/constitution.md`) declara los umbrales
  de cobertura ≥85% global y ≥90% en dominio y aplicación como **NO NEGOCIABLES**, y la
  compuerta de calidad G3 los exige con reporte. Omitirlos del spec los volvería no verificables.
- El principio X establece el modelo de datos existente como **fuente de verdad**, por lo que
  referenciarlo es una restricción de negocio heredada, no una decisión de diseño tomada aquí.
- La fase habilitadora no tiene historia técnica propia en `HU/Historias_Tecnicas/`, y sin
  especificarla en alguna parte el "Definition of Done" de la constitución no es alcanzable.

**Acción recomendada:** al ejecutar `/speckit-plan`, mover el detalle de estructura de carpetas
y herramientas de cobertura al `plan.md`, dejando en el spec solo el resultado observable
(la solución compila, el esquema se aplica, la cobertura se mide y falla bajo el umbral).

### Nota 2 — Frontera de alcance: RF-CFG-02 (cambio de contraseña) — RESUELTO

`docs/ORDEN-IMPLEMENTACION-HU.md` ubica RF-CFG-02 en la Fase 1, junto a las cuatro historias
RF-LOG. **Decisión del 2026-08-20: se excluye de esta feature** y se reubica en la feature del
módulo RF-CFG / RF-USR, para no arrastrar la pantalla `/configuracion-perfil` a esta entrega.

Consecuencia asumida y registrada en el spec: la Fase 1 del documento de orden cierra con cuatro
de sus cinco historias. Esta feature solo exige el almacenamiento seguro de contraseñas con hash
y salt; la política de complejidad (mayúscula, número, carácter especial) llega con RF-CFG-02.

### Decisiones tomadas en la iteración 2 (2026-08-20)

| Requisito | Decisión | Motivo |
|-----------|----------|--------|
| FR-008 | Token de acceso: 15 minutos | Limita la ventana de abuso ante filtración sin multiplicar las renovaciones |
| FR-009 | Credencial de renovación: 8 horas | Cubre una jornada completa sin reescribir contraseña, requisito de usabilidad del POS |
| FR-032 | Cambios de estado y rol surten efecto al vencer el token, no por verificación en cada petición | Evita una consulta a base de datos por petición; el POS es sensible a la latencia. Ventana máxima aceptada: 15 minutos |
| FR-032a | Revocación inmediata de credenciales de renovación al desactivar o cambiar roles | Impide que la ventana de 15 minutos se extienda mediante una renovación |

### Estado general

- Iteración de validación: 2
- Marcadores de clarificación pendientes: **0**
- Inventario del spec: 5 historias de usuario, 38 requisitos funcionales, 10 criterios de éxito
- Bloqueantes para `/speckit-plan`: **ninguno**
- Único punto abierto, no bloqueante: la excepción de la Nota 1, a resolver moviendo el detalle
  técnico al `plan.md`
