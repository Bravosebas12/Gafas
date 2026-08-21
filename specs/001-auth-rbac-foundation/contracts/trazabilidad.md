# Trazabilidad — Escenarios de aceptación ↔ pruebas

**Feature**: 001-auth-rbac-foundation
**Sostiene la compuerta G4**: "cada criterio de aceptación tiene prueba correspondiente".

Los 32 escenarios de aceptación numerados de [spec.md](../spec.md) se listan aquí con el tipo de
prueba que los cubre y el proyecto donde vive, seguidos de los 9 casos borde de la especificación,
que se marcan como tales. El principio IV exige además que las pruebas se nombren por el
comportamiento esperado, no por el método que ejercitan.

**Tipos**: `Dominio` = `Optica.Domain.Tests` · `Aplicación` = `Optica.Application.Tests` ·
`Integración` = `Optica.Integration.Tests` (SQL Server real) · `Arquitectura` =
`Optica.Architecture.Tests` · `Navegador` = `Optica.E2E.Tests` (Playwright con NUnit, decisión
D-13)

---

## US1 — Ingreso al sistema con credenciales propias (6 escenarios)

| # | Escenario | Tipo | Nota |
|---|---|---|---|
| 1.1 | Credenciales correctas conceden acceso | Integración | Contra usuario real en base |
| 1.2 | Contraseña incorrecta produce mensaje genérico | Aplicación | Se verifica el tipo de error, no solo el texto |
| 1.3 | Usuario inexistente responde igual y en el mismo rango de tiempo | Integración | Mide el tiempo de ambos caminos y comprueba SC-004; el señuelo PBKDF2 es lo que hace pasar esta prueba |
| 1.4 | Usuario desactivado no puede ingresar con credenciales válidas | Aplicación | Regla R-U1 |
| 1.5 | Proveedor de identidad externo declarado impide el arranque | Integración | Arranca el host con configuración inválida y espera excepción; cubre G5 |
| 1.6 | Todo intento queda registrado con hora, identificador y origen | Integración | Verifica la fila en `LoginAttempts` |

---

## US2 — Sesión protegida con renovación y cierre (7 escenarios)

| # | Escenario | Tipo | Nota |
|---|---|---|---|
| 2.1 | El ingreso entrega token de acceso y credencial de renovación | Integración | Comprueba ambas cookies y sus atributos `HttpOnly`, `Secure`, `SameSite` |
| 2.2 | Token vencido más credencial válida emiten un par nuevo | Aplicación | Con `FakeTimeProvider`, sin esperas reales |
| 2.3 | Credencial vencida, revocada o de otro usuario es rechazada | Aplicación | Tres casos, regla R-C2 |
| 2.4 | Reutilizar una credencial rotada revoca toda la cadena | Integración | Cubre R-C3 y valida la decisión D-05 |
| 2.5 | El cierre de sesión revoca las credenciales de renovación | Integración | |
| 2.6 | La credencial almacenada no aparece en claro en la base | Integración | Consulta directa a `RefreshTokens`; cubre SC-007 |
| 2.7 | El token contiene identificador y roles, y nada sensible | Aplicación | Inspecciona los claims emitidos; falla si aparece cualquier campo no previsto |

---

## US3 — Bloqueo automático ante fuerza bruta (6 escenarios)

| # | Escenario | Tipo | Nota |
|---|---|---|---|
| 3.1 | El quinto fallo consecutivo bloquea y se audita | Integración | Verifica la fila en `AUDITORIA.LogUsuarios` |
| 3.2 | Contraseña correcta durante el bloqueo es rechazada | Aplicación | Regla R-U2 |
| 3.3 | Pasados 15 minutos se puede reintentar y el contador queda en cero | Aplicación | `FakeTimeProvider`; el desbloqueo es implícito, sin proceso programado |
| 3.4 | Un ingreso correcto reinicia el contador de fallos | Aplicación | Regla R-U3 |
| 3.5 | El mensaje de cuenta bloqueada es indistinguible del de credenciales incorrectas | Aplicación | Regla R-U4 |
| 3.6 | Fallos contra un usuario inexistente no crean ni bloquean cuenta | Integración | `USUARIO_ID` nulo en `LoginAttempts`, regla R-I2 |
| 3.7 | **Caso borde**: N intentos fallidos concurrentes cuentan exactamente N | Integración | Exigido explícitamente por FR-023 y por el principio IV, que obliga a probar la concurrencia declarada. Valida D-06 |

