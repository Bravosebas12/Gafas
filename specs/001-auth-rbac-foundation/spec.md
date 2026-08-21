# Feature Specification: Fundación de Solución y Autenticación con Control de Acceso

**Feature Branch**: `001-auth-rbac-foundation`

**Created**: 2026-08-20

**Status**: Draft

**Input**: User description: "Fundación de solución y módulo de Autenticación y Control de Acceso (RF-LOG-01 a RF-LOG-04)"

**Historias fuente**: [RF-LOG-01](../../HU/Historias_Tecnicas/RF-LOG-01_Autenticacion_Nativa_SQL.md), [RF-LOG-02](../../HU/Historias_Tecnicas/RF-LOG-02_JWT_Refresh_Tokens.md), [RF-LOG-03](../../HU/Historias_Tecnicas/RF-LOG-03_Bloqueo_Cuenta.md), [RF-LOG-04](../../HU/Historias_Tecnicas/RF-LOG-04_Roles_RBAC.md)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ingreso al sistema con credenciales propias (Priority: P1)

Un empleado de la óptica abre la aplicación, escribe su nombre de usuario y contraseña, y obtiene acceso al sistema. Las credenciales se validan contra la base de datos propia de la óptica; no existe ninguna opción de ingresar con cuentas de Google, Microsoft, Facebook u otro proveedor externo. Cuando las credenciales no son correctas, el sistema responde con un mensaje genérico que no permite deducir si el usuario existe.

**Why this priority**: Sin ingreso al sistema ninguna otra funcionalidad de la óptica es alcanzable. Es la raíz de dependencias de los siete módulos y la única historia marcada como crítica en la segmentación original.

**Independent Test**: Se prueba completamente creando un usuario en la base de datos e intentando ingresar con credenciales correctas, incorrectas e inexistentes. Entrega valor por sí sola: habilita el acceso controlado a la aplicación.

**Acceptance Scenarios**:

1. **Given** un usuario activo existe en la base de datos propia, **When** ingresa nombre de usuario y contraseña correctos, **Then** el sistema lo autentica y le concede acceso.
2. **Given** un usuario activo existe, **When** ingresa una contraseña incorrecta, **Then** el sistema rechaza el ingreso con un mensaje genérico que no distingue entre usuario inexistente y contraseña equivocada.
3. **Given** el nombre de usuario no existe, **When** se intenta ingresar, **Then** el sistema responde con el mismo mensaje genérico y en el mismo rango de tiempo que en el caso de contraseña incorrecta.
4. **Given** un usuario fue desactivado lógicamente, **When** intenta ingresar con credenciales válidas, **Then** el sistema rechaza el acceso.
5. **Given** la configuración del sistema declara un proveedor de identidad externo, **When** el sistema arranca, **Then** la configuración es rechazada y el arranque falla con un error explícito en lugar de habilitar el proveedor.
6. **Given** cualquier intento de ingreso, exitoso o fallido, **When** se procesa, **Then** queda registrado con marca de tiempo, identificador presentado y origen de la petición.

---

### User Story 2 - Sesión protegida con renovación y cierre (Priority: P1)

Tras ingresar, el empleado trabaja durante toda su jornada sin volver a escribir la contraseña. Su sesión se renueva sola en segundo plano. Si su sesión es robada o él cierra sesión, la credencial de renovación deja de servir de inmediato.

**Why this priority**: Sin renovación, la sesión corta obligaría a reescribir la contraseña cada pocos minutos, lo que hace inusable el punto de venta. Va junto a la historia 1 porque el ingreso sin manejo de sesión no es operable.

**Independent Test**: Se prueba autenticándose, esperando el vencimiento del token de acceso, renovando con la credencial de renovación y verificando que la credencial anterior queda inservible tras la rotación.

**Acceptance Scenarios**:

