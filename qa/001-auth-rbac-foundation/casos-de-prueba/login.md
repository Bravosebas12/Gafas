# Casos de prueba automatizados — Ingreso al sistema

**Feature:** 001-auth-rbac-foundation · **Historia:** US1, Ingreso con credenciales propias
**Requisitos:** FR-001 a FR-006 · **Contrato:** `POST /api/auth/login`
**Escenarios origen:** [autenticacion-login.feature](../features/autenticacion-login.feature)

Estos son los casos que se automatizan para el ingreso. Cada uno tiene título, precondiciones,
pasos discretos, un resultado esperado observable por paso y datos concretos. Un caso al que le
falten las precondiciones o los datos no es reproducible, y un resultado esperado que dos personas
no puedan acordar sin adivinar no sirve como criterio de aprobación.

## Cómo leer este documento

| Campo | Significado |
|---|---|
| **ID** | `CP-LOG-nn`. Estable: se referencia desde el código de prueba y desde los informes |
| **Nivel** | `Unitaria` sin dependencias, `Integración` contra SQL Server real, `Arranque` sobre el host |
| **Tarea** | Tarea de [tasks.md](../../../specs/001-auth-rbac-foundation/tasks.md) que lo implementa |
| **Técnica** | Equivalencia, límite o concurrencia. La que originó el caso |

**Control del tiempo.** Ningún caso espera tiempo real. Los que dependen de vencimientos sustituyen
el reloj por uno de prueba, de modo que "avanzar 15 minutos" es instantáneo y determinista. Un caso
que dependa del reloj del sistema mide la infraestructura, no el requisito.

**Nomenclatura de las pruebas.** El nombre del método describe el comportamiento esperado, no el
método bajo prueba: `Usuario_desactivado_no_puede_autenticarse_aunque_la_contrasena_sea_correcta`.
Cuando la prueba falla, su nombre debe bastar para saber qué regla se rompió.

---

## Bloque 1 · Reglas del usuario (nivel Unitaria, tarea T027)

Reglas R-U1 a R-U5 del [modelo de datos](../../../specs/001-auth-rbac-foundation/data-model.md).
Sin base de datos ni host: entidad de dominio pura.

### CP-LOG-01 · Un usuario activo sin bloqueo puede autenticarse

- **Técnica:** equivalencia, clase válida · **Requisito:** FR-001
- **Precondiciones:** existe un usuario con `ACTIVO = 1`, `BLOQUEADO_HASTA` nulo e
  `INTENTOS_FALLIDOS = 0`.
- **Datos:** usuario `jperez`, reloj de prueba en `2026-08-20T09:00:00Z`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Consultar si el usuario puede autenticarse en la hora del reloj | La verificación es correcta y no devuelve error |

### CP-LOG-02 · Un usuario desactivado no puede autenticarse aunque la contraseña sea correcta

- **Técnica:** equivalencia, clase inválida · **Requisito:** FR-005, regla R-U1
- **Precondiciones:** el usuario `mlopez` existe con `ACTIVO = 0`.
- **Datos:** contraseña correcta `Optica2026#`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Consultar si el usuario puede autenticarse | La verificación falla con el código de error `cuenta-inactiva` |

### CP-LOG-03 · La desactivación es lógica y conserva la fila

- **Técnica:** equivalencia · **Requisito:** FR-005, principio VII
- **Precondiciones:** el usuario `jperez` existe activo, asociado al empleado 1.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Desactivar el usuario | La propiedad de activo queda en falso |
| 2 | Leer el nombre de usuario y el identificador de empleado | Siguen siendo `jperez` y `1`; ninguno queda nulo ni vacío |

### CP-LOG-04 · Un usuario bloqueado no puede autenticarse aunque la contraseña sea correcta

- **Técnica:** equivalencia, clase inválida · **Requisito:** FR-019, regla R-U2
- **Precondiciones:** el usuario `rgomez` está bloqueado hasta `2026-08-20T09:15:00Z`.
- **Datos:** reloj de prueba en `2026-08-20T09:00:00Z`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Consultar si el usuario puede autenticarse | La verificación falla con el código de error `cuenta-bloqueada` |

### CP-LOG-05 · El bloqueo sigue vigente un segundo antes de cumplirse

