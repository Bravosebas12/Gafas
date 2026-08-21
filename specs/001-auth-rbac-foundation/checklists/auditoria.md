# Auditoría y Observabilidad Checklist: Fundación de Solución y Autenticación con Control de Acceso

**Purpose**: validar la **calidad de los requisitos de trazabilidad, auditoría y observabilidad** antes de implementar. Evalúa si están completos, delimitados y verificables; no verifica implementación.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

Referencia normativa: principios VII (Trazabilidad y Auditoría Inalterable) y VIII (Observabilidad) de [constitution.md](../../../.specify/memory/constitution.md).

## Completitud: qué se registra

- [x] CHK001 ¿Está definida de forma exhaustiva la lista de eventos que esta feature debe auditar, o se deduce evento por evento de los requisitos funcionales? [Completeness, Spec §FR-021, §FR-028] — **RESUELTO 2026-08-21**: se añadió FR-038 con la lista **exhaustiva y cerrada**: bloqueo de cuenta, cambio de roles, cierre de sesión y revocación en cascada. Ya no se deduce requisito por requisito, y eso es lo que hace auditable la compuerta G7. Decisión D-14.
- [x] CHK002 ¿Está especificada la frontera entre lo que va a la traza de intentos y lo que va al registro de auditoría? La especificación no la declara y el diseño la resolvió por su cuenta. [Gap, Spec §FR-006, §FR-021] — **RESUELTO 2026-08-21**: la frontera la fija data-model.md al separar dos entidades. `IntentoDeIngreso` registra **todo** intento, exista o no la cuenta, y es la traza de seguridad; `RegistroDeAuditoría` registra cambios sensibles con valor anterior y posterior, y es la traza de cambios. La especificación no lo enuncia como regla, pero el diseño lo resolvió de forma consistente y documentada.
- [x] CHK003 ¿Existe un requisito que exija auditar la **renovación** de sesión, o solo el ingreso, el bloqueo y el cambio de roles? [Coverage, Spec §FR-037] — **RESUELTO 2026-08-21**: decidido de forma explícita que la renovación **no** va a la tabla de auditoría, solo al registro estructurado de FR-037. El criterio es el valor forense por fila: ocurre unas 30 veces por usuario y jornada y cada fila diría lo mismo, que la sesión siguió viva. Decisión D-14.
- [x] CHK004 ¿Está definido si el cierre de sesión debe quedar registrado, y en qué traza? [Gap, Spec §FR-014] — **RESUELTO 2026-08-21**: el cierre de sesión **sí** se audita, incluido en FR-038. Sin él no se puede reconstruir cuándo terminó una sesión, que es la mitad de la pregunta "quién estaba dentro y hasta cuándo".
- [x] CHK005 ¿Se exige registrar la revocación en cascada disparada por reutilización de credencial, que es un indicio de compromiso? [Coverage, Spec §FR-013] — **RESUELTO 2026-08-21**: la revocación en cascada **sí** se audita, incluida en FR-038. Era el hueco más serio que este recorrido encontró: es el indicio más fuerte de compromiso que el sistema sabe detectar y no quedaba registrado en ningún sitio.
- [x] CHK006 ¿Está especificado qué debe registrarse cuando la acción no tiene un usuario responsable, como la creación del primer Administrador? [Gap, Spec §FR-035] — **RESUELTO 2026-08-21**: definido en T083. La creación del primer Administrador se audita con la acción `CREAR_PRIMER_ADMIN`; el responsable es el operador que ejecuta el comando en la máquina, no un usuario de la aplicación, y por eso la acción se identifica por su nombre.

## Claridad: cómo se registra

