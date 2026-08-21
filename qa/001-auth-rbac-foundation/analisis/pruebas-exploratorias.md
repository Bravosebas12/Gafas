# Pruebas exploratorias — Feature 001

Los escenarios Gherkin comprueban lo que la especificación dice. Las pruebas exploratorias buscan
lo que **no dice**: la suposición que nadie escribió, la interacción entre dos reglas correctas por
separado, el comportamiento en un estado que el diseño no contempló.

Se organizan por **cartas de exploración** al estilo de gestión de sesiones: cada carta declara una
misión, un área, una duración acotada y un oráculo —cómo se reconoce un problema cuando aparece—.
No llevan pasos guionados; si se pudieran guionar, serían escenarios Gherkin y no harían falta aquí.

## Cómo se ejecuta y qué se entrega

Cada sesión es de tiempo fijo y termina con notas: qué se probó, qué se encontró, qué quedó sin
mirar y qué preguntas nuevas surgieron. Un hallazgo confirmado se convierte en un escenario de
regresión en `../features/` y deja de ser exploratorio. La carta que no encuentra nada también
informa: dice que el área es sólida y por qué se buscó ahí.

Estas sesiones **no se automatizan**. Son deliberadamente de un solo uso y dependen del juicio de
quien las ejecuta, lo que las coloca en el lado de "permanece manual" del criterio de graduación a
automatización.

---

## E-01 · Enumeración de cuentas por canales laterales

**Misión:** averiguar si algún canal, distinto del cuerpo de la respuesta, permite distinguir una
cuenta existente de una inexistente.
**Área:** `POST /api/auth/login`, FR-004, SC-004.
**Duración:** 60 minutos.

**Dónde buscar.** El cuerpo y el código de estado ya están cubiertos por escenarios. Lo que no está
cubierto son los demás canales: longitud exacta de la respuesta en bytes, orden y presencia de
cabeceras, cookies emitidas en el fallo, presencia o ausencia de `Set-Cookie` vacío, tiempo de
respuesta bajo carga y con caché frío, comportamiento tras el quinto fallo en una cuenta que existe
frente a una que no, y contenido de los registros accesibles a un operador con permisos mínimos.

**Oráculo:** cualquier diferencia observable y reproducible entre las dos situaciones es un
hallazgo, incluso si el cuerpo es idéntico. La derivación PBKDF2 señuelo del contrato cubre el
tiempo; la pregunta abierta es si cubre todo lo demás.

**Riesgo que justifica la carta:** la especificación protege la respuesta, no el resto del
intercambio. Un `Set-Cookie` presente solo cuando la cuenta existe delata tanto como un mensaje
distinto.

---

## E-02 · Interacción entre bloqueo y sesión activa

**Misión:** determinar qué ocurre con las sesiones ya abiertas cuando la cuenta se bloquea por
fuerza bruta.
**Área:** FR-017 a FR-019 cruzados con FR-007 a FR-014.
**Duración:** 45 minutos.

**Dónde buscar.** La especificación define el bloqueo sobre el **ingreso**, y no dice nada sobre las
sesiones vigentes. Si un empleado tiene sesión abierta en el punto de venta y un tercero bloquea su
cuenta con cinco intentos fallidos desde otra máquina, ¿el empleado sigue trabajando? ¿Puede
renovar? ¿Puede renovar después de que el bloqueo expire? ¿Y si el bloqueo ocurre justo entre el
vencimiento de su token y su renovación?

**Oráculo:** el comportamiento debe ser el mismo en las tres corridas y debe poder explicarse en una
frase. Si no está definido, el hallazgo no es un defecto de código sino un **hueco de
especificación**, y eso es igual de valioso.

**Riesgo que justifica la carta:** un tercero podría cortar la sesión de un vendedor a mitad de
venta con solo cinco intentos fallidos, convirtiendo una defensa en una vía de denegación de
servicio contra un compañero.

---

## E-03 · Desfase de relojes y cambios de hora

**Misión:** provocar incoherencias en los vencimientos manipulando la relación entre el reloj de la
aplicación y el de la base de datos.
**Área:** FR-008, FR-009, FR-018, principio X sobre UTC.
**Duración:** 60 minutos.

**Dónde buscar.** El esquema pone `SYSUTCDATETIME()` como valor por omisión en varias columnas,
mientras la aplicación calcula vencimientos con su propia abstracción de reloj. Son **dos fuentes
de tiempo**. Qué pasa si difieren en 30 segundos, en 5 minutos, o si la base va por detrás. Qué pasa
en el cambio de horario de verano de una zona con horario local, si algún punto del sistema lo usa.
Qué pasa si se compara un valor con zona contra uno sin zona.

**Oráculo:** un bloqueo que dure más o menos de 15 minutos reales, una sesión que sobreviva a sus 8
horas, o un token aceptado tras vencer. Cualquier vencimiento que dependa de qué reloj se consultó
es un hallazgo.

**Riesgo que justifica la carta:** es el caso borde que la propia especificación anticipa y el más
difícil de cubrir con escenarios deterministas, porque el reloj de prueba sustituye justamente lo
que aquí se quiere estresar.

---

## E-04 · Estados imposibles en la base de datos

**Misión:** construir a mano estados que la aplicación nunca produciría y observar cómo reacciona.
**Área:** modelo de datos, FR-030, FR-031, FR-034.
**Duración:** 45 minutos.

