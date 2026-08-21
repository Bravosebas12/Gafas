# Análisis de valores límite — Feature 001

Los defectos se concentran en los bordes. Un `>` escrito donde iba `>=` no falla en el centro de
la clase válida: falla exactamente en el borde, y solo ahí. Por eso cada regla numérica o temporal
de esta feature se prueba en **tres puntos**: el valor inmediatamente anterior al límite, el límite
exacto y el valor inmediatamente posterior.

Cuando el límite es temporal, la unidad del salto importa. Probar "a los 15 minutos" con una
precisión de minutos no distingue `>` de `>=`; hay que bajar al segundo.

## Longitud de `nombreUsuario` — límites 1 y 100

Origen: contrato de `POST /api/auth/login`, columna `NOMBRE_USUARIO nvarchar(100)`.

| Valor | Clase | Esperado | Por qué este punto |
|---|---|---|---|
| 0 | por debajo del mínimo | 400 | detecta un `MinLength` ausente |
| 1 | mínimo exacto | 401 | detecta `> 1` escrito donde iba `>= 1` |
| 100 | máximo exacto | 401 | detecta `< 100` escrito donde iba `<= 100` |
| 101 | por encima del máximo | 400 | detecta un `MaxLength` ausente |

El resultado esperado en el borde válido es 401, no 200, porque un nombre de 1 o 100 caracteres
pasa la validación de formato y falla la de credenciales. **Esa es la aserción que importa**: el
borde separa el 400 del 401, no el 400 del 200. Si un nombre de 100 caracteres devolviera 400, el
sistema estaría rechazando entradas legítimas que la columna admite.

## Longitud de `contrasena` — rango 8 a 12 (FR-003a)

**Actualizado 2026-08-20.** El tope de 256 que este documento señalaba como indefendible fue
sustituido por un rango de **8 a 12 caracteres**, decidido por el responsable del proyecto y
registrado en la decisión D-01a de `research.md`. La observación de este análisis quedó atendida.

El rango se aplica en **dos planos distintos**, y esa distinción es lo que hay que probar:

| Plano | Qué se valida | Dónde |
|---|---|---|
| Establecer la contraseña | Rango completo, 8 a 12 | Primer Administrador (FR-035), y creación y cambio en RF-USR y RF-CFG |
| Ingresar | Solo el tope de 12 | `POST /api/auth/login` |

### Bordes en el ingreso

| Valor | Clase | Esperado | Por qué |
|---|---|---|---|
| 0 | vacío | 400 | Campo obligatorio |
| 1 | por debajo del mínimo de política | **401** | El mínimo **no** se valida en el ingreso: devolver 400 revelaría la política y rompería el mensaje genérico único de FR-004 |
| 7 | justo por debajo del mínimo | **401** | Mismo motivo |
| 8 | mínimo de política | 401 | Formato válido, credenciales incorrectas |
| 12 | máximo exacto | 401 | Formato válido, credenciales incorrectas |
| 13 | por encima del máximo | 400 | Único borde de longitud que produce 400 en el ingreso |

**La aserción que importa** es que el 400 aparece solo al superar 12. Los valores 1 y 7 son las
pruebas que evitan la regresión más probable de esta implementación: que alguien aplique el
`MinLength(8)` también en el validador del ingreso y, sin darse cuenta, convierta la longitud de la
contraseña en un canal que distingue "contraseña corta" de "credencial incorrecta".

### Bordes al establecer la contraseña

| Valor | Clase | Esperado |
|---|---|---|
| 7 | por debajo del mínimo | rechazo por validación |
| 8 | mínimo exacto | aceptado |
| 12 | máximo exacto | aceptado |
| 13 | por encima del máximo | rechazo por validación |

### Nota sobre la procedencia del límite

A diferencia del 100 del nombre de usuario, que sale de `NOMBRE_USUARIO nvarchar(100)`, y del 45
de `IP_ORIGEN nvarchar(45)`, este rango **no se deriva del esquema**: la contraseña no se almacena,
solo su derivación de tamaño fijo. Es una decisión de política, y como tal está registrada en
D-01a junto con su desviación explícita de NIST SP 800-63B, que exige admitir al menos 64
caracteres. El argumento del costo de derivación que este documento ya descartaba sigue siendo
inválido: en PBKDF2-HMAC-SHA256 una contraseña larga añade un hash, no seiscientos mil.

## Longitud de `IP_ORIGEN` — límite 45

Origen: `IP_ORIGEN nvarchar(45)` en `LoginAttempts`. El valor 45 es exactamente la longitud de una
dirección IPv6 en notación completa con sufijo IPv4 embebido.

| Valor | Esperado |
|---|---|
| 39 caracteres, IPv6 completa | se registra íntegra |
| 45 caracteres, IPv6 con IPv4 embebida | se registra íntegra, sin truncar |

El escenario usa `2001:0db8:85a3:0000:0000:8a2e:0370:7334`. Un truncamiento silencioso aquí no
rompe nada visible, pero corrompe la trazabilidad de FR-006 justo en el caso que interesa
investigar: el origen de un ataque.

## Intentos fallidos consecutivos — límite 5

Origen: FR-017.

| Valor | Esperado | Por qué este punto |
|---|---|---|
| 4 | sin bloqueo, y la contraseña correcta ingresa | detecta un bloqueo que dispara a los 4 |
| 5 | bloqueo, con `BLOQUEADO_HASTA` a 15 minutos | el límite exacto |
| 6 en adelante | sigue bloqueada, sin extender el bloqueo | detecta un bloqueo que se reinicia con cada intento |

El punto 4 se prueba con una aserción adicional: tras los 4 fallos, **la contraseña correcta debe
ingresar**. Sin esa aserción, un sistema que bloquea al cuarto intento pasaría la prueba de que
"a los 4 no hay `BLOQUEADO_HASTA`" si además tuviera un bloqueo en memoria no persistido.