- [x] CHK007 ¿Está definido, para un cambio de roles, si el "valor anterior" es el conjunto completo de roles o solo el rol afectado? La diferencia cambia lo que se puede reconstruir después. [Ambiguity, Spec §FR-028] — **RESUELTO 2026-08-21**: es el **conjunto completo**. El contrato eligió reemplazo del conjunto entero en lugar de agregar y quitar sueltos, precisamente para que la regla del último administrador se evalúe sobre el estado final, y la auditoría registra el conjunto antes y el conjunto después.
- [ ] CHK008 ¿Está especificado el formato del valor anterior y posterior con precisión suficiente para que dos implementaciones produzcan trazas comparables? [Clarity, Spec §FR-028]
- [x] CHK009 ¿Es "identificador de correlación" un requisito verificable, con alcance definido —por petición, por sesión, por operación de negocio—? [Clarity, Spec §FR-037] — **RESUELTO 2026-08-21**: el alcance es **por petición**, fijado en T024: identificador de correlación por petición, propagado a los handlers. Verificable por los cinco casos que comprueban que todos los registros de una operación comparten el mismo identificador.
- [ ] CHK010 ¿Está definido si el identificador de correlación debe llegar al cliente para poder citarlo en un reporte de incidente? [Gap, Spec §FR-037]
- [ ] CHK011 ¿Está especificado el nivel de severidad esperado para cada evento, o se deja al criterio de quien implemente? [Gap, Constitución §VIII]

## Medibilidad de la traza

- [x] CHK012 ¿Es SC-008, que exige el 100% de reconstrucción, verificable con un procedimiento definido, o es una afirmación no comprobable? [Measurability, Spec §SC-008] — **RESUELTO 2026-08-21**: operacionalizado por siete escenarios que comprueban la escritura de la fila correspondiente en cada caso: los tres de FR-006 sobre intentos, los dos de FR-021 sobre el bloqueo y los dos de FR-028 sobre el cambio de roles. La pregunta que queda abierta es la de CHK013, qué debe poder responderse con esa traza.
- [ ] CHK013 ¿Está definido qué significa "reconstruible" en términos de las preguntas que la traza debe poder responder? [Clarity, Spec §SC-008]
- [x] CHK014 ¿Puede verificarse objetivamente que un registro de auditoría se escribió en la **misma transacción** que la operación, o el requisito solo se enuncia? [Measurability, Constitución §VII] — **RESUELTO 2026-08-21**: verificable de forma objetiva. El contrato de roles enumera la transacción única de cinco pasos y el escenario "si el cambio de roles falla, no queda auditoría huérfana" comprueba el rollback conjunto. Es la compuerta G7.

## Cobertura: lo que nunca debe registrarse

- [x] CHK015 ¿Está declarado como requisito explícito que los campos de auditoría no deben contener el hash de contraseña, el salt ni el hash de la credencial de renovación? [Coverage, Spec §FR-015] — **RESUELTO 2026-08-21**: declarado de forma explícita en T019, que **excluye por nombre** `PASSWORD_HASH`, `PASSWORD_SALT` y `TOKEN_HASH` del interceptor de auditoría, y no lo deja a criterio de quien implemente.
- [x] CHK016 ¿Existe un requisito sobre datos personales en la traza, dado que el sistema guarda identificación de clientes e historia clínica en features posteriores? [Gap, Constitución §VIII] — **RESUELTO 2026-08-21**: para esta feature no aplica: sus trazas contienen identificadores de usuario, nombres presentados y direcciones de origen, no datos de clientes. El principio VIII prohíbe registrar datos personales sensibles, y la regla concreta para historia clínica pertenece a RF-CLI, que es su dueña.
- [ ] CHK017 ¿Está especificado qué ocurre si el filtro de datos sensibles falla, es decir si se prefiere perder el log o arriesgar la fuga? [Gap, Exception Flow]

## Inmutabilidad y ciclo de vida