- **Técnica:** **valor límite**, borde inferior · **Requisito:** FR-018
- **Precondiciones:** bloqueo hasta `2026-08-20T09:15:00Z`.
- **Datos:** reloj de prueba en `2026-08-20T09:14:59Z`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Consultar si el usuario puede autenticarse | La verificación falla con el código `cuenta-bloqueada` |

### CP-LOG-06 · El bloqueo se libera en el instante exacto del vencimiento

- **Técnica:** **valor límite**, borde exacto · **Requisito:** FR-018
- **Precondiciones:** bloqueo hasta `2026-08-20T09:15:00Z`.
- **Datos:** reloj de prueba en `2026-08-20T09:15:00Z`, exactamente.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Consultar si el usuario puede autenticarse | La verificación es correcta |

> Este par de casos, CP-LOG-05 y CP-LOG-06, es el que distingue un `>` de un `>=` en la comparación
> del vencimiento. Sin el segundo, una implementación que libera un minuto tarde pasa igual.

### CP-LOG-07 · El desbloqueo es implícito y no requiere limpiar el estado

- **Técnica:** valor límite, borde superior · **Requisito:** FR-018
- **Precondiciones:** bloqueo hasta `2026-08-20T09:15:00Z`.
- **Datos:** reloj de prueba en `2026-08-20T09:16:00Z`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Consultar si el usuario puede autenticarse | La verificación es correcta |
| 2 | Leer `BLOQUEADO_HASTA` | Conserva su valor: ningún proceso lo limpió |

### CP-LOG-08 · Un usuario desactivado y bloqueado a la vez se rechaza por inactivo

- **Técnica:** equivalencia, clases superpuestas · **Requisito:** FR-005, FR-019
- **Precondiciones:** el usuario tiene `ACTIVO = 0` y un bloqueo vigente.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Consultar si el usuario puede autenticarse | La verificación falla con el código `cuenta-inactiva`, no con `cuenta-bloqueada` |

> Fija la precedencia entre dos causas simultáneas. Sin este caso, el orden de evaluación queda
> librado al azar y el código de auditoría cambia de un día para otro.

### CP-LOG-09 · Un ingreso exitoso deja el contador de fallos en cero

- **Técnica:** equivalencia · **Requisito:** FR-020, regla R-U3
- **Precondiciones:** el usuario tiene un bloqueo vigente y fallos acumulados.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Registrar un ingreso exitoso en la entidad | `INTENTOS_FALLIDOS` queda en `0` |

### CP-LOG-10 · Un ingreso exitoso limpia el bloqueo

- **Técnica:** equivalencia · **Requisito:** FR-020, regla R-U3

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Registrar un ingreso exitoso en la entidad | `BLOQUEADO_HASTA` queda nulo |

### CP-LOG-11 · Las cuatro causas de rechazo comparten exactamente el mismo mensaje

- **Técnica:** equivalencia · **Requisito:** FR-004, regla R-U4
- **Precondiciones:** ninguna. Se inspeccionan los errores declarados.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Leer el mensaje de los errores de cuenta inexistente, credenciales incorrectas, cuenta inactiva y cuenta bloqueada | Los cuatro mensajes son idénticos: hay un solo valor distinto |

### CP-LOG-12 · Las cuatro causas se distinguen por código para el log y la traza

- **Técnica:** equivalencia · **Requisito:** FR-004, regla R-U4

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Leer el código de los mismos cuatro errores | Los cuatro códigos son distintos entre sí |

> CP-LOG-11 y CP-LOG-12 son el par que hace cumplible FR-004 sin perder trazabilidad: hacia fuera
> un solo mensaje, hacia dentro cuatro códigos. Probar solo uno de los dos deja pasar la
> implementación que iguala todo y vuelve el log inútil, o la que distingue todo y permite enumerar
> cuentas.

### CP-LOG-13 · El mensaje expuesto no revela la causa del rechazo

- **Técnica:** equivalencia, prueba parametrizada · **Requisito:** FR-004
- **Datos:** términos prohibidos `inexistente`, `no existe`, `inactiv`, `bloquead`, `desactivad`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Buscar cada término en el mensaje de credenciales incorrectas, sin distinguir mayúsculas | Ninguno aparece |

### CP-LOG-14 · La entidad no expone ninguna propiedad con la contraseña en claro

