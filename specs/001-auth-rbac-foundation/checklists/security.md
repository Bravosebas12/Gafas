# Security Checklist: Fundación de Solución y Autenticación con Control de Acceso

**Purpose**: validar la **calidad de los requisitos de seguridad** antes de escribir código: si están completos, cuantificados, consistentes y verificables. No verifica implementación.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

**Nota**: cada ítem pregunta si el requisito **está bien escrito**, no si el sistema funciona. Un ítem marcado no significa que algo funcione, sino que el requisito es defendible.

## Completitud de los requisitos de credenciales

- [x] CHK001 ¿Está cuantificada la fuerza del hash de contraseña con criterios verificables —algoritmo, iteraciones, longitud de salt— o el requisito se queda en "función de hash segura"? [Clarity, Spec §FR-003] — **RESUELTO 2026-08-21**: delegación deliberada y correcta. La especificación declara la propiedad —hash irreversible con salt por usuario— y la decisión D-01 fija el mecanismo: PBKDF2-HMAC-SHA256, 600.000 iteraciones, salt de 128 bits, clave de 256. Cuantificar el algoritmo en el spec lo ataría a una tecnología, que es justo lo que la plantilla evita.
- [x] CHK002 ¿Existe algún requisito de longitud mínima o complejidad para la contraseña del **primer Administrador**? [Gap, Spec §FR-035] — **RESUELTO 2026-08-20**: FR-003a fija un mínimo de 8 caracteres aplicable donde la contraseña se establece, incluida la creación del primer Administrador. La política de **complejidad** sigue perteneciendo a RF-CFG-02.
- [x] CHK003 ¿Está definido el comportamiento ante una contraseña con espacios o caracteres no latinos? [Coverage, Spec §Edge Cases] — **RESUELTO 2026-08-20**: FR-003b lo declara de forma explícita, con escenario de prueba en `qa/.../features/autenticacion-login.feature`.
- [x] CHK004 ¿Se especifica un límite superior de longitud de contraseña, y qué ocurre al excederlo? [Gap] — **RESUELTO 2026-08-20**: máximo de 12 caracteres (FR-003a), con **rechazo** y nunca truncamiento. Decisión D-01a, que registra la desviación consciente de NIST SP 800-63B.
- [x] CHK005 ¿Está declarado como requisito que la contraseña no debe aparecer en ningún log, traza ni mensaje de error, o solo se deduce del principio VIII de la constitución? [Traceability, Spec §FR-003] — **RESUELTO 2026-08-21**: declarado en la regla R-U5 de data-model.md, exigido por la compuerta G8 y verificado por el caso CP-LOG-32, que concatena los campos del intento registrado y comprueba que no contiene la contraseña. No queda solo deducido del principio VIII.

## Completitud de los requisitos de sesión

- [x] CHK006 ¿Está especificado si la rotación de la credencial de renovación **reinicia** el plazo de 8 horas o si preserva el inicio original de la sesión? [Ambiguity, Spec §FR-009, §FR-011] — **RESUELTO 2026-08-20**: se añadió FR-009a. El vencimiento es absoluto desde la autenticación y la rotación lo hereda. Consecuencia aceptada: un turno de más de 8 horas exige autenticarse dos veces.
- [x] CHK007 ¿Existe un requisito de duración máxima absoluta de sesión, independiente de la renovación? [Gap] — **RESUELTO 2026-08-20**: FR-009a lo fija en 8 horas desde la autenticación.
- [ ] CHK008 ¿Está definido cuántas sesiones concurrentes puede tener un mismo usuario, y si un ingreso desde un dispositivo nuevo debe invalidar los anteriores? [Gap, Spec §US2]
- [x] CHK009 ¿Están especificados en la especificación los requisitos de transporte de las credenciales, o esa decisión vive solo en el plan? Si vive solo en el plan, ¿es una delegación deliberada? [Traceability, Spec §FR-007] — **RESUELTO 2026-08-21**: delegación deliberada. La especificación no fija el transporte; la decisión D-04 elige cookies `HttpOnly` y la desviación respecto al contrato dibujado en RF-LOG-01 está registrada en Complexity Tracking del plan.
- [ ] CHK010 ¿Se exige de forma explícita que la aplicación se sirva sobre canal cifrado? Ningún requisito lo menciona, y varias decisiones de sesión lo presuponen. [Gap]
- [ ] CHK011 ¿Están definidos requisitos sobre el algoritmo de firma del token y la rotación de su clave? [Gap, Spec §FR-015]