1. **Given** un ingreso exitoso, **When** el sistema responde, **Then** entrega un token de acceso de corta duración y una credencial de renovación de mayor duración.
2. **Given** el token de acceso venció, **When** el cliente presenta una credencial de renovación válida y vigente, **Then** el sistema emite un nuevo par de credenciales.
3. **Given** una credencial de renovación vencida, revocada o perteneciente a otro usuario, **When** se intenta renovar, **Then** el sistema rechaza la renovación.
4. **Given** una credencial de renovación fue rotada, **When** se reutiliza la credencial anterior, **Then** el sistema la rechaza y revoca toda la cadena de credenciales activas de ese usuario.
5. **Given** un usuario con sesión activa, **When** cierra sesión, **Then** sus credenciales de renovación quedan revocadas y no permiten emitir nuevos tokens.
6. **Given** una credencial de renovación almacenada, **When** se inspecciona la base de datos, **Then** no aparece el valor en claro, solo su representación hasheada.
7. **Given** un token de acceso emitido, **When** se inspecciona su contenido, **Then** contiene identificador de usuario y roles, y ningún dato personal o sensible innecesario.

---

### User Story 3 - Bloqueo automático ante fuerza bruta (Priority: P2)

Cuando alguien intenta adivinar la contraseña de una cuenta, el sistema la bloquea temporalmente tras cinco intentos fallidos consecutivos y la libera sola pasados quince minutos. El mensaje mostrado no revela que la cuenta está bloqueada ni que existe.

**Why this priority**: Es una defensa exigida por la especificación de seguridad, pero el ingreso y la sesión funcionan sin ella. Se implementa inmediatamente después porque expone la autenticación a fuerza bruta mientras no exista.

**Independent Test**: Se prueba emitiendo intentos fallidos sucesivos contra una cuenta y verificando el bloqueo al quinto, el rechazo con credenciales correctas durante la ventana, y el desbloqueo automático al vencer.

**Acceptance Scenarios**:

1. **Given** una cuenta con cuatro intentos fallidos consecutivos dentro de la ventana de quince minutos, **When** falla un quinto intento, **Then** la cuenta queda bloqueada y el bloqueo se registra en auditoría.
2. **Given** una cuenta bloqueada, **When** se presenta la contraseña correcta antes de que venzan los quince minutos, **Then** el sistema rechaza el ingreso.
3. **Given** una cuenta bloqueada, **When** transcurren los quince minutos, **Then** el usuario puede volver a intentar y el contador de fallas queda en cero.
4. **Given** una cuenta con intentos fallidos por debajo del límite, **When** el usuario ingresa correctamente, **Then** el contador de fallas consecutivas se reinicia.
5. **Given** una cuenta bloqueada, **When** se responde al cliente, **Then** el mensaje es indistinguible del de credenciales incorrectas.
6. **Given** intentos fallidos contra un nombre de usuario inexistente, **When** se procesan, **Then** quedan registrados sin crear ni bloquear ninguna cuenta.

---

### User Story 4 - Roles estrictos validados en el servidor (Priority: P2)

El administrador asigna a cada empleado uno o varios roles entre Administrador, Vendedor y Optómetra. Cada operación del sistema verifica en el servidor que el rol del solicitante la permita; ocultar un botón en la pantalla nunca es suficiente. Solo el Administrador puede asignar o quitar roles, y cada cambio queda auditado.

**Why this priority**: Condiciona todos los endpoints de los módulos siguientes, así que debe existir antes de inventario y ventas. Va después del bloqueo porque el sistema es operable —aunque sin segregación de funciones— mientras solo existan administradores.

**Independent Test**: Se prueba asignando cada rol a un usuario distinto e invocando directamente las operaciones protegidas, sin pasar por la interfaz, para confirmar que el servidor acepta o rechaza según el rol.

**Acceptance Scenarios**:

1. **Given** un usuario con rol Administrador, **When** invoca cualquier operación del sistema, **Then** el servidor la autoriza.
2. **Given** un usuario con rol Vendedor, **When** invoca operaciones de inventario y ventas autorizadas para su rol, **Then** el servidor las autoriza.
3. **Given** un usuario con rol Optómetra, **When** invoca operaciones de fórmulas y recomendaciones autorizadas para su rol, **Then** el servidor las autoriza.
4. **Given** un usuario sin el rol requerido, **When** invoca la operación directamente contra el servidor omitiendo la interfaz, **Then** el servidor rechaza la acción por falta de permiso.
5. **Given** un usuario que no es Administrador, **When** intenta asignar o quitar un rol a cualquier usuario, **Then** el sistema rechaza la operación.
6. **Given** un Administrador, **When** asigna o quita un rol, **Then** el cambio se persiste y queda auditado con usuario responsable, marca de tiempo y valor anterior y posterior.
7. **Given** un usuario con dos roles asignados, **When** invoca una operación permitida a cualquiera de los dos, **Then** el servidor la autoriza.
8. **Given** un rol fuera del conjunto Administrador, Vendedor y Optómetra, **When** se intenta asignar, **Then** el sistema rechaza la asignación.

---

### User Story 5 - Base técnica verificable del proyecto (Priority: P3)

El equipo dispone de una solución compilable con las capas definidas en la especificación técnica, la base de datos creada a partir del script existente, y una medición automática de cobertura de pruebas que falla cuando cae por debajo del umbral de la constitución.

**Why this priority**: Es habilitador, no funcional: nadie lo percibe como valor de negocio, pero sin él el "Definition of Done" de la constitución no es verificable. Se especifica aquí porque las historias P1 y P2 son los primeros casos de uso que lo ejercitan.

**Independent Test**: Se prueba clonando el repositorio, compilando, aplicando el esquema de base de datos y ejecutando la suite de pruebas con reporte de cobertura en una máquina limpia.

**Acceptance Scenarios**:

1. **Given** un clon limpio del repositorio, **When** se compila la solución, **Then** compila sin errores ni advertencias.
2. **Given** la solución, **When** se revisa su estructura, **Then** respeta la separación de capas de la especificación técnica y las dependencias apuntan hacia el dominio.
3. **Given** una base de datos vacía, **When** se aplica el esquema, **Then** quedan creadas las tablas del modelo de datos existente, incluidos usuarios, roles, credenciales de renovación e intentos de ingreso.
4. **Given** la suite de pruebas, **When** se ejecuta con medición de cobertura, **Then** reporta al menos 85% global y al menos 90% en las capas de dominio y aplicación, y falla el proceso si no se alcanza.
5. **Given** el arranque del sistema, **When** no existe ningún usuario, **Then** se dispone de un mecanismo controlado para crear el primer Administrador sin credenciales embebidas en el código.

---

### Edge Cases

- Dos ingresos simultáneos con la misma contraseña incorrecta sobre la misma cuenta: el contador de fallas no debe contar de menos ni bloquear antes del quinto intento.
- Uso simultáneo de la misma credencial de renovación desde dos dispositivos: solo una rotación debe prosperar; la otra debe rechazarse y revocar la cadena.
- Un usuario es desactivado o pierde un rol mientras tiene un token de acceso vigente: el servidor sigue autorizándolo hasta que el token venza, con un máximo de 15 minutos, y sus credenciales de renovación quedan revocadas de inmediato para que no pueda prolongar la sesión (FR-032, FR-032a).
- Un usuario queda sin ningún rol asignado: debe poder ingresar pero ninguna operación protegida debe autorizarle.
- El último Administrador del sistema intenta quitarse su propio rol: el sistema debe impedir quedarse sin ningún Administrador.
- Contraseña con caracteres no latinos, espacios o longitud extrema: debe validarse y almacenarse sin corrupción ni truncamiento.
- Desfase de reloj entre el servidor de aplicación y la base de datos: los vencimientos de bloqueo y de credenciales deben calcularse sobre una única fuente de tiempo en UTC.
- Petición sin credencial, con credencial malformada o con firma inválida: debe rechazarse sin filtrar detalles del error de validación.
- Un empleado sin usuario asociado, o un usuario cuyo empleado fue dado de baja: debe definirse el efecto sobre el ingreso.