- **Técnica:** equivalencia · **Requisito:** FR-003, regla R-U5

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Listar por reflexión las propiedades públicas de la entidad | No existe ninguna llamada `Contrasena`, `ContrasenaEnClaro` ni `Password` |

### CP-LOG-15 · La representación en texto no incluye el hash ni la sal

- **Técnica:** equivalencia · **Requisito:** FR-003, principio VIII
- **Datos:** hash `[1,2,3,4]`, sal `[5,6,7,8]`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Convertir la entidad a texto | El resultado no contiene los bytes del hash ni de la sal, en ninguna codificación, incluida base 64 |

> Cubre la fuga más común: un registro de excepción que imprime el objeto completo.

### CP-LOG-16 · Un usuario nuevo nace activo, sin bloqueo y sin fallos

- **Técnica:** equivalencia · **Requisito:** modelo de datos

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Crear un usuario con empleado, nombre, hash y sal válidos | Queda activo, con `BLOQUEADO_HASTA` nulo e `INTENTOS_FALLIDOS` en `0` |

### CP-LOG-17 · No se crea un usuario sin nombre

- **Técnica:** equivalencia, clase inválida · **Requisito:** modelo de datos
- **Datos:** nombre de usuario vacío `""` y solo espacios `"   "`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Intentar crear el usuario con cada valor | La creación falla en ambos casos |

### CP-LOG-18 · Un nombre de usuario en los bordes de longitud se acepta

- **Técnica:** **valor límite** · **Requisito:** `NOMBRE_USUARIO nvarchar(100)`
- **Datos:** longitudes `1` y `100`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Crear el usuario con cada longitud | La creación es correcta en ambos casos |

### CP-LOG-19 · Un nombre más largo que la columna se rechaza

- **Técnica:** **valor límite**, borde superior · **Requisito:** `nvarchar(100)`
- **Datos:** longitud `101`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Intentar crear el usuario | La creación falla antes de llegar a la base de datos |

### CP-LOG-20 · No existe usuario sin empleado

- **Técnica:** equivalencia, clase inválida · **Requisito:** modelo de datos
- **Datos:** identificador de empleado `0`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Intentar crear el usuario | La creación falla |

**Fuera de alcance de este bloque, deliberadamente.** El incremento del contador de fallos y la
decisión de bloquear al llegar a cinco **no** se prueban en la entidad. La decisión D-06 los resuelve
en una sola sentencia `UPDATE` con `OUTPUT`, precisamente para que dos intentos simultáneos no
pierdan un incremento. Modelarlo en el dominio significaría leer y escribir por separado desde la
aplicación, que es lo que D-06 prohíbe. Su prueba está en el bloque 3, con FR-023.

---

## Bloque 2 · Handler del comando de ingreso (nivel Unitaria, tarea T028)

Con dobles de prueba para repositorio, hasheador, registrador de intentos, emisor de token y reloj.
Sin base de datos.

### CP-LOG-21 · Credenciales correctas sobre cuenta activa inician sesión

- **Técnica:** equivalencia, clase válida · **Requisito:** FR-001, FR-007 · **Gherkin:** escenario 1.1
- **Precondiciones:** el repositorio devuelve el usuario `jperez` activo, cuyo hash corresponde a
  `Optica2026#`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con `jperez` y `Optica2026#` | El resultado es correcto |
| 2 | Consultar el emisor de token | Emitió exactamente 1 par de credenciales |

### CP-LOG-22 · Contraseña incorrecta rechaza el ingreso y no emite token

- **Técnica:** equivalencia, clase inválida · **Requisito:** FR-004 · **Gherkin:** escenario 1.2

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con `jperez` y `ClaveMala#1` | El resultado es fallido |
| 2 | Consultar el emisor de token | Emitió 0 credenciales |

### CP-LOG-23 · Una cuenta desactivada no ingresa aunque la contraseña sea correcta

- **Técnica:** equivalencia, clase inválida · **Requisito:** FR-005 · **Gherkin:** escenario 1.4
- **Precondiciones:** el repositorio devuelve `mlopez` con `ACTIVO = 0`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con la contraseña correcta | El resultado es fallido |
| 2 | Consultar el emisor de token | Emitió 0 credenciales |