- [x] CHK018 ¿Está declarado como requisito de esta feature que las tablas de auditoría y de intentos son de solo inserción, o se hereda del principio VII sin restatement? [Traceability, Constitución §VII] — **RESUELTO 2026-08-21**: declarado, no solo heredado. La regla R-I3 de data-model.md dice que la tabla de intentos es de solo inserción y que nunca se actualiza ni se borra, y existe un escenario que comprueba que el registro de auditoría del bloqueo no puede alterarse ni eliminarse.
- [x] CHK019 ¿Existe algún requisito de retención o purga para la traza de intentos de ingreso, que crece con cada intento fallido incluido el de un atacante? [Gap] — **RESUELTO 2026-08-21**: FR-041 exige el mecanismo de purga y la decisión D-15 fija un año para los intentos de ingreso. Implementado por T096. El valor está en Assumptions marcado como confirmable con el responsable.
- [x] CHK020 ¿Existe un requisito de retención para las credenciales de renovación, que acumulan una fila por rotación y del orden de treinta por usuario y jornada? [Gap] — **RESUELTO 2026-08-21**: FR-041 y la decisión D-15 fijan treinta días para las credenciales de renovación ya revocadas o vencidas, contando con que solo la última de cada cadena tiene valor operativo. Implementado por T096.
- [x] CHK021 ¿Está definido quién puede **leer** el registro de auditoría, dado que esta feature lo escribe pero la consulta pertenece a RF-USR-03? [Gap, Spec §Assumptions] — **RESUELTO 2026-08-21**: delimitado por alcance. Esta feature **escribe** la traza; la consulta y su control de acceso pertenecen a RF-USR-03, tal como Assumptions declara. No es un hueco, es una frontera de feature.
- [x] CHK022 ¿Se especifica el efecto de la baja lógica de un usuario sobre sus registros históricos de auditoría e intentos? [Coverage, Spec §FR-005] — **RESUELTO 2026-08-21**: se conservan íntegros. Las tablas son de solo inserción por la regla R-I3 y el principio VII, y la baja del usuario es lógica, así que la fila del usuario sigue existiendo y sus registros históricos siguen siendo atribuibles.

## Observabilidad operativa

- [x] CHK023 ¿Existen requisitos de métricas de latencia, volumen y tasa de error para el ingreso y la renovación, que el principio VIII exige para operaciones críticas? La especificación no los declara. [Gap, Constitución §VIII] — **RESUELTO 2026-08-21**: se añadió FR-040, que exige métricas de latencia, volumen y tasa de error para ingreso, renovación y cambio de roles, como pide el principio VIII. Se implementan en T095, tarea propia y deliberadamente separada del registro estructurado de T024.
- [ ] CHK024 ¿Está definido el umbral a partir del cual la latencia del ingreso debe considerarse anómala, más allá del límite de 3 segundos de SC-001? [Gap, Spec §SC-001]
- [ ] CHK025 ¿Existen requisitos sobre qué debe ocurrir si el destino del log no está disponible? [Gap, Exception Flow]

## Consistencia

- [x] CHK026 ¿Son consistentes FR-006 y FR-021 en cuanto a la marca de tiempo y la fuente de tiempo usada, evitando que dos trazas del mismo evento difieran? [Consistency, Spec §FR-006, §FR-021] — **RESUELTO 2026-08-21**: consistentes por construcción. La decisión D-07 fija una única fuente de tiempo, `TimeProvider`, y el principio X exige UTC en toda marca. Dos trazas del mismo evento no pueden diferir porque no hay una segunda fuente que consultar.
- [x] CHK027 ¿Está alineado el requisito de auditar el bloqueo con el requisito de no revelar la existencia de la cuenta, de modo que la traza no se convierta en un canal de fuga para quien la lee? [Consistency, Spec §FR-021, §FR-022] — **RESUELTO 2026-08-21**: no hay conflicto, porque los dos requisitos protegen frente a lectores distintos. El mensaje genérico de FR-004 protege frente al **solicitante anónimo**; la traza la lee un administrador autenticado, y quién puede leerla lo controla RF-USR-03.
- [x] CHK028 ¿Concuerda el alcance de auditoría de esta feature con el mecanismo genérico previsto en RF-USR-03, para que el segundo no obligue a reescribir el primero? [Consistency, Spec §Dependencies] — **RESUELTO 2026-08-21**: alineado por diseño, y fue una razón explícita para elegir EF Core en D-03. La auditoría se escribe desde un interceptor de persistencia genérico, no caso por caso, de modo que RF-USR-03 extiende el mecanismo en lugar de reescribirlo.

## Notes

- Los ítems CHK002, CHK007 y CHK019 son los de mayor impacto: el primero deja una decisión de diseño sin respaldo en la especificación, el segundo afecta lo que se puede reconstruir tras un incidente, y el tercero es deuda de crecimiento sin dueño.
- CHK023 es un incumplimiento del principio VIII que la especificación no cubre; conviene resolverlo antes de cerrar la compuerta G8.