## Ventana de acumulación — límite 15 minutos

Origen: FR-017, *"5 intentos fallidos consecutivos dentro de una ventana de 15 minutos"*.

| Escenario temporal | Esperado |
|---|---|
| 4 fallos, y el quinto a los 14 min 59 s del primero relevante | bloquea |
| 4 fallos, y el quinto a los 15 min 1 s | no bloquea; el contador queda en 1 |

Este es el límite más fácil de implementar mal, porque exige decidir **desde cuándo** se cuenta la
ventana: desde el primer fallo o desde el último. Los dos escenarios se anclan al último fallo, que
es la lectura de "consecutivos" que hace la especificación. Si la implementación eligiera la otra
lectura, el segundo escenario fallaría y forzaría la conversación, que es exactamente lo que una
prueba de borde debe provocar.

## Duración del bloqueo — límite 15 minutos

Origen: FR-018.

| Momento | Esperado |
|---|---|
| 14 min 59 s tras el bloqueo | 401 incluso con la contraseña correcta |
| 15 min 0 s | 200 con la contraseña correcta, contador en 0, `BLOQUEADO_HASTA` nulo |

El borde inferior se prueba con la **contraseña correcta**, no con una incorrecta. Con una
incorrecta el 401 sería indistinguible del rechazo normal y la prueba no demostraría nada.

## Vigencia del token de acceso — límite 15 minutos

Origen: FR-008.

| Momento | Esperado |
|---|---|
| 14 min 59 s tras la emisión | 200 en un endpoint protegido |
| 15 min 0 s | 401 |

## Vigencia de la sesión — límite absoluto de 8 horas

Origen: FR-009 y FR-009a.

| Momento | Esperado |
|---|---|
| 7 h 59 min tras la **autenticación** | la renovación devuelve 200 |
| 8 h 0 min | la renovación devuelve 401 y hay que autenticarse con contraseña |

El punto crítico es que el límite se mide desde la autenticación, **no desde el último uso**. El
escenario incluye la precondición de haber renovado cada 14 minutos de forma ininterrumpida
precisamente para matar la implementación que recalcula el vencimiento en cada rotación: bajo esa
implementación incorrecta, la sesión sería eterna y el escenario de las 8 h 0 min devolvería 200.

Un escenario adicional afirma el mecanismo de forma directa: tras renovar a las 4 horas, la fila
nueva de `RefreshTokens` debe tener `EXPIRES_AT` en la hora original de vencimiento, no cuatro
horas más tarde.

## Ventana de propagación de un cambio de roles — límite 15 minutos

Origen: FR-032 y FR-032a.

| Momento | Esperado |
|---|---|
| 14 min 59 s tras quitar los roles | el token viejo aún autoriza |
| 15 min 0 s | 401 |
| inmediato | la credencial de renovación ya está revocada |

Los dos primeros puntos verifican que la ventana existe y está acotada. El tercero verifica que
**no se puede extender**: sin la revocación inmediata de FR-032a, un usuario al que se le quitaron
los permisos podría renovar a los 14 minutos y conservar acceso otras 8 horas.

## Cantidad de administradores activos — límite 1

Origen: FR-030.

| Estado inicial | Operación | Esperado |
|---|---|---|
| 2 administradores | quitar el rol a uno | 200, queda 1 |
| 1 administrador | quitarse el rol a sí mismo | 400 con `ultimo-administrador` |
| 1 administrador | agregarse Vendedor conservando Administrador | 200 |

El tercer caso es el que distingue una implementación correcta de una que solo cuenta operaciones:
la regla se evalúa sobre el **estado final**, así que agregar un rol mientras se conserva
Administrador debe pasar. Una implementación que rechace cualquier modificación del último
administrador fallaría aquí, y sería un defecto de usabilidad real.

## Umbrales de cobertura — límites 85 % y 90 %

Origen: FR-036 y principio IV de la constitución.

| Ámbito | Valor | Esperado |
|---|---|---|
| global | 84.9 % | el proceso falla |
| global | 85.0 % | el proceso pasa |
| dominio y aplicación | 89.9 % | el proceso falla |
| dominio y aplicación | 90.0 % | el proceso pasa |

El límite es inclusivo: 85.0 % pasa. Un `>` en lugar de `>=` en la configuración de la compuerta
haría fallar builds correctos, y el equipo terminaría bajando el umbral por la razón equivocada.

## Concurrencia en los bordes

Los límites anteriores se prueban en secuencia, pero varios pueden cruzarse en paralelo. Estos
escenarios cubren el borde bajo carga simultánea:

| Situación | Esperado | Requisito |
|---|---|---|
| 5 intentos fallidos simultáneos | contador exactamente 5, cuenta bloqueada | FR-023 |
| 4 intentos fallidos simultáneos | contador exactamente 4, sin bloqueo | FR-023 |
| ingreso correcto y fallido simultáneos | contador en 0 o en 1, nunca bloqueada | FR-023 |
| 2 renovaciones con la misma credencial | exactamente un 200 y un 401 | FR-011 |
| 2 administradores quitándose el rol a la vez | exactamente un 200 y un 400, queda ≥ 1 admin | FR-030 |
| 2 creaciones de la misma cuenta a la vez | exactamente una tiene éxito | FR-034 |

El caso del ingreso correcto concurrente con uno fallido admite **dos resultados válidos**, 0 o 1,
porque el orden de aplicación no está determinado. Lo que no admite es un tercer resultado: que la
cuenta quede bloqueada. Fijar una sola respuesta donde el sistema tiene dos legítimas produce una
prueba intermitente, y una prueba intermitente se acaba desactivando.