### CP-LOG-24 · Una cuenta bloqueada no ingresa aunque la contraseña sea correcta

- **Técnica:** equivalencia, clase inválida · **Requisito:** FR-019
- **Precondiciones:** `rgomez` bloqueado hasta 10 minutos en el futuro respecto al reloj de prueba.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con la contraseña correcta | El resultado es fallido |
| 2 | Consultar el emisor de token | Emitió 0 credenciales |

### CP-LOG-25 · Las cuatro causas de rechazo devuelven el mismo error

- **Técnica:** equivalencia, comparación entre clases · **Requisito:** FR-004
- **Precondiciones:** cuatro contextos: sin usuarios, usuario activo con clave incorrecta, usuario
  desactivado, usuario bloqueado.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando en los cuatro contextos y recoger los cuatro errores | Los cuatro comparten un único código y un único mensaje |

> Es el caso central del handler. Si alguna causa devuelve un error distinto, el sistema permite
> enumerar cuentas y SC-004 no se cumple, por más que el tiempo de respuesta esté igualado.

### CP-LOG-26 · Un usuario inexistente dispara la derivación señuelo

- **Técnica:** equivalencia · **Requisito:** SC-004, decisión D-02 · **Gherkin:** escenario 1.3
- **Precondiciones:** el repositorio no devuelve ningún usuario para `fantasma`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con `fantasma` | El hasheador registra exactamente 1 derivación señuelo |

### CP-LOG-27 · Un usuario existente no dispara la derivación señuelo

- **Técnica:** equivalencia, caso complementario · **Requisito:** decisión D-02

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con `jperez` y clave incorrecta | El hasheador registra 0 derivaciones señuelo |

> El complemento importa: sin él, una implementación que llama al señuelo siempre pasa CP-LOG-26 y
> duplica el costo de cada ingreso legítimo.
>
> Ninguno de los dos mide tiempo. Medir milisegundos en una prueba unitaria produce una prueba
> intermitente; la medición real es CP-LOG-33, de integración.

### CP-LOG-28 · Un ingreso exitoso se registra como exitoso

- **Técnica:** equivalencia · **Requisito:** FR-006, regla R-I1

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con credenciales correctas | El registrador recibe exactamente 1 intento |
| 2 | Leer ese intento | Tiene nombre de usuario `jperez` y marca de exitoso verdadera |

### CP-LOG-29 · Un intento fallido se registra como fallido

- **Técnica:** equivalencia · **Requisito:** FR-006, regla R-I1

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con clave incorrecta | El registrador recibe 1 intento con la marca de exitoso en falso |

### CP-LOG-30 · Un intento contra una cuenta inexistente se registra sin identificador

- **Técnica:** equivalencia · **Requisito:** FR-022, regla R-I2 · **Gherkin:** escenario 1.6

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con `fantasma` | El registrador recibe 1 intento |
| 2 | Leer ese intento | Nombre presentado `fantasma`, identificador de usuario **nulo**, exitoso en falso |

### CP-LOG-31 · El intento registra la dirección de origen y la hora del reloj inyectado

- **Técnica:** equivalencia · **Requisito:** FR-006, principio X
- **Datos:** dirección `192.168.1.40`, reloj de prueba en `2026-08-20T09:00:00Z`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando indicando la dirección de origen | El intento registra `192.168.1.40` |
| 2 | Leer la marca de tiempo del intento | Es exactamente la del reloj inyectado, con desplazamiento cero, es decir en UTC |

### CP-LOG-32 · La contraseña presentada no llega al registro de intentos

- **Técnica:** equivalencia · **Requisito:** regla R-I4, compuerta G8

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con clave incorrecta y concatenar todos los campos del intento registrado | El texto resultante no contiene la contraseña presentada |

### CP-LOG-33 · El token de cancelación se propaga al repositorio

- **Técnica:** equivalencia · **Requisito:** principio de rendimiento de la constitución

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar el comando con un token de cancelación propio | El repositorio recibió ese mismo token, no uno vacío |

---

## Bloque 3 · Hasheo de contraseñas (nivel Unitaria sobre infraestructura, tarea T034)

### CP-LOG-34 · Una contraseña derivada coincide consigo misma

