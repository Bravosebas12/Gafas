# Casos de prueba de extremo a extremo con Playwright — Pantalla de ingreso

**Feature:** 001-auth-rbac-foundation · **Historia:** US1 · **Pantalla:** `/login`
**Implementa:** tarea T041 · **Compuertas:** G9, y cierre de SC-001
**Complementa:** [login.md](login.md), que especifica los casos de unidad e integración

---

## 1. Análisis de alcance: qué corresponde a Playwright y qué no

Antes de los casos, la parte que importa. Playwright conduce un navegador real, así que su valor
está en lo que **solo se puede observar desde el navegador**. Usarlo para lo demás no agrega
cobertura: agrega minutos de ejecución y probabilidad de fallo intermitente.

### La feature 001 tiene una sola pantalla

El plan lo dice de forma explícita en la compuerta G9: *"aplica a la única pantalla de la
feature"*. Todo lo demás de esta historia son endpoints de servidor. Eso acota el alcance de
Playwright a la pantalla de ingreso, y hace que este documento sea corto por diseño, no por
descuido.

### Lo que Playwright verifica y ninguna otra prueba puede

| Qué | Por qué solo el navegador lo demuestra | Requisito |
|---|---|---|
| Tiempo del recorrido completo hasta ver la pantalla principal | Incluye render, red y latencia del navegador. Medirlo en el servidor mide otra cosa | **SC-001** |
| Los cuatro estados de la vista | Cargando, vacío, error y con datos son estados de la interfaz, no de la respuesta HTTP | Principio IX, **G9** |
| Recorrido por teclado y foco visible | Requiere un motor de foco real | Principio IX, **G9** |
| Que `HttpOnly` funcione de verdad | Solo se prueba intentando leer la cookie desde JavaScript y comprobando que no aparece | FR-015, D-04 |
| Doble envío del formulario | Necesita eventos de interfaz reales | Hallazgo de la carta E-06 |
| Ausencia de proveedores externos en la pantalla | El DOM renderizado es la evidencia | FR-002, **SC-010** |
| Comportamiento a 360 píxeles de ancho | Requiere un viewport | Principio IX |

Las dos primeras filas son las que cierran los **únicos dos huecos** que
[trazabilidad.md](../trazabilidad.md) registra: SC-001 sin cubrir, y G9 apoyado solo en una carta
de exploración manual. Ese es el argumento fuerte para incorporar Playwright a esta feature.

### Lo que NO se pasa a Playwright, y por qué

Los bloques 1 a 3 de [login.md](login.md) —41 casos de reglas del usuario, handler y hasheo— se
quedan donde están. No es una preferencia de estilo:

- **No son observables desde el navegador.** Ningún clic demuestra que la sal mide 128 bits, que
  las cuatro causas de rechazo comparten código de error interno distinto, o que el bloqueo se
  libera en el milisegundo exacto del vencimiento.
- **Romperían los umbrales por capa.** El principio IV exige ≥90 % en dominio y aplicación, medido
  por capa. La cobertura que produce un navegador atravesando la pila entera no es atribuible a una
  capa, así que no ayuda a cumplir ese umbral y además lo distorsiona si se mezcla en el mismo
  informe.
- **Coste desproporcionado.** Cada caso de navegador cuesta segundos; su equivalente unitario,
  milisegundos. Conducir 41 aserciones de unidad por la interfaz convierte una suite de 200 ms en
  una de varios minutos, y la suite que tarda es la que se deja de ejecutar.

### Dos cosas que Playwright tampoco puede hacer aquí

**No puede manipular el reloj.** Todos los casos temporales de esta feature —vencimiento del
bloqueo a los 15 minutos, del token de acceso a los 15, de la sesión a las 8 horas— dependen de
sustituir el reloj del servidor. Un navegador no alcanza esa abstracción: cambiar la hora del
cliente no mueve el reloj del servidor. Esos casos **se quedan en las pruebas de integración**, que
sí pueden inyectar el reloj. Lo que sí es verificable por navegador es **provocar** el bloqueo con
cinco envíos, porque eso no requiere avanzar el tiempo.

**No debe medir la igualación de tiempos de SC-004.** Los 100 milisegundos de diferencia entre
cuenta existente e inexistente se miden a nivel HTTP, donde el ruido es el del servidor. Añadir
render y pintado del navegador mete varianza mayor que la magnitud que se quiere medir. Ese caso es
CP-LOG-46, de integración.

---

## 2. Requisitos de infraestructura

Estos casos no corren sin lo siguiente, y nada de ello existe todavía:

| Requisito | Detalle | Depende de |
|---|---|---|
| Aplicación levantada | La pantalla de ingreso implementada y el host escuchando | T041 |
| HTTPS | Las cookies llevan `Secure`; sobre HTTP el navegador las descarta y todos los casos de sesión fallan por un motivo que no es el que se prueba | T007 |
| Base de datos con datos conocidos | Las cuentas de la tabla de datos de abajo, sembradas antes de la corrida | T026, D-11 |
| Estado limpio entre casos | El contador de intentos fallidos y el bloqueo persisten en la fila del usuario: un caso que bloquea `bloqueo1` contamina al siguiente | T026 |
| Proyecto de prueba propio | Separado de los cuatro existentes, y **excluido del informe de cobertura por capa** | tarea nueva |

### Datos de prueba sembrados

Todas las contraseñas respetan el rango de 8 a 12 caracteres de FR-003a.

| Usuario | Contraseña | Estado | Rol |
|---|---|---|---|
| `jperez` | `Optica2026#` | Activa | Vendedor |
| `mlopez` | `Optica2026#` | `ACTIVO = 0` | Vendedor |
| `bloqueo1` | `Optica2026#` | Activa, para provocar el bloqueo | Vendedor |
| `sinrol1` | `Optica2026#` | Activa, sin roles | — |
| `fantasma` | — | **No existe** | — |

---

## 3. Casos

Misma anatomía que [login.md](login.md): identificador estable, técnica, requisito,
precondiciones, datos y una tabla de pasos con un resultado observable por paso.

### CP-E2E-01 · El ingreso correcto lleva a la pantalla principal

- **Técnica:** equivalencia, camino feliz · **Requisito:** FR-001 · **Etiqueta:** `@smoke`
- **Precondiciones:** navegador en `/login`, sin sesión previa.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Escribir `jperez` en el campo de usuario y `Optica2026#` en el de contraseña | Ambos campos muestran el valor escrito; el de contraseña lo muestra enmascarado |
| 2 | Pulsar el botón de ingresar | La dirección del navegador deja de ser `/login` y la pantalla principal queda visible |

### CP-E2E-02 · El recorrido completo del ingreso tarda menos de 3 segundos

- **Técnica:** rendimiento de extremo a extremo · **Requisito:** **SC-001** · **Etiqueta:** `@rendimiento`
- **Precondiciones:** aplicación ya calentada con al menos una petición previa, para no medir la
  compilación en primer uso.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Marcar el instante del envío del formulario con credenciales correctas | — |
| 2 | Esperar a que la pantalla principal sea visible y marcar el instante | El intervalo entre ambas marcas es menor a 3.000 milisegundos |
| 3 | Repetir la medición 5 veces y tomar la mediana | La mediana es menor a 3.000 milisegundos |

> Se mide la mediana de cinco corridas, no una sola. Con una única medición, un pico de la máquina
> convierte el caso en intermitente, y una prueba intermitente se acaba desactivando. Este caso
> cierra el hueco que `trazabilidad.md` registra como el único criterio de éxito sin cubrir.

### CP-E2E-03 · Estado vacío: foco inicial en el campo de usuario y sin errores

- **Técnica:** estados de vista · **Requisito:** principio IX, **G9** · **Etiqueta:** `@a11y`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Abrir `/login` | El campo de usuario tiene el foco |
| 2 | Inspeccionar la pantalla | No hay ningún mensaje de error visible |

### CP-E2E-04 · Estado de carga: el botón se deshabilita y no admite segundo envío

- **Técnica:** estados de vista · **Requisito:** principio IX, **G9**
- **Precondiciones:** la respuesta del servidor se retarda de forma controlada para poder observar
  el estado intermedio.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar el formulario con credenciales correctas | El botón de ingresar queda deshabilitado y aparece el indicador de progreso |
| 2 | Intentar pulsar el botón otra vez mientras el indicador está visible | No se envía una segunda petición: el servidor recibió exactamente 1 |

### CP-E2E-05 · Estado de error: mensaje genérico con ícono además del color

- **Técnica:** estados de vista, accesibilidad · **Requisito:** FR-004, principio IX, **G9**

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar el formulario con `jperez` y `ClaveMala#1` | Aparece el mensaje exactamente igual a "Usuario o contraseña incorrectos" |
| 2 | Inspeccionar el bloque del mensaje | Contiene un ícono, además del color: la información no depende solo del color |
| 3 | Comprobar la dirección del navegador | Sigue en `/login` |

### CP-E2E-06 · Las cuatro causas de rechazo se ven idénticas en la pantalla

- **Técnica:** equivalencia entre clases · **Requisito:** FR-004 · **Etiqueta:** `@seguridad`
- **Datos:** `fantasma` inexistente, `jperez` con clave errada, `mlopez` inactiva, `bloqueo1`
  bloqueada tras cinco fallos.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar el formulario en los cuatro casos y capturar el texto visible del mensaje | Los cuatro textos son idénticos entre sí |