**Dónde buscar.** Insertar directamente en SQL: un usuario con `BLOQUEADO_HASTA` en el pasado y
`INTENTOS_FALLIDOS` en 5; un usuario con `INTENTOS_FALLIDOS` negativo; un usuario activo cuyo
empleado no existe, si la restricción lo permite; una fila de `UsuariosRoles` apuntando a un rol
borrado; cero usuarios con rol Administrador; dos filas en `UsuariosRoles` con el mismo par;
`EXPIRES_AT` anterior a la fecha de creación en `RefreshTokens`.

**Oráculo:** la aplicación debe fallar de forma controlada y explicable, o corregir el estado. Lo
que no debe hacer es conceder acceso indebido, quedar en bucle, o exponer un error con detalles
internos. El caso de cero administradores es especialmente interesante: FR-030 impide llegar ahí
por la aplicación, pero no dice qué hacer si ya se llegó por otra vía.

**Riesgo que justifica la carta:** una restauración de respaldo parcial, una corrección manual en
producción o una migración a medias producen exactamente estos estados.

---

## E-05 · Concurrencia en el borde del bloqueo

**Misión:** buscar ventanas de carrera en el conteo de intentos y en el bloqueo, más allá de los
escenarios ya escritos.
**Área:** FR-017, FR-020, FR-023.
**Duración:** 60 minutos.

**Dónde buscar.** Un ingreso correcto que llega en el mismo milisegundo que el quinto fallo. Cinco
fallos simultáneos justo cuando el bloqueo anterior expira. Un cambio de contraseña, cuando exista,
concurrente con un intento fallido. Intentos desde varias direcciones a la vez sobre la misma
cuenta. El quinto fallo llegando durante la escritura de auditoría del cuarto.

**Oráculo:** el contador nunca queda por encima de 5 ni por debajo de los fallos realmente
ocurridos; nunca hay una cuenta bloqueada con contador en 0; el número de filas en `LoginAttempts`
coincide exactamente con el número de peticiones enviadas. La atomicidad que el contrato promete en
una sola sentencia debe sostenerse bajo presión.

**Riesgo que justifica la carta:** el contrato resuelve el conteo en una sentencia atómica, y las
pruebas escritas verifican los casos que se anticiparon. Las carreras que importan son las que no
se anticiparon.

---

## E-06 · Recorrido de la pantalla de ingreso con teclado y lector

**Misión:** verificar que el ingreso es operable sin ratón y con lector de pantalla, y que ningún
estado deja al usuario sin salida.
**Área:** principio IX, pantalla de ingreso con render en servidor.
**Duración:** 45 minutos.

**Dónde buscar.** Recorrido completo con tabulación: orden, foco visible, envío con Enter.
Comportamiento del mensaje de error genérico: si se anuncia al lector, si el foco vuelve al campo
correcto, si se limpia al reintentar. Estado durante el envío: si el botón permite doble envío, si
hay indicación de progreso. Ancho de 360 píxeles. Contraste real de los mensajes de error sobre su
fondo. Comportamiento con autocompletado del navegador y con gestor de contraseñas.

**Oráculo:** cada paso del flujo se completa sin ratón; el error se percibe sin ver la pantalla;
ningún estado queda sin indicación. Un doble envío que produzca dos intentos fallidos contados es a
la vez un defecto de interfaz y de seguridad, porque consume el presupuesto de cinco intentos al
doble de velocidad.

**Riesgo que justifica la carta:** es la única pantalla de la feature, y es la puerta de todo el
sistema. Un vendedor con las manos ocupadas y un lector de pantalla no son casos hipotéticos.

---

## E-07 · Contenido del token y de los registros

**Misión:** buscar filtraciones de datos sensibles en todo lo que el sistema emite o persiste.
**Área:** FR-015, FR-037, principio VIII.
**Duración:** 45 minutos.

**Dónde buscar.** Decodificar el token de acceso y revisar cada claim, incluidos los que el marco
agrega por omisión y que nadie declaró. Provocar una excepción no controlada y leer la respuesta
completa. Revisar los registros de un ingreso fallido, de una renovación rechazada y de un cambio de
roles, buscando la contraseña, el token, la credencial de renovación, el hash o el salt. Revisar
qué queda en los mensajes de error de validación de formato. Revisar las cabeceras de respuesta.

**Oráculo:** ningún valor secreto aparece en ningún canal observable. El claim que nadie declaró
pero el marco agregó es el hallazgo típico de esta carta.

**Riesgo que justifica la carta:** FR-015 acota lo que el token **debe** contener, pero los marcos
agregan claims por omisión, y un registro de excepción imprime el objeto completo sin preguntar.

---

## Cobertura y huecos conocidos

| Área de la feature | Cartas que la tocan |
|---|---|
| Autenticación y enumeración | E-01, E-06, E-07 |
| Sesión, renovación y rotación | E-02, E-03, E-07 |
| Bloqueo de cuenta | E-01, E-02, E-03, E-05 |
| Control de acceso y roles | E-04, E-07 |
| Modelo de datos e integridad | E-03, E-04 |
| Interfaz y accesibilidad | E-06 |

**Sin carta asignada, y es deliberado:** la herramienta de creación del primer Administrador y la
compuerta de cobertura. Ambas están cubiertas por escenarios deterministas en
`../features/base-tecnica.feature`, se ejecutan una vez por instalación o en cada compilación, y no
tienen la variabilidad de estado que hace productiva una exploración. Explorarlas gastaría tiempo
donde el guion ya alcanza.
