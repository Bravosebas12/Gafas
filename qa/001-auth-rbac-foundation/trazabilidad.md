# Trazabilidad — Feature 001-auth-rbac-foundation

La trazabilidad existe para responder una pregunta: **qué requisito no tiene ninguna prueba**. Una
tabla donde todo aparece cubierto no sirve de nada; lo que sirve es la sección de huecos al final.

- **Escenarios:** 81, en 5 archivos `.feature`
- **Requisitos funcionales:** 39 de 39 con al menos un escenario
- **Criterios de éxito:** 9 de 10 con escenario, 1 sin cubrir
- **Cartas de exploración:** 7

## Requisitos funcionales

| Requisito | Archivo | Escenarios | Técnica |
|---|---|---|---|
| FR-001 validar contra base propia | autenticacion-login | 2 | equivalencia |
| FR-002 rechazar proveedor externo | autenticacion-login | 2 | equivalencia |
| FR-003 hash y salt | autenticacion-login | 3 | equivalencia |
| FR-004 mensaje genérico idéntico | autenticacion-login, bloqueo-cuenta | 5 | equivalencia, límite |
| FR-005 impedir ingreso a desactivados | autenticacion-login | 1 | equivalencia |
| FR-006 registrar todo intento | autenticacion-login | 3 | equivalencia, límite |
| FR-007 emitir par de credenciales | sesion-y-renovacion | 1 | equivalencia |
| FR-008 token de acceso a 15 min | sesion-y-renovacion | 1 | límite |
| FR-009 sesión de 8 horas | sesion-y-renovacion | 1 | límite |
| FR-009a vencimiento absoluto | sesion-y-renovacion | 2 | límite |
| FR-010 credencial hasheada | sesion-y-renovacion | 1 | equivalencia |
| FR-011 rotación en cada uso | sesion-y-renovacion | 2 | equivalencia, concurrencia |
| FR-012 rechazar credencial inválida | sesion-y-renovacion | 2 | equivalencia |
| FR-013 revocar cadena por reutilización | sesion-y-renovacion | 1 | equivalencia |
| FR-014 cierre de sesión | sesion-y-renovacion | 2 | equivalencia |
| FR-015 token sin datos sensibles | sesion-y-renovacion | 1 | equivalencia |
| FR-016 rechazar token inválido | sesion-y-renovacion | 2 | equivalencia |
| FR-017 bloqueo a los 5 intentos | bloqueo-cuenta | 4 | límite |
| FR-018 bloqueo de 15 minutos | bloqueo-cuenta | 2 | límite |
| FR-019 rechazo durante el bloqueo | bloqueo-cuenta | 2 | equivalencia |
| FR-020 reinicio del contador | bloqueo-cuenta | 2 | equivalencia, límite |
| FR-021 auditoría del bloqueo | bloqueo-cuenta | 2 | equivalencia |
| FR-022 cuentas inexistentes | autenticacion-login, bloqueo-cuenta | 2 | equivalencia |
| FR-023 conteo concurrente | bloqueo-cuenta | 3 | concurrencia |
| FR-024 conjunto cerrado de roles | control-acceso-roles | 3 | equivalencia |
| FR-025 varios roles simultáneos | control-acceso-roles | 1 | equivalencia |
| FR-026 validación en el servidor | control-acceso-roles | 2 | equivalencia |
| FR-027 solo Administrador cambia roles | control-acceso-roles | 3 | equivalencia |
| FR-028 auditoría del cambio de roles | control-acceso-roles | 2 | equivalencia |
| FR-029 roles en la cuenta, no en el empleado | control-acceso-roles | 2 | equivalencia |
| FR-030 último administrador | control-acceso-roles | 4 | límite, concurrencia |
| FR-031 usuario sin roles | control-acceso-roles | 1 | límite |
| FR-032 ventana de 15 minutos | control-acceso-roles | 1 | límite |
| FR-032a revocación inmediata | control-acceso-roles | 2 | equivalencia |
| FR-033 regla de dependencias | base-tecnica | 5 | equivalencia |
| FR-034 modelo de datos existente | base-tecnica | 3 | equivalencia, concurrencia |
| FR-035 primer Administrador | base-tecnica | 3 | equivalencia |
| FR-036 umbrales de cobertura | base-tecnica | 2 | límite |
| FR-037 log estructurado | base-tecnica | 2 | equivalencia |

## Criterios de éxito