El escenario 3.7 no está numerado en la especificación: proviene de la sección de casos borde. Se
incluye porque el principio IV lo hace obligatorio.

---

## US4 — Roles estrictos validados en el servidor (8 escenarios)

| # | Escenario | Tipo | Nota |
|---|---|---|---|
| 4.1 | Administrador queda autorizado en las operaciones de esta feature | Integración | El alcance por módulo se completa en sus features |
| 4.2 | Vendedor queda autorizado donde su rol lo permite | Integración | Con las políticas definidas hasta ahora |
| 4.3 | Optómetra queda autorizado donde su rol lo permite | Integración | Atención al código sin tilde |
| 4.4 | Sin el rol requerido, la operación invocada directamente contra el servidor se rechaza | Integración | Cubre SC-005 y la compuerta G6; no pasa por la interfaz |
| 4.5 | Un no administrador no puede asignar ni quitar roles | Integración | FR-027 |
| 4.6 | El cambio de roles se persiste y se audita con antes y después | Integración | Incluye una prueba de rollback que verifica que la auditoría se revierte con la operación (G7) |
| 4.7 | Dos roles asignados autorizan lo permitido a cualquiera de los dos | Aplicación | Regla R-UR1 |
| 4.8 | Un rol fuera del conjunto cerrado se rechaza | Aplicación | Validador del comando, antes de tocar la base |
| 4.9 | **Caso borde**: el último administrador no puede quitarse su propio rol | Integración | Regla R-UR3, evaluada sobre el estado final dentro de la transacción |
| 4.10 | **Caso borde**: un usuario sin roles ingresa pero no autoriza ninguna operación protegida | Integración | Regla R-UR4, FR-031 |

Los escenarios 4.9 y 4.10 provienen de la sección de casos borde de la especificación.

---

## US5 — Base técnica verificable (5 escenarios)

| # | Escenario | Tipo | Nota |
|---|---|---|---|
| 5.1 | Un clon limpio compila sin errores ni advertencias | Compilación | Advertencias como errores en `Directory.Build.props` |
| 5.2 | La estructura respeta las capas y las dependencias apuntan al dominio | Arquitectura | Un caso por fila de la tabla del principio II; cubre G1 |
| 5.3 | El esquema se aplica sobre una base vacía | Integración | Ejecuta los scripts `000`, `001` y `002` y verifica la existencia de las tablas |
| 5.4 | La cobertura alcanza los umbrales y falla el proceso si no | Compuerta | coverlet con umbral por capa; cubre G3 |
| 5.5 | Existe un mecanismo para crear el primer Administrador sin credenciales en el código | Integración | Ejercita el comando de D-12 |

---

## Casos borde de la especificación sin escenario numerado

Todos exigen prueba. Se listan aparte para que ninguno se pierda al generar las tareas.

