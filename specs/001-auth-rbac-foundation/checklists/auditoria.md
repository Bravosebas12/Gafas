# Auditoría y Observabilidad Checklist: Fundación de Solución y Autenticación con Control de Acceso

**Purpose**: validar la **calidad de los requisitos de trazabilidad, auditoría y observabilidad** antes de implementar. Evalúa si están completos, delimitados y verificables; no verifica implementación.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

Referencia normativa: principios VII (Trazabilidad y Auditoría Inalterable) y VIII (Observabilidad) de [constitution.md](../../../.specify/memory/constitution.md).

## Completitud: qué se registra

- [ ] CHK001 ¿Está definida de forma exhaustiva la lista de eventos que esta feature debe auditar, o se deduce evento por evento de los requisitos funcionales? [Completeness, Spec §FR-021, §FR-028]
- [ ] CHK002 ¿Está especificada la frontera entre lo que va a la traza de intentos y lo que va al registro de auditoría? La especificación no la declara y el diseño la resolvió por su cuenta. [Gap, Spec §FR-006, §FR-021]
- [ ] CHK003 ¿Existe un requisito que exija auditar la **renovación** de sesión, o solo el ingreso, el bloqueo y el cambio de roles? [Coverage, Spec §FR-037]
- [ ] CHK004 ¿Está definido si el cierre de sesión debe quedar registrado, y en qué traza? [Gap, Spec §FR-014]
- [ ] CHK005 ¿Se exige registrar la revocación en cascada disparada por reutilización de credencial, que es un indicio de compromiso? [Coverage, Spec §FR-013]
- [ ] CHK006 ¿Está especificado qué debe registrarse cuando la acción no tiene un usuario responsable, como la creación del primer Administrador? [Gap, Spec §FR-035]

## Claridad: cómo se registra

- [ ] CHK007 ¿Está definido, para un cambio de roles, si el "valor anterior" es el conjunto completo de roles o solo el rol afectado? La diferencia cambia lo que se puede reconstruir después. [Ambiguity, Spec §FR-028]
- [ ] CHK008 ¿Está especificado el formato del valor anterior y posterior con precisión suficiente para que dos implementaciones produzcan trazas comparables? [Clarity, Spec §FR-028]
- [ ] CHK009 ¿Es "identificador de correlación" un requisito verificable, con alcance definido —por petición, por sesión, por operación de negocio—? [Clarity, Spec §FR-037]
- [ ] CHK010 ¿Está definido si el identificador de correlación debe llegar al cliente para poder citarlo en un reporte de incidente? [Gap, Spec §FR-037]
- [ ] CHK011 ¿Está especificado el nivel de severidad esperado para cada evento, o se deja al criterio de quien implemente? [Gap, Constitución §VIII]

## Medibilidad de la traza

- [ ] CHK012 ¿Es SC-008, que exige el 100% de reconstrucción, verificable con un procedimiento definido, o es una afirmación no comprobable? [Measurability, Spec §SC-008]
- [ ] CHK013 ¿Está definido qué significa "reconstruible" en términos de las preguntas que la traza debe poder responder? [Clarity, Spec §SC-008]
- [ ] CHK014 ¿Puede verificarse objetivamente que un registro de auditoría se escribió en la **misma transacción** que la operación, o el requisito solo se enuncia? [Measurability, Constitución §VII]

## Cobertura: lo que nunca debe registrarse

- [ ] CHK015 ¿Está declarado como requisito explícito que los campos de auditoría no deben contener el hash de contraseña, el salt ni el hash de la credencial de renovación? [Coverage, Spec §FR-015]
- [ ] CHK016 ¿Existe un requisito sobre datos personales en la traza, dado que el sistema guarda identificación de clientes e historia clínica en features posteriores? [Gap, Constitución §VIII]
- [ ] CHK017 ¿Está especificado qué ocurre si el filtro de datos sensibles falla, es decir si se prefiere perder el log o arriesgar la fuga? [Gap, Exception Flow]

## Inmutabilidad y ciclo de vida

- [ ] CHK018 ¿Está declarado como requisito de esta feature que las tablas de auditoría y de intentos son de solo inserción, o se hereda del principio VII sin restatement? [Traceability, Constitución §VII]
- [ ] CHK019 ¿Existe algún requisito de retención o purga para la traza de intentos de ingreso, que crece con cada intento fallido incluido el de un atacante? [Gap]
- [ ] CHK020 ¿Existe un requisito de retención para las credenciales de renovación, que acumulan una fila por rotación y del orden de treinta por usuario y jornada? [Gap]
- [ ] CHK021 ¿Está definido quién puede **leer** el registro de auditoría, dado que esta feature lo escribe pero la consulta pertenece a RF-USR-03? [Gap, Spec §Assumptions]
- [ ] CHK022 ¿Se especifica el efecto de la baja lógica de un usuario sobre sus registros históricos de auditoría e intentos? [Coverage, Spec §FR-005]

## Observabilidad operativa

- [ ] CHK023 ¿Existen requisitos de métricas de latencia, volumen y tasa de error para el ingreso y la renovación, que el principio VIII exige para operaciones críticas? La especificación no los declara. [Gap, Constitución §VIII]
- [ ] CHK024 ¿Está definido el umbral a partir del cual la latencia del ingreso debe considerarse anómala, más allá del límite de 3 segundos de SC-001? [Gap, Spec §SC-001]
- [ ] CHK025 ¿Existen requisitos sobre qué debe ocurrir si el destino del log no está disponible? [Gap, Exception Flow]

## Consistencia

- [ ] CHK026 ¿Son consistentes FR-006 y FR-021 en cuanto a la marca de tiempo y la fuente de tiempo usada, evitando que dos trazas del mismo evento difieran? [Consistency, Spec §FR-006, §FR-021]
- [ ] CHK027 ¿Está alineado el requisito de auditar el bloqueo con el requisito de no revelar la existencia de la cuenta, de modo que la traza no se convierta en un canal de fuga para quien la lee? [Consistency, Spec §FR-021, §FR-022]
- [ ] CHK028 ¿Concuerda el alcance de auditoría de esta feature con el mecanismo genérico previsto en RF-USR-03, para que el segundo no obligue a reescribir el primero? [Consistency, Spec §Dependencies]

## Notes

- Los ítems CHK002, CHK007 y CHK019 son los de mayor impacto: el primero deja una decisión de diseño sin respaldo en la especificación, el segundo afecta lo que se puede reconstruir tras un incidente, y el tercero es deuda de crecimiento sin dueño.
- CHK023 es un incumplimiento del principio VIII que la especificación no cubre; conviene resolverlo antes de cerrar la compuerta G8.