## Requirements *(mandatory)*

### Functional Requirements

**Autenticación (RF-LOG-01)**

- **FR-001**: El sistema DEBE validar las credenciales de ingreso exclusivamente contra su propia base de datos SQL.
- **FR-002**: El sistema DEBE rechazar cualquier configuración que habilite un proveedor de identidad externo, fallando el arranque con un error explícito.
- **FR-003**: El sistema DEBE almacenar las contraseñas con función de hash segura y salt por usuario, y nunca en claro ni con cifrado reversible.
- **FR-004**: El sistema DEBE responder con un mensaje genérico e idéntico ante usuario inexistente, contraseña incorrecta, cuenta desactivada y cuenta bloqueada.
- **FR-005**: El sistema DEBE impedir el ingreso a usuarios desactivados lógicamente.
- **FR-006**: El sistema DEBE registrar todo intento de ingreso, exitoso o fallido, con identificador presentado, resultado, marca de tiempo en UTC y dirección de origen.

**Sesión (RF-LOG-02)**

- **FR-007**: El sistema DEBE emitir, tras un ingreso exitoso, un token de acceso de corta duración junto con una credencial de renovación.
- **FR-008**: El token de acceso DEBE expirar 15 minutos después de su emisión.
- **FR-009**: La credencial de renovación DEBE expirar 8 horas después de su emisión, de modo que cubra una jornada laboral completa.
- **FR-010**: El sistema DEBE almacenar las credenciales de renovación hasheadas, asociadas a un usuario, con fecha de vencimiento y fecha de revocación.
- **FR-011**: El sistema DEBE rotar la credencial de renovación en cada uso, invalidando la anterior.
- **FR-012**: El sistema DEBE rechazar la renovación cuando la credencial esté vencida, revocada, ya rotada o no pertenezca al usuario que la presenta.
- **FR-013**: El sistema DEBE revocar todas las credenciales de renovación activas de un usuario cuando se detecte la reutilización de una credencial ya rotada.
- **FR-014**: Los usuarios DEBEN poder cerrar sesión, lo que revoca sus credenciales de renovación.
- **FR-015**: El token de acceso DEBE contener únicamente identificador de usuario, roles y metadatos de vigencia; ningún dato personal ni sensible.
- **FR-016**: El sistema DEBE rechazar tokens de acceso con firma inválida, vencidos o malformados sin revelar el motivo del rechazo.

**Bloqueo de cuenta (RF-LOG-03)**

- **FR-017**: El sistema DEBE bloquear una cuenta al acumular 5 intentos fallidos consecutivos dentro de una ventana de 15 minutos.
- **FR-018**: El bloqueo DEBE durar 15 minutos y liberarse automáticamente al vencer, sin intervención administrativa.
- **FR-019**: El sistema DEBE rechazar el ingreso durante el bloqueo incluso con credenciales correctas.
- **FR-020**: El sistema DEBE reiniciar el contador de fallas consecutivas tras un ingreso exitoso y tras vencer el bloqueo.
- **FR-021**: El sistema DEBE registrar cada bloqueo en auditoría con la cuenta afectada y la marca de tiempo.
- **FR-022**: El sistema DEBE procesar intentos fallidos contra nombres de usuario inexistentes sin crear registros de cuenta y sin permitir inferir su ausencia.
- **FR-023**: El conteo de intentos fallidos DEBE ser correcto ante intentos concurrentes sobre la misma cuenta.

**Control de acceso (RF-LOG-04)**

