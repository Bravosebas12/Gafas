# Fidelidad al Modelo de Datos Checklist: Fundación de Solución y Autenticación con Control de Acceso

**Purpose**: validar la **calidad de los requisitos que tocan persistencia**: si están expresados contra el esquema existente, si lo que el esquema no puede representar está declarado, y si las reglas son verificables. No verifica implementación.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

Referencia normativa: principio X (Modelo de Datos como Fuente de Verdad) de [constitution.md](../../../.specify/memory/constitution.md). Esquema: [001_modelo_datos_optica.sql](../../../Scripts/SQL/001_modelo_datos_optica.sql).

## Consistencia entre requisitos y esquema

- [x] CHK001 ¿Es consistente FR-029 con el esquema? [Conflict, Spec §FR-029] — **RESUELTO 2026-08-20**: se corrigió el requisito para alinearlo con el esquema, sin cambio de esquema. La compuerta G10 sigue en verde.
- [ ] CHK002 ¿Está declarado que el caso borde del "empleado dado de baja" **no es representable** en el esquema, dado que la tabla de empleados no tiene columna de baja lógica? [Gap, Spec §Edge Cases]
- [ ] CHK003 ¿Es consistente el requisito de baja lógica del principio VII con el esquema para todas las entidades que esta feature toca, o solo para el usuario? [Consistency, Constitución §VII]
- [ ] CHK004 ¿Concuerda la longitud máxima del nombre de usuario declarada en los contratos con la que admite el esquema, y está definido qué debe ocurrir al excederla? [Consistency, Spec §FR-001]
- [ ] CHK005 ¿Está declarado que el conjunto cerrado de roles del requisito coincide con los códigos ya sembrados, incluida la diferencia entre el código sin tilde y el nombre visible con tilde? [Clarity, Spec §FR-024]

## Requisitos de identidad y unicidad

- [ ] CHK006 ¿Está especificado si la comparación del nombre de usuario distingue mayúsculas de minúsculas, o el requisito lo delega implícitamente a la colación de la base? [Ambiguity, Spec §FR-001]
- [ ] CHK007 ¿Está definido el comportamiento requerido cuando la unicidad del nombre de usuario se viola por concurrencia, en términos de mensaje hacia el usuario? [Gap, Coverage]
- [ ] CHK008 ¿Está declarado como requisito que un empleado tiene como máximo un usuario, o solo existe como restricción en el esquema? [Traceability, Spec §Key Entities]
- [ ] CHK009 ¿Está definido si un usuario puede existir sin empleado asociado, dado que el esquema lo prohíbe? [Coverage, Spec §Key Entities]

## Requisitos de tiempo

- [ ] CHK010 ¿Está declarado como requisito verificable que toda marca de tiempo se persiste y se compara en UTC sobre una **única** fuente de tiempo? [Clarity, Spec §FR-006]
- [ ] CHK011 ¿Está especificado el comportamiento requerido ante desfase de reloj entre la aplicación y la base de datos, más allá de mencionarlo como caso borde? [Coverage, Spec §Edge Cases]
- [ ] CHK012 ¿Está definido si el vencimiento del bloqueo debe evaluarse por comparación en cada intento o mediante un proceso que limpie el estado? Son dos diseños con consecuencias distintas. [Ambiguity, Spec §FR-018]

## Cobertura de reglas que dependen de la base

- [ ] CHK013 ¿Está el requisito de conteo correcto bajo concurrencia expresado de forma verificable, con un criterio de éxito medible? [Measurability, Spec §FR-023]
- [ ] CHK014 ¿Está especificado el alcance exacto de la revocación en cascada: todas las credenciales del usuario, o solo las de la cadena afectada? [Ambiguity, Spec §FR-013]
- [ ] CHK015 ¿Está definido cómo se evalúa la regla del último Administrador: sobre usuarios activos, sobre asignaciones de rol, o sobre ambos? [Ambiguity, Spec §FR-030]
- [ ] CHK016 ¿Está declarado el requisito de que la integridad referencial se declare en la base y no solo en la aplicación? [Traceability, Constitución §X]
- [ ] CHK017 ¿Está especificado el requisito de que las operaciones que afectan varias tablas se ejecuten en una transacción explícita, para los casos de esta feature? [Coverage, Constitución §Rendimiento]

## Evolución y deuda declarada

- [ ] CHK018 ¿Está declarado que un cambio futuro de algoritmo de hash requerirá proponer un cambio de esquema, dado que hoy no hay columna de versión de algoritmo? [Assumption, Gap]
- [ ] CHK019 ¿Está fijada la versión del script de esquema contra la que esta feature se escribió, de modo que un cambio posterior del script no invalide el diseño en silencio? [Ambiguity, Spec §FR-034]
- [ ] CHK020 ¿Existe un requisito sobre el crecimiento de las tablas de intentos y de credenciales de renovación, o queda como deuda sin dueño? [Gap]
- [ ] CHK021 ¿Está declarado el requisito de que las columnas de usuario y fecha de creación y actualización se pueblen en todas las tablas que esta feature escribe? [Coverage, Constitución §VII]

## Trazabilidad

- [ ] CHK022 ¿Está documentado, requisito por requisito, si necesita o no un cambio de esquema, de modo que la compuerta G10 sea auditable y no una afirmación? [Traceability, Constitución §X]
- [ ] CHK023 ¿Están las siete entidades declaradas en la especificación mapeadas de forma explícita a tablas existentes, sin ninguna entidad huérfana? [Completeness, Spec §Key Entities]
- [ ] CHK024 ¿Se declara de forma explícita que esta feature no crea ni modifica roles, sino que solo los lee y los asigna? [Clarity, Spec §FR-024]

## Notes

- CHK001 es el ítem más importante del checklist: es una contradicción directa entre un requisito funcional y el esquema declarado como fuente de verdad. Debe resolverse corrigiendo el requisito o proponiendo el cambio de esquema, no dejándolo a interpretación de quien implemente.
- CHK019 tiene un riesgo concreto en este repositorio: el script de esquema fue modificado durante la elaboración de este plan. Sin una versión fijada, la fidelidad al modelo no es verificable.