## Completitud de los requisitos de resistencia a ataques

- [x] CHK012 ¿Existe algún requisito de limitación por origen de la petición, además del bloqueo por cuenta? [Gap, Spec §FR-017] — **RESUELTO 2026-08-20**: queda **fuera de alcance** de forma explícita, con el riesgo aceptado por escrito en Assumptions. DEBE revisarse antes de exponer la aplicación a internet.
- [x] CHK013 ¿Está especificado el comportamiento requerido ante un volumen anómalo de intentos fallidos contra cuentas inexistentes? [Coverage, Spec §FR-022] — **RESUELTO 2026-08-21**: misma decisión de alcance que CHK012. No se exige respuesta automatizada, pero el volumen queda **detectable a posteriori**: FR-006 y FR-022 obligan a registrar todo intento contra cuenta inexistente con su origen y marca de tiempo.
- [x] CHK014 ¿Se define un requisito de detección o notificación ante bloqueos repetidos, o el bloqueo silencioso agota lo exigido? [Gap, Spec §FR-021] — **RESUELTO 2026-08-21**: fuera de alcance de forma deliberada. FR-021 exige auditar cada bloqueo, lo que deja la traza; la notificación proactiva pertenece al módulo de notificaciones, cuyo esquema `NOTIFICACIONES` ya existe pero ninguna feature de este alcance consume.
- [x] CHK015 ¿Está definido si el bloqueo debe poder levantarse de forma administrativa antes de los quince minutos, o si el vencimiento automático es la única vía? [Ambiguity, Spec §FR-018] — **RESUELTO 2026-08-21**: FR-018 lo responde, el vencimiento automático es la única vía —"sin intervención administrativa"— y data-model lo refuerza: el desbloqueo es implícito, ningún proceso limpia `BLOQUEADO_HASTA`.

## Claridad y medibilidad

- [x] CHK016 ¿Es "mensaje genérico e idéntico" verificable de forma objetiva, incluyendo el código de estado y el tiempo de respuesta, y no solo el texto? [Measurability, Spec §FR-004] — **RESUELTO 2026-08-21**: verificable en cuatro planos. Código 401, `title` y `type` literales en el contrato, igualación de tiempos por SC-004, y los casos CP-LOG-25 y CP-E2E-06, que comparan las cuatro causas entre sí en servidor y en pantalla.
- [x] CHK017 ¿Es el umbral de 100 milisegundos de SC-004 medible de forma reproducible, con un método de medición y un número de muestras definidos? [Measurability, Spec §SC-004] — **RESUELTO 2026-08-21**: el caso CP-LOG-46 fija el método: 50 peticiones por grupo y comparación de **medianas**, no de promedios, porque un solo pico desplaza el promedio y vuelve la prueba intermitente.
- [x] CHK018 ¿Está cuantificado "sin revelar el motivo del rechazo" de modo que se pueda distinguir un cumplimiento de un incumplimiento? [Clarity, Spec §FR-016] — **RESUELTO 2026-08-21**: cuantificado por enumeración. El contrato lista las cinco clases de token inválido y el caso correspondiente asevera que el cuerpo no menciona la firma, ni el vencimiento, ni el formato.
- [x] CHK019 ¿Puede verificarse objetivamente SC-010, que exige que no exista ninguna vía de ingreso por proveedor externo, o depende de una revisión humana? [Measurability, Spec §SC-010] — **RESUELTO 2026-08-21**: verificable sin criterio humano en tres niveles. T022 falla el arranque ante un proveedor declarado, un caso de arranque lo comprueba, y CP-E2E-11 inspecciona el documento renderizado y los destinos de todos los enlaces.

## Consistencia entre requisitos

