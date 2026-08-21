# Particiones de equivalencia — Feature 001

La técnica consiste en dividir el dominio de cada entrada en clases donde el sistema debe
comportarse igual, y probar **un representante por clase**. Probar tres nombres de usuario
inexistentes distintos no aporta nada: los tres pertenecen a la misma clase. Lo que aporta es no
dejar ninguna clase sin representante.

Cada clase de esta tabla tiene su escenario en `../features/`. La columna de escenario es el
enlace entre el análisis y la prueba.

## Entrada: `nombreUsuario` en el ingreso

| # | Clase | Representante | Resultado esperado | Escenario |
|---|---|---|---|---|
| V1 | Cuenta existente, activa, contraseña correcta | `jperez` / `Optica2026#` | 200 con cookies de sesión | Credenciales correctas sobre cuenta activa conceden acceso |
| I1 | Cuenta inexistente | `fantasma` | 401 genérico | Las cuatro clases de rechazo devuelven una respuesta idéntica |
| I2 | Cuenta existente, contraseña incorrecta | `jperez` / `ClaveMala#1` | 401 genérico | ídem |
| I3 | Cuenta existente pero desactivada | `mlopez` con `ACTIVO = 0` | 401 genérico | Una cuenta desactivada lógicamente no puede ingresar |
| I4 | Cuenta existente pero bloqueada | `rgomez` con bloqueo vigente | 401 genérico | Durante el bloqueo se rechaza incluso la contraseña correcta |
| F1 | Formato inválido por longitud | 0 o 101 caracteres | 400 de validación | El límite de longitud del nombre de usuario separa el 400 del 401 |

**Decisión de diseño que la técnica hace visible:** las clases I1 a I4 son cuatro clases distintas
del punto de vista del sistema, pero FR-004 exige que produzcan una **respuesta indistinguible**.
Por eso se probaron como un único esquema de escenario con cuatro ejemplos y una aserción sobre la
igualdad de la respuesta, en lugar de cuatro escenarios independientes: lo que se verifica no es
cada clase por separado, sino que ninguna se distinga de las otras.

La clase F1 existe únicamente para separar el 400 del 401. Confundirlas es un defecto de
seguridad: un 400 en un nombre de 101 caracteres y un 401 en uno de 100 revelan dónde está el
límite de validación, pero no revelan si la cuenta existe, que es lo que importa proteger.

## Entrada: `contrasena` en el ingreso

| # | Clase | Representante | Resultado esperado | Escenario |
|---|---|---|---|---|
| V2 | Correcta para la cuenta | `Optica2026#` | 200 | Credenciales correctas sobre cuenta activa conceden acceso |
| V3 | Correcta con caracteres no latinos y espacios | `Ñandú Ó¢2026`, 12 caracteres | 200, sin corrupción | Una contraseña con caracteres no latinos y espacios se acepta sin corrupción |
| I5 | Incorrecta | `ClaveMala#1` | 401 genérico | Las cuatro clases de rechazo devuelven una respuesta idéntica |
| F2 | Vacía | `""` | 400, campo obligatorio | El límite de longitud de la contraseña separa el 400 del 401 |
| F3 | Por debajo del mínimo de política | 1 o 7 caracteres | **401**, no 400: aplicar el mínimo aquí revelaría la política y rompería FR-004 | ídem |
| F4 | Por encima del tope de FR-003a | 13 caracteres | 400 | ídem |

La clase V3 no aparece en la especificación: sale del caso borde *"contraseña con caracteres no
latinos, espacios o longitud extrema"*. Es la clase que rompe cuando la columna, la conexión o el
`sqlcmd` usan una codificación distinta a UTF-8, y el repositorio ya tiene precedente de eso: el
`README.md` de `Scripts/SQL` documenta que `Optómetra` se guardó como `OptÃ³metra` en la primera
carga.

## Entrada: credencial de renovación

| # | Clase | Representante | Resultado esperado | Escenario |
|---|---|---|---|---|
| V4 | Vigente, no rotada, del propio usuario | `RT-1` recién emitida | 200 con par nuevo | Renovar con una credencial válida emite un par nuevo y anula el anterior |
| I6 | Vencida por cumplirse las 8 horas | `RT-1` a las 8 h 1 min | 401 | Las cuatro clases de credencial inválida rechazan la renovación |
| I7 | Revocada por cierre de sesión | `RT-1` tras logout | 401 | ídem |
| I8 | Ya rotada | `RT-1` tras usar `RT-2` | 401 **y revocación de la cadena** | Reutilizar una credencial ya rotada revoca toda la cadena del usuario |
| I9 | De otro usuario | credencial de `vend1` presentada por `opto1` | 401 | Las cuatro clases de credencial inválida rechazan la renovación |
| I10 | Ausente | sin cookie | 401 | Renovar sin presentar credencial se rechaza |