| Caso borde | Tipo | Cubierto en |
|---|---|---|
| Fallos concurrentes sobre la misma cuenta | Integración | 3.7 |
| Misma credencial de renovación desde dos dispositivos | Integración | 2.4 |
| Desactivación o cambio de roles con token vigente | Integración | Prueba propia: verifica que el token sigue sirviendo hasta vencer y que las credenciales de renovación ya no (FR-032, FR-032a) |
| Usuario sin ningún rol | Integración | 4.10 |
| Último administrador se quita el rol | Integración | 4.9 |
| Contraseña con caracteres no latinos, espacios o longitud extrema | Dominio | Prueba propia: ida y vuelta de hash sin truncamiento ni corrupción |
| Desfase de reloj entre aplicación y base de datos | Integración | Prueba propia: los vencimientos se calculan en la aplicación, no mezclando `SYSUTCDATETIME()` con la hora del proceso (D-07) |
| Petición sin credencial, malformada o con firma inválida | Integración | Prueba propia: tres casos, todos 401 sin detalle (FR-016) |
| Empleado sin usuario, o usuario cuyo empleado fue dado de baja | Dominio | Prueba propia. **Atención**: `Empleados` no tiene columna de baja lógica en el esquema, así que "empleado dado de baja" no es representable hoy. Se prueba lo que sí existe —usuario inactivo— y se anota como punto a resolver en la feature RF-USR, que es la dueña de la gestión de empleados |

---

## Nivel navegador — pantalla de ingreso (decisión D-13)

Segunda capa sobre la pantalla de ingreso, en `Optica.E2E.Tests` con Playwright y NUnit. Los casos
están especificados en
[qa/001-auth-rbac-foundation/casos-de-prueba/login-e2e-playwright.md](../../../qa/001-auth-rbac-foundation/casos-de-prueba/login-e2e-playwright.md).

**No alteran el total de 41 pruebas previstas**, y conviene entender por qué: la mayoría son una
segunda capa sobre escenarios que ya tienen prueba por debajo, y los que aportan cobertura nueva
—SC-001 y la compuerta G9— no son escenarios de aceptación numerados, así que no entran en el
conteo que sostiene G4.

| Caso | Cubre | Aporte propio |
|---|---|---|
| CP-E2E-01 | Escenario 1.1 | Segunda capa: el recorrido real del navegador |
| CP-E2E-02 | **SC-001** | **Sí.** Único que mide el recorrido completo bajo 3 segundos |
| CP-E2E-03 a 05 | **Compuerta G9** | **Sí.** Los cuatro estados de la vista |
| CP-E2E-06 | Escenarios 1.2, 1.4 y 3.5 | **Sí.** Verifica que las cuatro causas se ven idénticas en pantalla, no solo que el handler devuelve el mismo error |
| CP-E2E-07, 08, 15 | **Compuerta G9** | **Sí.** Lector de pantalla, teclado con foco visible y contraste |
| CP-E2E-09 | Principio IX | **Sí.** Usable a 360 píxeles |
| CP-E2E-10 | Escenario 2.1, FR-015 | **Sí.** Que el navegador **respete** `HttpOnly`, no solo que la cabecera lo declare |
| CP-E2E-11 | Escenario 1.5, SC-010 | Segunda capa: la pantalla no ofrece proveedores externos |
| CP-E2E-12 | Escenarios 3.1 y 3.2 | Segunda capa: el bloqueo provocado desde la interfaz |
| CP-E2E-13 | Escenario 4.10 | Segunda capa: usuario sin roles |
| CP-E2E-14 | FR-003, regla R-U5 | **Sí.** La contraseña no queda en el documento ni en la dirección |

**Fuera de este nivel, a propósito.** Los vencimientos de 15 minutos y 8 horas siguen en
`Integración`: Playwright no puede mover el reloj del servidor. Y la igualación de tiempos de
SC-004 se mide a nivel HTTP, porque el render del navegador introduce más varianza que los 100
milisegundos que se quieren medir.

---

## Resumen de cobertura de escenarios

| Historia | Escenarios numerados | Casos borde añadidos | Total de pruebas previstas |
|---|---|---|---|
| US1 | 6 | 1 | 7 |
| US2 | 7 | 1 | 8 |
| US3 | 6 | 1 | 7 |
| US4 | 8 | 2 | 10 |
| US5 | 5 | 0 | 5 |
| Transversales | — | 4 | 4 |
| **Total** | **32** | **9** | **41** |

Ningún escenario de aceptación queda sin prueba. La compuerta G4 está satisfecha en el plan; su
verificación real ocurre al ejecutar las tareas.
