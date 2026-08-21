# Fidelidad al Modelo de Datos Checklist: Fundación de Solución y Autenticación con Control de Acceso

**Purpose**: validar la **calidad de los requisitos que tocan persistencia**: si están expresados contra el esquema existente, si lo que el esquema no puede representar está declarado, y si las reglas son verificables. No verifica implementación.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

Referencia normativa: principio X (Modelo de Datos como Fuente de Verdad) de [constitution.md](../../../.specify/memory/constitution.md). Esquema: [001_modelo_datos_optica.sql](../../../Scripts/SQL/001_modelo_datos_optica.sql).

## Consistencia entre requisitos y esquema

- [x] CHK001 ¿Es consistente FR-029 con el esquema? [Conflict, Spec §FR-029] — **RESUELTO 2026-08-20**: se corrigió el requisito para alinearlo con el esquema, sin cambio de esquema. La compuerta G10 sigue en verde.
- [x] CHK002 ¿Está declarado que el caso borde del "empleado dado de baja" **no es representable** en el esquema, dado que la tabla de empleados no tiene columna de baja lógica? [Gap, Spec §Edge Cases] — **RESUELTO 2026-08-21**: declarado en `contracts/trazabilidad.md`, en la fila del caso borde: `Empleados` no tiene columna de baja lógica, así que "empleado dado de baja" no es representable hoy. Se prueba lo que sí existe, el usuario inactivo, y queda anotado como punto para RF-USR.
- [x] CHK003 ¿Es consistente el requisito de baja lógica del principio VII con el esquema para todas las entidades que esta feature toca, o solo para el usuario? [Consistency, Constitución §VII] — **RESUELTO 2026-08-21**: consistente solo para `Usuarios`, que tiene `ACTIVO`. `Empleados` carece de columna de baja, y esa asimetría está documentada en CHK002 y diferida a RF-USR, que es la dueña de la gestión de empleados.
- [x] CHK004 ¿Concuerda la longitud máxima del nombre de usuario declarada en los contratos con la que admite el esquema, y está definido qué debe ocurrir al excederla? [Consistency, Spec §FR-001] — **RESUELTO 2026-08-21**: concuerdan. El contrato declara 1 a 100 y el esquema `NOMBRE_USUARIO nvarchar(100)`. Al exceder, el rechazo ocurre en validación antes de llegar a la base, con 400; verificado por los casos CP-LOG-18 y CP-LOG-19.
- [x] CHK005 ¿Está declarado que el conjunto cerrado de roles del requisito coincide con los códigos ya sembrados, incluida la diferencia entre el código sin tilde y el nombre visible con tilde? [Clarity, Spec §FR-024] — **RESUELTO 2026-08-21**: declarado en `contracts/roles-endpoints.md`, que advierte de forma explícita que el código es `Optometra` sin tilde y el nombre visible `Optómetra` con tilde, y que las políticas usan el **código**. Coincide con lo sembrado por `002_seed_catalogos_merge.sql`.

## Requisitos de identidad y unicidad

- [x] CHK006 ¿Está especificado si la comparación del nombre de usuario distingue mayúsculas de minúsculas, o el requisito lo delega implícitamente a la colación de la base? [Ambiguity, Spec §FR-001] — **RESUELTO 2026-08-21**: data-model.md lo declara: la comparación no distingue mayúsculas, según la colación de la base. La delegación es explícita, no implícita.
- [x] CHK007 ¿Está definido el comportamiento requerido cuando la unicidad del nombre de usuario se viola por concurrencia, en términos de mensaje hacia el usuario? [Gap, Coverage] — **RESUELTO 2026-08-21**: fuera de alcance funcional. Esta feature no crea usuarios por interfaz —eso es RF-USR—; el único punto de creación es el primer Administrador. El caso de base técnica verifica que ante dos creaciones simultáneas exactamente una prospera.
- [x] CHK008 ¿Está declarado como requisito que un empleado tiene como máximo un usuario, o solo existe como restricción en el esquema? [Traceability, Spec §Key Entities] — **RESUELTO 2026-08-21**: declarado como requisito en Key Entities: "Un empleado tiene como máximo un usuario". No vive solo en la restricción `UQ_Usuarios_Empleado`.
- [x] CHK009 ¿Está definido si un usuario puede existir sin empleado asociado, dado que el esquema lo prohíbe? [Coverage, Spec §Key Entities] — **RESUELTO 2026-08-21**: definido en los dos sentidos. `EMPLEADO_ID` es obligatorio y no existe usuario sin empleado; y FR-029 declara que un empleado sin cuenta de usuario no tiene roles ni acceso.

## Requisitos de tiempo

- [x] CHK010 ¿Está declarado como requisito verificable que toda marca de tiempo se persiste y se compara en UTC sobre una **única** fuente de tiempo? [Clarity, Spec §FR-006] — **RESUELTO 2026-08-21**: declarado y verificable. FR-006 exige UTC, el principio X exige una única fuente de tiempo, la decisión D-07 la fija en `TimeProvider`, y T021 prohíbe `DateTime.Now` y `DateTime.UtcNow` mediante regla del analizador.
- [x] CHK011 ¿Está especificado el comportamiento requerido ante desfase de reloj entre la aplicación y la base de datos, más allá de mencionarlo como caso borde? [Coverage, Spec §Edge Cases] — **RESUELTO 2026-08-21**: especificado en `contracts/trazabilidad.md`: los vencimientos se calculan en la aplicación, sin mezclar `SYSUTCDATETIME()` con la hora del proceso (D-07). La carta de exploración E-03 lo estresa además de forma manual.
- [x] CHK012 ¿Está definido si el vencimiento del bloqueo debe evaluarse por comparación en cada intento o mediante un proceso que limpie el estado? Son dos diseños con consecuencias distintas. [Ambiguity, Spec §FR-018] — **RESUELTO 2026-08-21**: data-model.md elige el diseño de forma explícita. El vencimiento es **implícito**: se evalúa comparando contra la hora actual en cada intento, sin proceso que limpie el estado, lo que hace inherente el desbloqueo automático de FR-018.