- **FR-024**: El sistema DEBE reconocer exactamente tres roles: Administrador, Vendedor y Optómetra, y rechazar cualquier otro.
- **FR-025**: Un usuario DEBE poder tener uno o varios roles simultáneamente, y ser autorizado si cualquiera de ellos permite la operación.
- **FR-026**: El sistema DEBE validar la autorización de cada operación en el servidor, independientemente de lo que muestre u oculte la interfaz.
- **FR-027**: Solo los usuarios con rol Administrador DEBEN poder asignar o quitar roles.
- **FR-028**: El sistema DEBE auditar cada cambio de roles con usuario responsable, usuario afectado, valor anterior, valor posterior y marca de tiempo.
- **FR-029**: Los roles DEBEN estar vinculados al perfil de empleado del usuario.
- **FR-030**: El sistema DEBE impedir que quede sin ningún usuario con rol Administrador activo.
- **FR-031**: Un usuario sin roles DEBE poder ingresar, pero ninguna operación protegida DEBE autorizarle.
- **FR-032**: Al desactivar un usuario o modificar sus roles, el cambio DEBE surtir efecto sobre las autorizaciones a más tardar cuando venza su token de acceso vigente, es decir dentro de una ventana máxima de 15 minutos. No se exige verificación de estado en cada petición.
- **FR-032a**: Al desactivar un usuario o modificar sus roles, el sistema DEBE revocar en el acto todas sus credenciales de renovación activas, de modo que la ventana de 15 minutos no pueda extenderse mediante una renovación.

**Base técnica**

- **FR-033**: La solución DEBE organizarse en las capas de la especificación técnica, con las dependencias apuntando hacia el dominio.
- **FR-034**: El esquema de base de datos DEBE derivarse del modelo de datos existente en `Scripts/SQL/001_modelo_datos_optica.sql`, sin redefinir tablas ya modeladas.
- **FR-035**: El sistema DEBE ofrecer un mecanismo controlado de creación del primer Administrador que no requiera credenciales embebidas en el código fuente.
- **FR-036**: La medición de cobertura DEBE ejecutarse automáticamente y fallar el proceso por debajo de 85% global o 90% en dominio y aplicación.
- **FR-037**: Toda operación de ingreso, renovación, bloqueo y cambio de roles DEBE emitir registro estructurado con identificador de correlación.

### Key Entities

- **Empleado**: Persona que trabaja en la óptica. Nombre y apellido. Un empleado tiene como máximo un usuario.
- **Usuario**: Cuenta de acceso de un empleado. Nombre de usuario único, hash y salt de contraseña, indicador de activo, contador de intentos fallidos y momento hasta el cual está bloqueado.
- **Rol**: Perfil de permisos. Código único y nombre. Conjunto cerrado: Administrador, Vendedor, Optómetra.
- **UsuarioRol**: Asignación de un rol a un usuario. Relación de muchos a muchos.
- **CredencialDeRenovación**: Credencial hasheada asociada a un usuario, con vencimiento y marca de revocación, que permite emitir nuevos tokens de acceso.
- **IntentoDeIngreso**: Registro inmutable de cada intento, con usuario cuando se identificó, nombre presentado, resultado, marca de tiempo y dirección de origen.
- **RegistroDeAuditoría**: Traza inalterable de cambios sensibles sobre usuarios y roles, con valor anterior y posterior, usuario responsable y marca de tiempo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Un empleado con credenciales válidas completa el ingreso en menos de 3 segundos desde el envío del formulario hasta ver la pantalla principal.
- **SC-002**: Un empleado trabaja una jornada completa de 8 horas de uso continuo sin volver a escribir su contraseña.
- **SC-003**: Cinco intentos fallidos consecutivos bloquean la cuenta el 100% de las veces, y ningún sexto intento con credenciales correctas prospera dentro de la ventana de 15 minutos.
- **SC-004**: La diferencia de tiempo de respuesta entre un intento con usuario inexistente y uno con contraseña incorrecta se mantiene por debajo de 100 milisegundos, de modo que no permita enumerar cuentas.
- **SC-005**: El 100% de las operaciones protegidas rechaza a un solicitante sin el rol requerido cuando se invoca directamente contra el servidor, sin pasar por la interfaz.
- **SC-006**: Ninguna credencial de renovación reutilizada tras su rotación consigue emitir un token de acceso, en el 100% de los intentos.
- **SC-007**: Una inspección de la base de datos no revela ninguna contraseña ni credencial de renovación en claro.
- **SC-008**: El 100% de los ingresos, bloqueos y cambios de rol queda reconstruible desde los registros de auditoría e intentos.
- **SC-009**: La cobertura de pruebas alcanza al menos 85% global y 90% en dominio y aplicación, verificada por el proceso automático.
- **SC-010**: El sistema no expone ninguna vía de ingreso mediante proveedores de identidad externos, verificado por prueba automática.