- [x] CHK020 ¿Es consistente FR-029 con el modelo de datos? [Conflict, Spec §FR-029] — **RESUELTO 2026-08-20**: se corrigió FR-029. Los roles se asignan a la cuenta de usuario y son atribuibles al empleado a través de ella, sin cambio de esquema.
- [x] CHK021 ¿Es consistente la ventana de exposición de FR-032 con el requisito de menor privilegio del principio VI, y está registrada la aceptación explícita de esos quince minutos? [Consistency, Spec §FR-032] — **RESUELTO 2026-08-21**: la aceptación está por escrito en Assumptions, con fecha y razón —evitar una consulta a base de datos por petición en el punto de venta—, y FR-032a acota la ventana impidiendo extenderla por renovación.
- [x] CHK022 ¿Concuerdan FR-031, que permite ingresar sin roles, y FR-026, que exige autorizar toda operación, sobre qué puede hacer exactamente un usuario sin roles tras autenticarse? [Consistency, Spec §FR-031] — **RESUELTO 2026-08-21**: el contrato de roles lo declara sin ambigüedad. Un usuario sin roles satisface solo la política `Autenticado`, es decir cierre de sesión y consulta de su propia sesión; toda otra operación responde 403.
- [x] CHK023 ¿Están alineados el alcance de esta feature y el de RF-CFG-02 sobre quién es responsable de la política de complejidad, sin dejar el requisito huérfano entre ambas? [Consistency, Spec §Assumptions] — **RESUELTO 2026-08-21**: sin requisito huérfano. FR-003a es dueño de la **longitud** en esta feature; la **complejidad** pertenece a RF-CFG-02 y RF-USR-02, y Assumptions lo declara de forma explícita.

## Cobertura de escenarios de excepción y recuperación

- [ ] CHK024 ¿Están definidos los requisitos para el escenario en que un administrador necesita expulsar de inmediato a un usuario comprometido? FR-032a lo cubre de forma parcial, atada a un cambio de estado o de roles. [Coverage, Spec §FR-032a]
- [ ] CHK025 ¿Existen requisitos para el escenario de fuga del secreto de firma del token, es decir invalidación masiva de sesiones? [Gap, Exception Flow]
- [ ] CHK026 ¿Está especificado qué debe ocurrir si la base de datos no está disponible durante un intento de ingreso, en términos de mensaje y de registro? [Gap, Exception Flow]
- [x] CHK027 ¿Se define el comportamiento requerido cuando la revocación en cascada de FR-013 se dispara sobre el usuario legítimo, que pierde su sesión sin haber hecho nada? [Coverage, Spec §FR-013] — **RESUELTO 2026-08-21**: el contrato de `POST /api/auth/refresh` lo aborda de frente: el usuario legítimo pierde la sesión y eso es deliberado, porque significa que alguien más tenía su credencial. Verificado por el caso de reutilización.

## Supuestos y dependencias

- [x] CHK028 ¿Está validado el supuesto de que una ventana de quince minutos de autorización obsoleta es aceptable para el negocio, o se asumió por criterio técnico? [Assumption, Spec §Assumptions] — **RESUELTO 2026-08-21**: decidido por el responsable del proyecto el 2026-08-20 y registrado en Assumptions con su razón. No es un supuesto técnico silencioso.
- [x] CHK029 ¿Está documentado el supuesto de que la sede opera con red estable, y qué implica para la sesión una interrupción de conectividad durante una venta? [Assumption, Spec §Assumptions] — **RESUELTO 2026-08-21**: el supuesto está documentado en Assumptions —una sede, red estable, sin operación sin conexión—. El efecto de una caída a mitad de venta pertenece a la feature del punto de venta, que es la dueña de ese flujo.
- [x] CHK030 ¿Existe un esquema de identificadores que permita rastrear cada requisito de seguridad hasta su prueba, y está completo para los 38 requisitos funcionales? [Traceability] — **RESUELTO 2026-08-21**: existe en dos matrices. `contracts/trazabilidad.md` mapea los 32 escenarios de aceptación y los 9 casos borde al tipo de prueba, y `qa/.../trazabilidad.md` mapea requisito a escenario. **Corrección**: los requisitos funcionales son 41, no 38, desde que se añadieron FR-003a, FR-003b, FR-009a y FR-032a.

## Notes

- Marcar con `[x]` los ítems resueltos y anotar la resolución en línea.
- Un ítem que resulte en cambio de la especificación DEBE reflejarse también en [plan.md](../plan.md) y en [tasks.md](../tasks.md).
- Los ítems CHK006, CHK020 y CHK012 son los de mayor impacto: el primero puede permitir sesiones indefinidas, el segundo es una contradicción con el modelo de datos, y el tercero deja abierta una vía de ataque que el bloqueo por cuenta no cubre.