| 2 | Comparar el texto completo de la región de error, incluidos atributos accesibles | No hay ninguna diferencia observable entre los cuatro |

> Es el mismo requisito que CP-LOG-25 verifica en el handler, pero aquí se comprueba lo que **el
> usuario ve**. Un handler correcto con una pantalla que añade "cuenta bloqueada, intente más
> tarde" filtra la información igual, y solo esta capa lo detecta.

### CP-E2E-07 · El mensaje de error se anuncia a un lector de pantalla

- **Técnica:** accesibilidad · **Requisito:** principio IX, **G9** · **Etiqueta:** `@a11y`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar credenciales incorrectas | La región del mensaje está marcada como región activa de anuncio |
| 2 | Inspeccionar el campo que causó el fallo | Está asociado al mensaje de error mediante su relación accesible y marcado como inválido |

### CP-E2E-08 · El formulario se completa y envía solo con teclado

- **Técnica:** accesibilidad · **Requisito:** principio IX, **G9** · **Etiqueta:** `@a11y`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Desde el campo de usuario, tabular una vez | El foco pasa al campo de contraseña |
| 2 | Tabular otra vez | El foco pasa al control de mostrar u ocultar contraseña |
| 3 | Tabular otra vez | El foco pasa al botón de ingresar |
| 4 | Comprobar cada uno de los cuatro elementos anteriores | Todos muestran indicación visible de foco |
| 5 | Escribir credenciales correctas y pulsar Enter sin usar el ratón | La pantalla principal queda visible |

> El orden de los pasos 1 a 3 es el que fija el plan. Si la implementación coloca el control de
> mostrar contraseña fuera del recorrido, o después del botón, este caso lo detecta.

### CP-E2E-09 · La pantalla es usable a 360 píxeles de ancho

- **Técnica:** diseño adaptable · **Requisito:** principio IX · **Etiqueta:** `@a11y`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Fijar el viewport en 360 por 640 y abrir `/login` | El ancho del contenido del documento no supera 360 píxeles: no hay desplazamiento horizontal |
| 2 | Ingresar con credenciales correctas | La pantalla principal queda visible |

### CP-E2E-10 · La cookie de sesión no es legible desde JavaScript

- **Técnica:** seguridad · **Requisito:** FR-015, decisión D-04 · **Etiqueta:** `@seguridad`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ingresar correctamente | La sesión queda establecida |
| 2 | Leer las cookies del documento desde el contexto de la página | El resultado no contiene `optica_at` ni `optica_rt` |
| 3 | Leer las cookies desde el contexto del navegador, fuera de la página | Ambas están presentes, con `HttpOnly` en verdadero, `Secure` en verdadero y `SameSite` en `Strict` |

> El paso 2 es el que da valor a este caso. Que la cabecera `Set-Cookie` declare `HttpOnly` lo
> comprueba CP-LOG-44 a nivel HTTP; que el navegador **lo respete** solo se demuestra intentando
> leerla y no encontrándola.

### CP-E2E-11 · La pantalla no ofrece ninguna vía de ingreso con proveedor externo

- **Técnica:** equivalencia · **Requisito:** FR-002, **SC-010** · **Etiqueta:** `@seguridad`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Abrir `/login` y volcar el contenido renderizado | No aparece ningún control ni enlace de Google, Microsoft, Facebook, Apple ni otro proveedor de identidad |
| 2 | Enumerar los destinos de todos los enlaces y botones de la pantalla | Ninguno apunta a un dominio externo |

### CP-E2E-12 · Cinco envíos fallidos bloquean la cuenta desde la interfaz

- **Técnica:** valor límite · **Requisito:** FR-017, FR-019 · **Etiqueta:** `@seguridad`
- **Precondiciones:** `bloqueo1` con el contador de fallos en 0 y sin bloqueo.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Enviar el formulario cuatro veces con contraseña incorrecta | Las cuatro veces aparece el mensaje genérico |
| 2 | Enviar por quinta vez con contraseña incorrecta | Aparece el mismo mensaje genérico, sin indicación de bloqueo |
| 3 | Enviar con la contraseña **correcta** | El ingreso es rechazado y aparece el mismo mensaje genérico |

> El paso 3 es el que demuestra el bloqueo desde la interfaz, y usa la contraseña correcta a
> propósito: con una incorrecta el rechazo sería indistinguible del normal y el caso no probaría
> nada. El vencimiento del bloqueo a los 15 minutos **no** se prueba aquí: requiere mover el reloj
> del servidor, y eso es CP-LOG del bloque de integración.

### CP-E2E-13 · Un usuario sin roles ingresa pero no ve opciones que no puede usar