**La clase I8 no es equivalente a las demás y por eso tiene escenario propio.** Las clases I6, I7,
I9 e I10 solo rechazan la petición; I8 además dispara la revocación de todas las credenciales
activas del usuario, porque una credencial ya rotada que reaparece significa que alguien más la
tenía. Meterla en el mismo esquema habría escondido el efecto que la hace importante.

## Entrada: token de acceso en un endpoint protegido

| # | Clase | Representante | Resultado esperado | Escenario |
|---|---|---|---|---|
| V5 | Válido y vigente | emitido hace 1 min | 200 | El token de acceso vence exactamente a los 15 minutos |
| I11 | Firma inválida | firmado con otra clave | 401 sin detalle | Los tokens de acceso inválidos se rechazan sin revelar el motivo |
| I12 | Vencido | emitido hace 16 min | 401 sin detalle | ídem |
| I13 | Cuerpo alterado tras la firma | claims modificados | 401 sin detalle | ídem |
| I14 | Malformado | cadena arbitraria | 401 sin detalle | ídem |
| I15 | Ausente | sin cookie | 401 | ídem |
| I16 | Válido pero de otro emisor | emisor `otra-optica` | 401 | Un token válido de otra instalación no concede acceso |

## Entrada: conjunto de roles en `PUT /api/users/{id}/roles`

| # | Clase | Representante | Resultado esperado | Escenario |
|---|---|---|---|---|
| V6 | Un rol del conjunto cerrado | `["Vendedor"]` | 200 | Solo el Administrador puede modificar roles |
| V7 | Varios roles del conjunto cerrado | `["Optometra", "Vendedor"]` | 200 | Un cambio de roles se audita con valor anterior y posterior |
| V8 | Conjunto vacío | `[]` | 200, el usuario queda sin roles | Un usuario sin roles ingresa pero no autoriza ninguna operación protegida |
| I17 | Rol inexistente | `Supervisor` | 400 | Un código de rol fuera del conjunto cerrado se rechaza |
| I18 | Código con la caja alterada | `administrador` | 400 | ídem |
| I19 | Nombre visible en vez del código | `Optómetra` | 400 | ídem |
| I20 | Cadena vacía como código | `""` | 400 | ídem |
| I21 | Intento de inyección | `Vendedor'--` | 400 | ídem |
| I22 | Estado final sin administradores | quitar el rol al único admin | 400 con `ultimo-administrador` | Con un único administrador activo no se puede dejar el sistema sin ninguno |

La clase I19 merece atención: el contrato advierte que el código es `Optometra` sin tilde y el
nombre visible `Optómetra` con tilde. Enviar el nombre visible donde va el código es el error más
probable que cometerá quien integre, y sin esta clase pasaría inadvertido hasta producción.

## Entrada: rol del solicitante en una operación protegida

| # | Clase | Representante | Resultado esperado | Escenario |
|---|---|---|---|---|
| V9 | Tiene el rol requerido | `admin1` en `GET /api/roles` | 200 | El sistema reconoce exactamente tres roles |
| V10 | Tiene varios roles, uno satisface | `vend1` con Vendedor y Administrador | 200 | Un usuario con dos roles se autoriza si cualquiera satisface la política |
| I23 | Autenticado sin el rol requerido | `vend1` en `GET /api/roles` | 403 | Un usuario sin el rol requerido es rechazado al invocar directamente el servidor |
| I24 | Autenticado sin ningún rol | `sinrol1` | 403 | Un usuario sin roles ingresa pero no autoriza ninguna operación protegida |
| I25 | Sin autenticar | sin sesión | 401 | Sin sesión no se puede modificar roles |
| I26 | Usuario objetivo inexistente | id `999999` | 404 | Modificar los roles de un usuario inexistente devuelve no encontrado |

La distinción entre I25 y I23 es la distinción entre 401 y 403, y es sustantiva: *no sé quién eres*
frente a *sé quién eres y no puedes*. Colapsarlas en un solo código es un defecto de contrato que
esta partición fuerza a mirar.

## Clases deliberadamente fuera de alcance

| Clase | Por qué no se prueba aquí |
|---|---|
| Contraseña que no cumple la política de complejidad | Pertenece a RF-CFG-02 y RF-USR-02. Esta feature solo exige almacenamiento seguro |
| Creación, edición y desactivación de usuarios por interfaz | Pertenece a la feature del módulo RF-USR |
| Operaciones de inventario y ventas por rol | El mapa completo de permisos se define en las features de cada módulo. Aquí se prueba el mecanismo |