| Criterio | Cubierto por | Estado |
|---|---|---|
| SC-001 ingreso en menos de 3 segundos | — | **sin cubrir** |
| SC-002 jornada de 8 horas sin reescribir contraseña | La sesión vence 8 horas después de la autenticación | cubierto |
| SC-003 cinco fallos bloquean el 100 % de las veces | El quinto intento fallido consecutivo bloquea la cuenta, más los 3 de concurrencia | cubierto |
| SC-004 diferencia de tiempo menor a 100 ms | El tiempo de respuesta no permite deducir si una cuenta existe | cubierto |
| SC-005 el 100 % de operaciones protegidas rechaza sin rol | Solo el Administrador puede modificar roles, más los 2 de FR-026 | cubierto |
| SC-006 credencial reutilizada nunca emite token | Reutilizar una credencial ya rotada revoca toda la cadena | cubierto |
| SC-007 nada en claro en la base de datos | La contraseña no se almacena en claro, La credencial se almacena hasheada | cubierto |
| SC-008 el 100 % de operaciones es reconstruible | Los 3 de FR-006, los 2 de FR-021, los 2 de FR-028 | cubierto |
| SC-009 cobertura ≥ 85 % y ≥ 90 % | El umbral de cobertura falla el proceso por debajo del límite | cubierto |
| SC-010 ninguna vía con proveedor externo | El sistema no expone ninguna vía de ingreso con proveedor externo | cubierto |

## Compuertas de la constitución

| Compuerta | Cubierta por |
|---|---|
| G1 regla de dependencias | 5 escenarios de FR-033, incluida la falla de compilación deliberada |
| G3 cobertura | 2 escenarios de FR-036, con los cuatro bordes |
| G5 sin proveedores externos ni secretos | 2 de FR-002 y 1 de FR-035 |
| G6 reglas críticas validadas en el servidor | 2 de FR-026, invocando el servidor sin pasar por la interfaz |
| G7 auditoría en la misma transacción | Si el cambio de roles falla, no queda auditoría huérfana |
| G8 log con correlación y sin datos sensibles | 2 escenarios de FR-037 |
| G10 sin cambios de esquema no propuestos | 3 escenarios de FR-034 |

Sin escenario en esta carpeta: **G2** (casos de uso como comandos y consultas, sin lógica en
controladores) y **G9** (cuatro estados de vista, teclado, contraste y tokens). G2 se verifica por
revisión de código, no por prueba ejecutable. G9 corresponde a la pantalla de ingreso y se cubre
por la carta de exploración E-06 más la checklist de la skill de interfaz; cuando lleguen las
pantallas del módulo RF-USR merecerá escenarios propios.

## Huecos identificados

**SC-001, ingreso en menos de 3 segundos, no tiene escenario.** Es deliberado y conviene explicar
por qué. El criterio mide el recorrido completo desde el envío del formulario hasta la pantalla
principal, lo que incluye render, red y latencia del navegador. Medirlo con un escenario de
servidor daría un número que no corresponde al criterio, y medirlo de extremo a extremo exige la
pantalla montada y un entorno representativo, que no existen todavía. Queda como prueba de
rendimiento a ejecutar cuando la pantalla de ingreso esté construida. Registrarlo como hueco es
más honesto que cubrirlo con una prueba que mide otra cosa.

**Interacción entre bloqueo y sesiones activas: hueco de especificación, no de pruebas.** La carta
E-02 lo persigue. FR-017 a FR-019 definen el bloqueo sobre el ingreso y no dicen nada sobre las
sesiones ya abiertas. Hasta que la especificación lo defina no se puede escribir un escenario, y
escribirlo con una suposición nuestra sería inventar el requisito. Es la clase de hueco que la
exploración convierte en pregunta antes de que se convierta en defecto.

**Cero administradores por vía externa.** FR-030 impide llegar a ese estado desde la aplicación,
pero no define qué hacer si ya se llegó por una restauración parcial o una corrección manual. La
carta E-04 lo explora.

## Graduación a automatización

Orden por retorno, no por volumen. Lo primero que se automatiza es lo que se ejecuta en cada
entrega y protege una regresión crítica; lo último, lo que depende de juicio humano.

**Automatizar primero.** Los escenarios etiquetados `@smoke` y `@seguridad`: ingreso correcto,
bloqueo al quinto intento, rechazo con contraseña correcta durante el bloqueo, rotación y
reutilización de credencial, y los tres de rechazo por rol invocando el servidor directamente. Son
deterministas, se ejecutan en cada entrega y su fallo es inequívoco.

**Automatizar en la misma tanda, con soporte de infraestructura.** Los `@concurrencia` y los
`@limite` temporales. Requieren reloj de prueba sustituible y base de datos real, así que dependen
de que esa infraestructura exista, pero su valor de regresión es alto: son precisamente los
defectos que una revisión de código no ve.

**Permanece manual.** Las siete cartas de exploración, por definición. Y el recorrido de
accesibilidad de E-06: las herramientas automáticas detectan una fracción de los problemas reales
de teclado y lector de pantalla, y el resto necesita a alguien recorriendo la pantalla.

**No automatizar por ahora.** SC-001, hasta que exista la pantalla y un entorno representativo.
Automatizar una medición de rendimiento contra un entorno no representativo produce un número que
nadie puede interpretar, y una prueba que nadie interpreta se acaba desactivando.