- **Técnica:** equivalencia, prueba parametrizada · **Requisito:** FR-003, clase V3 del análisis
- **Datos**, todos dentro del rango de 8 a 12 caracteres que fija FR-003a: `Optica2026#`,
  `Ñandú Ó¢2026`, `con espacios`, `  bordes    ` con espacios al principio y al final,
  `Ω≈ç√∫˜µ≤`, `密码测试2026`, `emoji🔐clave`, cadena de 12 con salto de línea interno, cadena de
  12 con tabulación interna, y `12345678` como mínimo exacto.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar y verificar cada valor contra su propio hash y sal | Coincide en los diez casos |

> Es el caso que rompe cuando la columna, la conexión o el `sqlcmd` usan una codificación distinta
> a UTF-8. El repositorio ya tiene precedente: el `README.md` de `Scripts/SQL` documenta que
> `Optómetra` se guardó como `OptÃ³metra` en la primera carga.

### CP-LOG-35 · Una contraseña en los bordes de longitud coincide sin truncarse

- **Técnica:** **valor límite** · **Requisito:** FR-003a, rango de 8 a 12 caracteres
- **Datos:** longitudes `8` y `12`, los dos bordes del rango, con el último carácter distinto del
  relleno para que un truncamiento se note.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar y verificar cada longitud | Coincide en los cuatro casos |

### CP-LOG-36 · Dos contraseñas que solo difieren en el último carácter no coinciden

- **Técnica:** **valor límite**, detección de truncamiento · **Requisito:** FR-003
- **Datos:** 11 caracteres de relleno más `A`, frente a los mismos 11 más `B`. Ambas de 12, el
  tope del rango.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar la primera y verificar la segunda contra ese hash | No coincide |

> Sin este caso, un truncamiento silencioso en el tope del rango pasa inadvertido y dos
> contraseñas distintas abren la misma cuenta.

### CP-LOG-37 · La comparación distingue mayúsculas y minúsculas

- **Técnica:** equivalencia · **Requisito:** FR-003
- **Datos:** `Optica2026#` frente a `optica2026#segura`.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar la primera y verificar la segunda | No coincide |

### CP-LOG-38 · Los espacios al final son significativos y no se recortan

- **Técnica:** valor límite · **Requisito:** FR-003
- **Datos:** `Optica2026# ` con espacio final, frente a la misma sin él.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar la primera y verificar la segunda | No coincide |

### CP-LOG-39 · La derivación no normaliza formas Unicode equivalentes

- **Técnica:** exploratoria promovida a regresión · **Requisito:** FR-003
- **Datos:** `cafeñ` con la ñ como un punto de código, frente a `cafeñ` con n más tilde combinante.
  **Se escriben con secuencias de escape**, `ñ` y `ñ`: en el archivo se verían idénticas.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Comprobar que las dos cadenas son distintas en bytes pero iguales al normalizar | Se cumple; si no, el caso está mal construido |
| 2 | Derivar la primera y verificar la segunda | No coincide |

> Las dos posturas son defendibles: normalizar o no. Lo que no es defendible es que dependa del
> azar. Este caso fija la decisión de **no** normalizar, para que no cambie por accidente al tocar
> la codificación.

### CP-LOG-40 · El salt mide 128 bits y el hash 256 bits

- **Técnica:** equivalencia · **Requisito:** decisión D-01

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar una contraseña y medir la sal | Mide 16 bytes |
| 2 | Medir el hash | Mide 32 bytes |

### CP-LOG-41 · Dos derivaciones de la misma contraseña producen sal y hash distintos

- **Técnica:** equivalencia · **Requisito:** FR-003, sal por usuario

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar la misma contraseña dos veces y comparar las sales | Son distintas |
| 2 | Comparar los hashes | Son distintos |

### CP-LOG-42 · El hash no contiene la contraseña en claro

- **Técnica:** equivalencia · **Requisito:** FR-003, SC-007

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Derivar y buscar la contraseña en el hash interpretado como texto y como base 64 | No aparece en ninguna de las dos formas |

### CP-LOG-43 · La derivación señuelo cuesta lo mismo que una derivación real

- **Técnica:** valor límite temporal · **Requisito:** SC-004, decisión D-02
- **Precondiciones:** ejecutar antes una derivación de calentamiento, para no medir la carga inicial
  del entorno de ejecución.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Medir una derivación real y una señuelo | La diferencia se mantiene dentro del 50 % del costo de la real |

