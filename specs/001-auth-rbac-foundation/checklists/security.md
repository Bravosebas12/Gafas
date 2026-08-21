# Security Checklist: Fundación de Solución y Autenticación con Control de Acceso

**Purpose**: validar la **calidad de los requisitos de seguridad** antes de escribir código: si están completos, cuantificados, consistentes y verificables. No verifica implementación.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

**Nota**: cada ítem pregunta si el requisito **está bien escrito**, no si el sistema funciona. Un ítem marcado no significa que algo funcione, sino que el requisito es defendible.

## Completitud de los requisitos de credenciales

- [ ] CHK001 ¿Está cuantificada la fuerza del hash de contraseña con criterios verificables —algoritmo, iteraciones, longitud de salt— o el requisito se queda en "función de hash segura"? [Clarity, Spec §FR-003]
- [x] CHK002 ¿Existe algún requisito de longitud mínima o complejidad para la contraseña del **primer Administrador**? [Gap, Spec §FR-035] — **RESUELTO 2026-08-20**: FR-003a fija un mínimo de 8 caracteres aplicable donde la contraseña se establece, incluida la creación del primer Administrador. La política de **complejidad** sigue perteneciendo a RF-CFG-02.
- [x] CHK003 ¿Está definido el comportamiento ante una contraseña con espacios o caracteres no latinos? [Coverage, Spec §Edge Cases] — **RESUELTO 2026-08-20**: FR-003b lo declara de forma explícita, con escenario de prueba en `qa/.../features/autenticacion-login.feature`.
- [x] CHK004 ¿Se especifica un límite superior de longitud de contraseña, y qué ocurre al excederlo? [Gap] — **RESUELTO 2026-08-20**: máximo de 12 caracteres (FR-003a), con **rechazo** y nunca truncamiento. Decisión D-01a, que registra la desviación consciente de NIST SP 800-63B.
- [ ] CHK005 ¿Está declarado como requisito que la contraseña no debe aparecer en ningún log, traza ni mensaje de error, o solo se deduce del principio VIII de la constitución? [Traceability, Spec §FR-003]

## Completitud de los requisitos de sesión

- [x] CHK006 ¿Está especificado si la rotación de la credencial de renovación **reinicia** el plazo de 8 horas o si preserva el inicio original de la sesión? [Ambiguity, Spec §FR-009, §FR-011] — **RESUELTO 2026-08-20**: se añadió FR-009a. El vencimiento es absoluto desde la autenticación y la rotación lo hereda. Consecuencia aceptada: un turno de más de 8 horas exige autenticarse dos veces.
- [x] CHK007 ¿Existe un requisito de duración máxima absoluta de sesión, independiente de la renovación? [Gap] — **RESUELTO 2026-08-20**: FR-009a lo fija en 8 horas desde la autenticación.
- [ ] CHK008 ¿Está definido cuántas sesiones concurrentes puede tener un mismo usuario, y si un ingreso desde un dispositivo nuevo debe invalidar los anteriores? [Gap, Spec §US2]
- [ ] CHK009 ¿Están especificados en la especificación los requisitos de transporte de las credenciales, o esa decisión vive solo en el plan? Si vive solo en el plan, ¿es una delegación deliberada? [Traceability, Spec §FR-007]
- [ ] CHK010 ¿Se exige de forma explícita que la aplicación se sirva sobre canal cifrado? Ningún requisito lo menciona, y varias decisiones de sesión lo presuponen. [Gap]
- [ ] CHK011 ¿Están definidos requisitos sobre el algoritmo de firma del token y la rotación de su clave? [Gap, Spec §FR-015]

## Completitud de los requisitos de resistencia a ataques

- [x] CHK012 ¿Existe algún requisito de limitación por origen de la petición, además del bloqueo por cuenta? [Gap, Spec §FR-017] — **RESUELTO 2026-08-20**: queda **fuera de alcance** de forma explícita, con el riesgo aceptado por escrito en Assumptions. DEBE revisarse antes de exponer la aplicación a internet.
- [ ] CHK013 ¿Está especificado el comportamiento requerido ante un volumen anómalo de intentos fallidos contra cuentas inexistentes? [Coverage, Spec §FR-022]
- [ ] CHK014 ¿Se define un requisito de detección o notificación ante bloqueos repetidos, o el bloqueo silencioso agota lo exigido? [Gap, Spec §FR-021]
- [ ] CHK015 ¿Está definido si el bloqueo debe poder levantarse de forma administrativa antes de los quince minutos, o si el vencimiento automático es la única vía? [Ambiguity, Spec §FR-018]