## Cobertura de reglas que dependen de la base

- [x] CHK013 ¿Está el requisito de conteo correcto bajo concurrencia expresado de forma verificable, con un criterio de éxito medible? [Measurability, Spec §FR-023] — **RESUELTO 2026-08-21**: expresado con criterio medible en tres casos. Cinco intentos simultáneos dejan el contador exactamente en 5 y la cuenta bloqueada; cuatro lo dejan exactamente en 4 sin bloqueo; y el caso mixto admite 0 o 1 pero nunca cuenta bloqueada.
- [x] CHK014 ¿Está especificado el alcance exacto de la revocación en cascada: todas las credenciales del usuario, o solo las de la cadena afectada? [Ambiguity, Spec §FR-013] — **RESUELTO 2026-08-21**: sin ambigüedad. FR-013 dice **todas** las credenciales activas del usuario, no solo la cadena afectada, y el contrato de renovación lo repite con la consecuencia práctica de que el usuario legítimo pierde la sesión.
- [x] CHK015 ¿Está definido cómo se evalúa la regla del último Administrador: sobre usuarios activos, sobre asignaciones de rol, o sobre ambos? [Ambiguity, Spec §FR-030] — **RESUELTO 2026-08-21**: definido en la regla R-UR3 y en el contrato: se cuentan los usuarios **activos** con rol Administrador y se evalúa **sobre el estado final** de la operación, en la misma transacción. Eso cubre el caso del administrador que se quita su propio rol.
- [x] CHK016 ¿Está declarado el requisito de que la integridad referencial se declare en la base y no solo en la aplicación? [Traceability, Constitución §X] — **RESUELTO 2026-08-21**: declarado en el principio X de la constitución: "la integridad referencial DEBE declararse en la base de datos, no solo en la aplicación". Verificado por el caso que comprueba la restricción de unicidad del nombre de usuario.
- [x] CHK017 ¿Está especificado el requisito de que las operaciones que afectan varias tablas se ejecuten en una transacción explícita, para los casos de esta feature? [Coverage, Constitución §Rendimiento] — **RESUELTO 2026-08-21**: exigido por la sección de rendimiento de la constitución y por la compuerta G7, y especificado para el caso concreto de esta feature en el contrato de roles, que enumera los cinco pasos de la transacción única, auditoría incluida.

## Evolución y deuda declarada

- [x] CHK018 ¿Está declarado que un cambio futuro de algoritmo de hash requerirá proponer un cambio de esquema, dado que hoy no hay columna de versión de algoritmo? [Assumption, Gap] — **RESUELTO 2026-08-21**: declarado en la nota de riesgo de la decisión D-01: no existe columna de versión de algoritmo, y un cambio futuro exigirá proponer el cambio de esquema según el principio X.
- [ ] CHK019 ¿Está fijada la versión del script de esquema contra la que esta feature se escribió, de modo que un cambio posterior del script no invalide el diseño en silencio? [Ambiguity, Spec §FR-034]
- [ ] CHK020 ¿Existe un requisito sobre el crecimiento de las tablas de intentos y de credenciales de renovación, o queda como deuda sin dueño? [Gap]
- [x] CHK021 ¿Está declarado el requisito de que las columnas de usuario y fecha de creación y actualización se pueblen en todas las tablas que esta feature escribe? [Coverage, Constitución §VII] — **RESUELTO 2026-08-21**: cubierto por T020, que puebla `USUARIO_CREACION`, `FECHA_CREACION`, `USUARIO_ACTUALIZACION` y `FECHA_ACTUALIZACION` desde `TimeProvider` y el usuario de la petición, en un interceptor y no caso por caso.

## Trazabilidad

- [x] CHK022 ¿Está documentado, requisito por requisito, si necesita o no un cambio de esquema, de modo que la compuerta G10 sea auditable y no una afirmación? [Traceability, Constitución §X] — **RESUELTO 2026-08-21**: auditable por entidad en lugar de por requisito, y es suficiente: las siete entidades de la especificación están mapeadas una por una a tabla existente en data-model.md, y todo requisito que toca datos lo hace a través de una de ellas. El único punto que parecía exigir columna nueva se analiza en D-05.
- [x] CHK023 ¿Están las siete entidades declaradas en la especificación mapeadas de forma explícita a tablas existentes, sin ninguna entidad huérfana? [Completeness, Spec §Key Entities] — **RESUELTO 2026-08-21**: las siete están mapeadas de forma explícita en data-model.md, sin entidad huérfana y sin tabla inventada.
- [x] CHK024 ¿Se declara de forma explícita que esta feature no crea ni modifica roles, sino que solo los lee y los asigna? [Clarity, Spec §FR-024] — **RESUELTO 2026-08-21**: declarado en `contracts/roles-endpoints.md`: el conjunto es cerrado, ya está sembrado, y "no existe `POST /api/roles` y no debe existir".

## Notes

- CHK001 es el ítem más importante del checklist: es una contradicción directa entre un requisito funcional y el esquema declarado como fuente de verdad. Debe resolverse corrigiendo el requisito o proponiendo el cambio de esquema, no dejándolo a interpretación de quien implemente.
- CHK019 tiene un riesgo concreto en este repositorio: el script de esquema fue modificado durante la elaboración de este plan. Sin una versión fijada, la fidelidad al modelo no es verificable.