- **Técnica:** equivalencia · **Requisito:** FR-031, principio IX

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Ingresar como `sinrol1` con su contraseña correcta | El ingreso es correcto y la pantalla principal queda visible |
| 2 | Inspeccionar la navegación visible | No se ofrece ninguna opción de administración |

> Ocultar la opción es cortesía hacia el usuario, no control de acceso. Que el servidor la rechace
> igualmente lo cubren los casos de autorización, invocando el endpoint sin pasar por la interfaz.

### CP-E2E-14 · La contraseña no queda expuesta en la pantalla ni en el historial

- **Técnica:** seguridad · **Requisito:** FR-003, regla R-U5 · **Etiqueta:** `@seguridad`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Escribir la contraseña y volcar el contenido renderizado | El valor en claro no aparece en el documento |
| 2 | Usar el control de mostrar contraseña y volver a ocultarla | Tras ocultarla, el campo vuelve a estar enmascarado |
| 3 | Comprobar la dirección del navegador tras el envío | No contiene la contraseña como parámetro |

### CP-E2E-15 · Los colores provienen de los tokens y el contraste cumple el mínimo

- **Técnica:** accesibilidad · **Requisito:** principio IX, **G9** · **Etiqueta:** `@a11y`

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | Leer el color de texto y de fondo del mensaje de error tal como los aplica el navegador | La relación de contraste entre ambos es de al menos 4.5 a 1 |
| 2 | Leer el color del botón principal y de su texto | La relación de contraste es de al menos 4.5 a 1 |
| 3 | Comprobar que los valores computados corresponden a los tokens definidos | Ningún color aparece como valor literal ajeno a la paleta de `docs/PLAN-MAQUETACION.md` |

---

## 4. Decisiones que hay que tomar antes de implementar

Estas no las puedo resolver yo, porque cambian el andamiaje del proyecto.

**Cómo se conduce Playwright desde .NET.** La suite del proyecto usa xUnit. Playwright para .NET
publica integraciones oficiales para NUnit y MSTest; para xUnit hay que verificar la disponibilidad
del paquete de integración en la versión vigente, y si no existe, escribir el arranque y cierre del
navegador a mano en un `fixture` de xUnit, que es unas veinte líneas. **Verificar antes de decidir**:
no doy por hecho que exista un paquete oficial para xUnit.

**Dónde viven estos casos.** Recomiendo un proyecto nuevo, `tests/Optica.E2E.Tests/`, por tres
razones: los umbrales de cobertura por capa se miden por proyecto de prueba y este no debe entrar
en ese cálculo; requiere navegadores instalados, lo que no debe condicionar a quien solo quiere
correr las pruebas de dominio; y su duración pide una etiqueta propia para excluirlo de la
ejecución rápida.

**Cuándo se ejecuta.** Estos quince casos tardan minutos, no milisegundos. Lo razonable es que la
compilación local corra los cuatro proyectos actuales y que la suite de navegador se ejecute en la
integración continua y bajo demanda.

**Qué se guarda cuando un caso falla.** Playwright puede conservar captura de pantalla, video y
traza. Sin eso, un fallo en integración continua es irreproducible y termina en "en mi máquina
pasa".

---

## 5. Trazabilidad

| Caso | Requisito | Cierra un hueco documentado |
|---|---|---|
| CP-E2E-01 | FR-001 | — |
| CP-E2E-02 | **SC-001** | **Sí:** único criterio de éxito sin cubrir en `trazabilidad.md` |
| CP-E2E-03, 04, 05 | Principio IX, G9 | **Sí:** los cuatro estados dejan de depender solo de la carta E-06 |
| CP-E2E-06 | FR-004 | — |
| CP-E2E-07, 08 | Principio IX, G9 | **Sí:** teclado y lector de pantalla, antes solo exploratorios |
| CP-E2E-09 | Principio IX | **Sí:** los 360 píxeles, antes solo exploratorios |
| CP-E2E-10 | FR-015, D-04 | — |
| CP-E2E-11 | FR-002, SC-010 | — |
| CP-E2E-12 | FR-017, FR-019 | — |
| CP-E2E-13 | FR-031 | — |
| CP-E2E-14 | FR-003 | — |
| CP-E2E-15 | Principio IX, G9 | **Sí:** contraste, antes solo exploratorio |

**Efecto sobre la trazabilidad de la feature.** Con estos casos, SC-001 pasa de *sin cubrir* a
cubierto, y la compuerta G9 pasa de apoyarse en una carta de exploración manual a tener nueve casos
automatizados. La carta E-06 sigue siendo útil y **no** se elimina: las herramientas automáticas
detectan una fracción de los problemas reales de teclado y lector de pantalla, y el resto necesita
a alguien recorriendo la pantalla.