## Claridad y medibilidad

- [ ] CHK016 ¿Es "mensaje genérico e idéntico" verificable de forma objetiva, incluyendo el código de estado y el tiempo de respuesta, y no solo el texto? [Measurability, Spec §FR-004]
- [ ] CHK017 ¿Es el umbral de 100 milisegundos de SC-004 medible de forma reproducible, con un método de medición y un número de muestras definidos? [Measurability, Spec §SC-004]
- [ ] CHK018 ¿Está cuantificado "sin revelar el motivo del rechazo" de modo que se pueda distinguir un cumplimiento de un incumplimiento? [Clarity, Spec §FR-016]
- [ ] CHK019 ¿Puede verificarse objetivamente SC-010, que exige que no exista ninguna vía de ingreso por proveedor externo, o depende de una revisión humana? [Measurability, Spec §SC-010]

## Consistencia entre requisitos

- [x] CHK020 ¿Es consistente FR-029 con el modelo de datos? [Conflict, Spec §FR-029] — **RESUELTO 2026-08-20**: se corrigió FR-029. Los roles se asignan a la cuenta de usuario y son atribuibles al empleado a través de ella, sin cambio de esquema.
- [ ] CHK021 ¿Es consistente la ventana de exposición de FR-032 con el requisito de menor privilegio del principio VI, y está registrada la aceptación explícita de esos quince minutos? [Consistency, Spec §FR-032]
- [ ] CHK022 ¿Concuerdan FR-031, que permite ingresar sin roles, y FR-026, que exige autorizar toda operación, sobre qué puede hacer exactamente un usuario sin roles tras autenticarse? [Consistency, Spec §FR-031]
- [ ] CHK023 ¿Están alineados el alcance de esta feature y el de RF-CFG-02 sobre quién es responsable de la política de complejidad, sin dejar el requisito huérfano entre ambas? [Consistency, Spec §Assumptions]

## Cobertura de escenarios de excepción y recuperación

- [ ] CHK024 ¿Están definidos los requisitos para el escenario en que un administrador necesita expulsar de inmediato a un usuario comprometido? FR-032a lo cubre de forma parcial, atada a un cambio de estado o de roles. [Coverage, Spec §FR-032a]
- [ ] CHK025 ¿Existen requisitos para el escenario de fuga del secreto de firma del token, es decir invalidación masiva de sesiones? [Gap, Exception Flow]
- [ ] CHK026 ¿Está especificado qué debe ocurrir si la base de datos no está disponible durante un intento de ingreso, en términos de mensaje y de registro? [Gap, Exception Flow]
- [ ] CHK027 ¿Se define el comportamiento requerido cuando la revocación en cascada de FR-013 se dispara sobre el usuario legítimo, que pierde su sesión sin haber hecho nada? [Coverage, Spec §FR-013]

## Supuestos y dependencias

- [ ] CHK028 ¿Está validado el supuesto de que una ventana de quince minutos de autorización obsoleta es aceptable para el negocio, o se asumió por criterio técnico? [Assumption, Spec §Assumptions]
- [ ] CHK029 ¿Está documentado el supuesto de que la sede opera con red estable, y qué implica para la sesión una interrupción de conectividad durante una venta? [Assumption, Spec §Assumptions]
- [ ] CHK030 ¿Existe un esquema de identificadores que permita rastrear cada requisito de seguridad hasta su prueba, y está completo para los 38 requisitos funcionales? [Traceability]

## Notes

- Marcar con `[x]` los ítems resueltos y anotar la resolución en línea.
- Un ítem que resulte en cambio de la especificación DEBE reflejarse también en [plan.md](../plan.md) y en [tasks.md](../tasks.md).
- Los ítems CHK006, CHK020 y CHK012 son los de mayor impacto: el primero puede permitir sesiones indefinidas, el segundo es una contradicción con el modelo de datos, y el tercero deja abierta una vía de ataque que el bloqueo por cuenta no cubre.