## Assumptions

- El ecosistema técnico está fijado por [ESPECIFICACION_TECNICA.md](../../HU/Historias_Tecnicas/ESPECIFICACION_TECNICA.md): .NET 10, Blazor WebAssembly con MudBlazor, Clean Architecture, CQRS con MediatR y SQL Server propio. No se reevalúa en esta feature.
- El modelo de datos de [001_modelo_datos_optica.sql](../../Scripts/SQL/001_modelo_datos_optica.sql) se toma como fuente de verdad; esta feature usa las tablas de `ADMINISTRACION_USUARIOS` y `AUDITORIA` ya definidas y no altera su diseño.
- **Decidido 2026-08-20:** la duración del token de acceso es de 15 minutos y la de la credencial de renovación de 8 horas (FR-008, FR-009). El token corto limita la ventana de abuso ante una filtración; las 8 horas cubren la jornada sin reescribir contraseña, requisito de usabilidad del punto de venta.
- **Decidido 2026-08-20:** los cambios de estado y de roles surten efecto al vencer el token de acceso, no mediante verificación en cada petición (FR-032). Se aceptó una ventana máxima de exposición de 15 minutos a cambio de no agregar una consulta a base de datos por petición, dado que el punto de venta es sensible a la latencia. La revocación inmediata de credenciales de renovación (FR-032a) impide que esa ventana se extienda.
- El alcance de operaciones autorizadas por rol Vendedor y Optómetra se define en detalle en las features de sus módulos; aquí solo se establece el mecanismo de validación y un mapa inicial.
- La administración completa de usuarios (creación, edición, desactivación lógica) pertenece a la feature del módulo RF-USR; esta feature solo cubre la creación controlada del primer Administrador.
- **Decidido 2026-08-20:** el cambio de contraseña por autogestión y la política de complejidad (RF-CFG-02, RF-USR-02) quedan **fuera** de esta feature; aquí solo se exige el almacenamiento seguro con hash y salt. Se evaluó incluirlos, como propone la Fase 1 de [ORDEN-IMPLEMENTACION-HU.md](../../docs/ORDEN-IMPLEMENTACION-HU.md), y se decidió excluirlos para no arrastrar la pantalla `/configuracion-perfil` del módulo de configuración personal a esta feature. Consecuencia asumida: la Fase 1 de ese documento se cierra con cuatro de sus cinco historias y RF-CFG-02 se reubica en la feature del módulo RF-CFG/RF-USR.
- El prototipo estático descrito en [PLAN-MAQUETACION.md](../../docs/PLAN-MAQUETACION.md) se considera material de referencia visual, no un entregable de esta feature.
- La pantalla de ingreso es la única interfaz incluida; el resto de pantallas llegan con sus módulos.
- El sistema opera en una sola sede con red local o internet estable; no se contempla operación sin conexión.
- Todas las marcas de tiempo se manejan en UTC, alineadas con los valores por defecto del modelo de datos.

## Dependencies

- No hay features previas: esta es la primera del proyecto y todas las demás dependen de ella para autenticación y autorización.
- Requiere una instancia de SQL Server accesible para desarrollo y para el proceso de pruebas automáticas.
- Las features de módulos posteriores (RF-INV, RF-CLI, RF-VNT, RF-DSH, RF-USR, RF-CFG) consumen el mecanismo de autorización definido aquí.