> La tolerancia es amplia a propósito. Una prueba de tiempo con margen estrecho falla de forma
> intermitente en una máquina cargada, y una prueba intermitente se acaba desactivando. La
> comprobación fina de los 100 milisegundos es CP-LOG-46, sobre el sistema completo.

---

## Bloque 4 · Ingreso de extremo a extremo (nivel Integración, tareas T030 a T032)

Contra SQL Server real y el host levantado. **Requieren el andamiaje de T026**, que aún no existe:
fábrica de host, base de datos de pruebas y limpieza de estado entre casos.

### CP-LOG-44 · Un ingreso exitoso responde 200 con cookies y registra el intento

- **Técnica:** equivalencia · **Requisito:** FR-001, FR-006 · **Gherkin:** escenarios 1.1 y 1.6
- **Precondiciones:** base de datos con el usuario `jperez` activo y rol `Vendedor`; tabla
  `LoginAttempts` vacía.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar `POST /api/auth/login` con credenciales correctas desde `192.168.1.40` | La respuesta tiene código 200 |
| 2 | Inspeccionar las cabeceras | Llegan las cookies `optica_at` y `optica_rt`, ambas con `HttpOnly`, `Secure` y `SameSite=Strict` |
| 3 | Inspeccionar el cuerpo | Contiene `roles` con `["Vendedor"]` y **ningún** valor de cookie |
| 4 | Consultar `LoginAttempts` | Contiene exactamente 1 fila, con `NOMBRE_USUARIO = 'jperez'`, `EXITOSO = 1`, `IP_ORIGEN = '192.168.1.40'` y `USUARIO_ID` igual al del usuario |

### CP-LOG-45 · Un intento contra una cuenta inexistente se registra sin crear la cuenta

- **Técnica:** equivalencia · **Requisito:** FR-022 · **Gherkin:** escenario 1.6

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar el ingreso con `fantasma` | La respuesta tiene código 401 |
| 2 | Consultar `LoginAttempts` | 1 fila con `NOMBRE_USUARIO = 'fantasma'`, `EXITOSO = 0` y `USUARIO_ID` nulo |
| 3 | Consultar `Usuarios` | No existe ninguna fila con ese nombre de usuario |

### CP-LOG-46 · El tiempo de respuesta no permite deducir si una cuenta existe

- **Técnica:** valor límite temporal · **Requisito:** SC-004 · **Gherkin:** escenario 1.3
- **Datos:** 50 peticiones con `fantasma` y 50 con `jperez`, todas con clave incorrecta.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar los dos grupos y calcular la mediana de cada uno | La diferencia entre medianas es menor a 100 milisegundos |
| 2 | Revisar los códigos de respuesta | Las 100 respuestas tienen código 401 |

> Se compara la **mediana**, no el promedio: un solo pico por recolección de basura o por
> planificación del sistema operativo desplaza el promedio y vuelve la prueba intermitente.

### CP-LOG-47 · El límite de longitud separa el 400 del 401

- **Técnica:** **valor límite** · **Requisito:** contrato, FR-003a, FR-004
- **Datos:** nombre de usuario de 0, 1, 100 y 101 caracteres; contraseña de 0, 1, 7, 8, 12 y 13.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar el ingreso con cada longitud de nombre de usuario | 0 y 101 responden 400; 1 y 100 responden 401 |
| 2 | Enviar el ingreso con contraseña de 0 caracteres | Responde 400: campo obligatorio |
| 3 | Enviar el ingreso con contraseña de 1 y de 7 caracteres | Ambas responden **401**, no 400 |
| 4 | Enviar el ingreso con contraseña de 8 y de 12 caracteres | Ambas responden 401 |
| 5 | Enviar el ingreso con contraseña de 13 caracteres | Responde 400 |

> El borde separa el 400 del 401, no el 400 del 200. Si un nombre de 100 caracteres respondiera 400,
> el sistema estaría rechazando entradas que la columna admite.
>
> El paso 3 es el más importante del caso, y el menos evidente. El mínimo de 8 que fija FR-003a
> **no** se valida en el ingreso: hacerlo convertiría una contraseña corta en un 400 distinguible
> del 401 de credencial incorrecta, lo que revela la política y abre una grieta en el mensaje
> genérico único de FR-004. Estas dos longitudes son las que atrapan a quien aplique el mínimo por
> simetría con el validador de creación.

### CP-LOG-48 · Cinco intentos fallidos simultáneos cuentan exactamente cinco

- **Técnica:** **concurrencia** · **Requisito:** FR-023, decisión D-06

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar 5 intentos fallidos en paralelo sobre `jperez` | `INTENTOS_FALLIDOS` queda exactamente en `5` |
| 2 | Leer `BLOQUEADO_HASTA` | Tiene valor: la cuenta quedó bloqueada |
| 3 | Consultar `LoginAttempts` | Contiene exactamente 5 filas para ese usuario |

### CP-LOG-49 · Cuatro intentos fallidos simultáneos no bloquean

- **Técnica:** **concurrencia**, valor límite · **Requisito:** FR-023

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar 4 intentos fallidos en paralelo | `INTENTOS_FALLIDOS` queda exactamente en `4` |
| 2 | Leer `BLOQUEADO_HASTA` | Queda nulo |

### CP-LOG-50 · Un ingreso correcto concurrente con uno fallido no deja el contador inconsistente

- **Técnica:** **concurrencia** · **Requisito:** FR-023
- **Precondiciones:** el usuario acumula 3 intentos fallidos.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar en paralelo un intento correcto y uno incorrecto | `INTENTOS_FALLIDOS` queda en `0` **o** en `1` |
| 2 | Leer `BLOQUEADO_HASTA` | Queda nulo |

> Admite dos resultados porque el orden de aplicación no está determinado. Lo que no admite es un
> tercero: que la cuenta quede bloqueada. Fijar un solo valor donde el sistema tiene dos legítimos
> produce una prueba intermitente.

### CP-LOG-51 · Una configuración con proveedor externo impide el arranque

- **Técnica:** equivalencia · **Requisito:** FR-002, compuerta G5 · **Gherkin:** escenario 1.5
- **Nivel:** Arranque · **Tarea:** T032

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Levantar el host con un proveedor de identidad externo declarado en la configuración | El arranque lanza excepción |
| 2 | Leer el mensaje de la excepción | Nombra la clave de configuración rechazada |
| 3 | Comprobar el estado del host | No quedó escuchando peticiones |

---

## Resumen y trazabilidad

| Bloque | Casos | Nivel | Tarea | Estado |
|---|---|---|---|---|
| 1 · Reglas del usuario | CP-LOG-01 a 20 | Unitaria | T027 | Listo para implementar |
| 2 · Handler de ingreso | CP-LOG-21 a 33 | Unitaria | T028 | Listo para implementar |
| 3 · Hasheo | CP-LOG-34 a 43 | Unitaria | T029 | Listo para implementar |
| 4 · Extremo a extremo | CP-LOG-44 a 51 | Integración y arranque | T030 a T032 | **Bloqueado por T026** |

**Requisitos de US1 cubiertos:** FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, y FR-019, FR-020,
FR-022 y FR-023 en lo que toca al ingreso. Los de sesión, bloqueo y roles se cubren en los
documentos de sus propias historias.

**Orden de implementación recomendado.** Bloques 1 a 3 primero: no necesitan base de datos, corren
en milisegundos y son los que más presión ejercen sobre el diseño. El bloque 4 después de T026,
porque sin la fábrica de host y la limpieza de estado entre casos, esos ocho casos se contaminan
entre sí.

## Corrección pendiente en tasks.md

La tarea T029 sitúa la prueba del hasheo en `tests/Optica.Domain.Tests/Seguridad/`, y ahí no puede
vivir: el hasheador se implementa en `Optica.Infrastructure` según T034, y `Optica.Domain.Tests`
solo referencia `Optica.Domain`, como exige el principio II. Ponerla en esa ruta obligaría a agregar
una referencia que viola la regla de dependencias y haría fallar la compuerta G1.

Los casos del bloque 3 corresponden por tanto a un proyecto que todavía no existe,
`Optica.Infrastructure.Tests`. La alternativa es ubicarlos en `Optica.Integration.Tests`, que es el
único proyecto de prueba que hoy referencia infraestructura, aunque no sean pruebas de integración
en sentido estricto: no tocan base de datos ni host.
