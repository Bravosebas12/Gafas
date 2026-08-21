# Phase 1 — Modelo de datos

**Feature**: 001-auth-rbac-foundation
**Fuente de verdad**: [Scripts/SQL/001_modelo_datos_optica.sql](../../Scripts/SQL/001_modelo_datos_optica.sql)

El principio X de la constitución declara ese script como fuente de verdad. Este documento **no
propone esquema**: mapea las siete entidades de la especificación a tablas que ya existen y declara
dónde vive cada regla. Toda columna citada aquí fue verificada contra el script.

**Conclusión anticipada**: esta feature no requiere ningún cambio de esquema. La compuerta G10
queda en verde.

---

## 1. Empleado → `ADMINISTRACION_USUARIOS.Empleados`

| Campo de dominio | Columna | Tipo | Nota |
|---|---|---|---|
| Id | `ID` | `bigint` identidad | |
| Nombre | `NOMBRE` | `nvarchar(100)` | Obligatorio |
| Apellido | `APELLIDO` | `nvarchar(100)` | Obligatorio |

Auditoría de fila: `USUARIO_CREACION`, `FECHA_CREACION`, `USUARIO_ACTUALIZACION`,
`FECHA_ACTUALIZACION`, con valor por defecto `SYSUTCDATETIME()`. Las pobla el interceptor.

**Relación**: un empleado tiene **como máximo un** usuario. La restricción está en la base:
`UQ_Usuarios_Empleado` sobre `Usuarios.EMPLEADO_ID`. No se replica la validación en la aplicación
como única defensa; se traduce el error de unicidad a un fallo de negocio legible.

**Alcance en esta feature**: solo lectura, más la creación del empleado del primer Administrador
(FR-035). La gestión de empleados es de la feature RF-USR.

---

## 2. Usuario → `ADMINISTRACION_USUARIOS.Usuarios`

| Campo de dominio | Columna | Tipo | Regla |
|---|---|---|---|
| Id | `ID` | `bigint` identidad | |
| EmpleadoId | `EMPLEADO_ID` | `bigint` no nulo | Clave foránea a `Empleados`; **obligatoria**, no existe usuario sin empleado |
| NombreUsuario | `NOMBRE_USUARIO` | `nvarchar(100)` | Único por `UQ_Usuarios_NOMBRE_USUARIO`. Se compara sin distinguir mayúsculas, según la colación de la base |
| HashContraseña | `PASSWORD_HASH` | `varbinary(256)` | Derivación PBKDF2-HMAC-SHA256, 256 bits (D-01) |
| SalContraseña | `PASSWORD_SALT` | `varbinary(128)` | Aleatorio de 128 bits, distinto por usuario |
| Activo | `ACTIVO` | `bit`, por defecto 1 | La baja es **lógica**, nunca física (principio VII) |
| BloqueadoHasta | `BLOQUEADO_HASTA` | `datetime2(7)` nulo | UTC. Nulo significa no bloqueado |
| IntentosFallidos | `INTENTOS_FALLIDOS` | `int`, por defecto 0 | Fallos **consecutivos**, no históricos |

### Reglas de dominio

- **R-U1** (FR-005): un usuario con `ACTIVO = 0` no puede autenticarse, aunque presente la
  contraseña correcta.
- **R-U2** (FR-017, FR-019): un usuario con `BLOQUEADO_HASTA` en el futuro no puede autenticarse,
  aunque presente la contraseña correcta.
- **R-U3** (FR-020): un ingreso exitoso deja `INTENTOS_FALLIDOS` en 0 y `BLOQUEADO_HASTA` en nulo.
- **R-U4** (FR-004): el rechazo por R-U1, por R-U2, por contraseña incorrecta y por usuario
  inexistente produce **el mismo mensaje** hacia el exterior. La distinción existe solo en el log
  y en la traza de intentos.
- **R-U5** (FR-003, principio VI): la contraseña nunca se persiste, ni se registra en log, ni se
  devuelve en ninguna respuesta.

### Transiciones de estado

```text
Activo sin bloqueo
  ├── fallo de credencial → INTENTOS_FALLIDOS + 1
  │     └── al llegar a 5 dentro de la ventana de 15 min → BLOQUEADO_HASTA = ahora + 15 min
  ├── ingreso correcto → INTENTOS_FALLIDOS = 0, BLOQUEADO_HASTA = null
  └── desactivación administrativa → ACTIVO = 0 + revocación de credenciales (FR-032a)

Bloqueado
  ├── intento durante la ventana, correcto o no → rechazado, mensaje genérico
  └── vencida la ventana → vuelve a Activo sin bloqueo, contador en 0
```

El vencimiento del bloqueo es **implícito**: no hay proceso que limpie `BLOQUEADO_HASTA`. Se
evalúa comparando contra la hora actual en cada intento, lo que evita depender de un trabajo
programado y hace que el desbloqueo automático de FR-018 sea inherente.

### Concurrencia (FR-023)

El incremento del contador y la decisión del bloqueo se resuelven en **una sola sentencia
`UPDATE`** con `OUTPUT`, nunca leyendo y escribiendo por separado desde la aplicación (D-06). Es la
única forma de que dos intentos simultáneos no pierdan un incremento.

---

## 3. Rol → `ADMINISTRACION_USUARIOS.Roles`

| Campo | Columna | Nota |
|---|---|---|
| Id | `ID` | |
| Codigo | `CODIGO` | `nvarchar(30)`, único por `UQ_Roles_CODIGO` |
| Nombre | `NOMBRE` | `nvarchar(100)` |

**Conjunto cerrado** (FR-024), ya sembrado por
[002_seed_catalogos_merge.sql](../../Scripts/SQL/002_seed_catalogos_merge.sql):

| `CODIGO` | `NOMBRE` |
|---|---|
| `Administrador` | Administrador |
| `Vendedor` | Vendedor |
| `Optometra` | Optómetra |

Atención al detalle: el código es `Optometra` **sin tilde** y el nombre visible `Optómetra`
**con tilde**. El código es lo que viaja en el token y en las políticas de autorización; la tilde
solo aparece en la interfaz. Confundirlos rompe la autorización de forma silenciosa.

- **R-R1** (FR-024): un código fuera de esos tres se rechaza en el validador del comando, antes de
  tocar la base.
- **R-R2**: la tabla no tiene columna `ACTIVO` y el seed no borra roles sobrantes, de modo que los
  roles no se desactivan ni se eliminan. Esta feature **no crea ni modifica roles**; solo los lee y
  los asigna.

---

## 4. UsuarioRol → `ADMINISTRACION_USUARIOS.UsuariosRoles`

Clave primaria compuesta `(USUARIO_ID, ROL_ID)`, con claves foráneas a ambas tablas. La clave
compuesta ya impide la asignación duplicada, sin validación adicional en la aplicación.

- **R-UR1** (FR-025): un usuario puede tener varios roles y se autoriza si **cualquiera** de ellos
  permite la operación.
- **R-UR2** (FR-027): solo un Administrador asigna o quita roles.
- **R-UR3** (FR-030): no puede quedar el sistema sin ningún usuario **activo** con rol
  Administrador. Se verifica dentro de la misma transacción del retiro del rol, contando los
  administradores activos restantes; si el resultado es cero, la operación se revierte. Cubre el
  caso borde del último administrador que intenta quitarse su propio rol.
- **R-UR4** (FR-031): un usuario sin ninguna fila aquí puede autenticarse, pero ninguna política de
  autorización lo admite.
- **R-UR5** (FR-028): cada asignación y retiro se audita con valor anterior y posterior.

---

## 5. CredencialDeRenovación → `ADMINISTRACION_USUARIOS.RefreshTokens`

| Campo | Columna | Regla |
|---|---|---|
| Id | `ID` | |
| UsuarioId | `USUARIO_ID` | Clave foránea a `Usuarios` |
| HashDelToken | `TOKEN_HASH` | `varbinary(256)`. Se persiste **solo el hash** (FR-010, SC-007) |
| VenceEn | `EXPIRES_AT` | UTC. Emisión + 8 horas (FR-009) |
| RevocadaEn | `REVOKED_AT` | Nulo mientras está vigente |

El valor en claro es aleatorio de 256 bits y **solo existe en la cookie del navegador**. Al no ser
un token con significado, su hash puede ser SHA-256 directo: no requiere derivación lenta como una
contraseña, porque no hay espacio de búsqueda que un atacante pueda recorrer.

### Reglas

- **R-C1** (FR-011): cada uso rota la credencial. La fila consumida recibe `REVOKED_AT` y se
  inserta una nueva, todo en la misma transacción.
- **R-C2** (FR-012): se rechaza la renovación si el hash no existe, si `EXPIRES_AT` ya pasó, si
  `REVOKED_AT` no es nulo, o si `USUARIO_ID` no corresponde al portador.
- **R-C3** (FR-013): encontrar el hash **con `REVOKED_AT` poblado** significa reutilización de una
  credencial ya consumida. Se revocan **todas** las credenciales activas de ese usuario. Ver D-05:
  esta es la razón por la que no se necesita una columna de encadenamiento.
- **R-C4** (FR-014): el cierre de sesión revoca las credenciales activas del usuario.
- **R-C5** (FR-032a): desactivar un usuario o cambiarle los roles revoca **en el acto** sus
  credenciales activas, para que la ventana de 15 minutos del token de acceso no pueda extenderse.

### Caso borde: dos dispositivos, la misma credencial

Solo una rotación debe prosperar. Se resuelve porque el `UPDATE` que marca `REVOKED_AT` se
condiciona a que siga siendo nula (`WHERE REVOKED_AT IS NULL`) y se comprueba el número de filas
afectadas: la segunda rotación afecta cero filas, se trata como R-C3 y revoca la cadena.

**Nota operativa**: la tabla acumula una fila por rotación, del orden de 32 filas por usuario y
jornada. No hay purga en esta feature; conviene un mantenimiento programado antes de que el volumen
importe. No es una regla de la especificación, es deuda anotada.

---

## 6. IntentoDeIngreso → `ADMINISTRACION_USUARIOS.LoginAttempts`

| Campo | Columna | Nota |
|---|---|---|
| Id | `ID` | |
| UsuarioId | `USUARIO_ID` | **Nulo** cuando el nombre presentado no corresponde a ningún usuario |
| NombrePresentado | `NOMBRE_USUARIO` | El texto que llegó, exista o no la cuenta |
| Exitoso | `EXITOSO` | `bit` |
| FechaIntento | `FECHA_INTENTO` | UTC |
| IpOrigen | `IP_ORIGEN` | `nvarchar(45)`, alcanza para IPv6 |

- **R-I1** (FR-006): se registra **todo** intento, exitoso o fallido.
- **R-I2** (FR-022): un intento contra un usuario inexistente se registra con `USUARIO_ID` nulo, sin
  crear ninguna cuenta. Que la columna admita nulos es precisamente lo que lo permite.
- **R-I3** (principio VII): tabla de **solo inserción**. Nunca se actualiza ni se borra.
- **R-I4** (principio VIII): nunca se guarda la contraseña presentada, en ninguna columna.

---

## 7. RegistroDeAuditoría → `AUDITORIA.LogUsuarios`

| Campo | Columna | Nota |
|---|---|---|
| Id | `ID` | |
| TablaOrigen | `TABLA_ORIGEN` | `nvarchar(128)` |
| RegistroId | `RegistroID` | `bigint`, identificador de la fila afectada |
| Accion | `ACCION` | `nvarchar(30)` |
| DatosAntes | `DATOS_ANTES` | `nvarchar(max)`, JSON del estado previo |
| DatosDespues | `DATOS_DESPUES` | `nvarchar(max)`, JSON del estado posterior |
| UsuarioAccion | `USUARIO_ACCION` | Quién ejecutó; nulo para acciones del sistema |
| FechaAccion | `FECHA_ACCION` | UTC |

### Eventos que esta feature audita

| Acción | Tabla origen | Requisito |
|---|---|---|
| `BLOQUEO_CUENTA` | `Usuarios` | FR-021 |
| `ASIGNAR_ROL` | `UsuariosRoles` | FR-028 |
| `QUITAR_ROL` | `UsuariosRoles` | FR-028 |
| `REVOCACION_CADENA` | `RefreshTokens` | FR-013, por reutilización detectada |
| `CREAR_PRIMER_ADMIN` | `Usuarios` | FR-035, con `USUARIO_ACCION` nulo |

- **R-A1** (principio VII, compuerta G7): el registro se escribe en la **misma transacción** que la
  operación auditada. Si la operación se revierte, el registro también. Se implementa con un
  interceptor de persistencia (D-03) y se verifica con una prueba de integración que fuerza el
  rollback.
- **R-A2**: tabla de solo inserción, sin actualización ni borrado.
- **R-A3** (principio VIII): los campos JSON **nunca** incluyen `PASSWORD_HASH`, `PASSWORD_SALT` ni
  `TOKEN_HASH`. Se excluyen de forma explícita en el interceptor, no por omisión accidental.

**Nota de alcance**: el ingreso exitoso y el fallido **no** se auditan aquí; quedan en
`LoginAttempts`, que es su tabla natural y la que el modelo previó para eso. Auditar lo mismo en
dos tablas produce dos fuentes de verdad que se desincronizan.

---

## Verificación final contra el principio X

| Regla de la especificación | ¿Requiere columna nueva? | Resolución |
|---|---|---|
| FR-011 rotación de credencial | No | `REVOKED_AT` en la fila consumida |
| FR-013 revocación en cascada por reutilización | No | Estado de `REVOKED_AT`, ver D-05 |
| FR-017 bloqueo tras 5 fallos | No | `INTENTOS_FALLIDOS` y `BLOQUEADO_HASTA` |
| FR-018 desbloqueo automático | No | Comparación de `BLOQUEADO_HASTA` contra la hora actual |
| FR-022 intento contra cuenta inexistente | No | `LoginAttempts.USUARIO_ID` admite nulos |
| FR-028 auditoría de cambios de rol | No | `AUDITORIA.LogUsuarios` con antes y después |
| FR-030 último administrador | No | Consulta de conteo dentro de la transacción |
| FR-035 primer administrador | No | Inserción en `Empleados` y `Usuarios` |
| D-01 versionado del algoritmo de hash | **Sí, a futuro** | No se necesita hoy con un algoritmo único. Un cambio futuro de algoritmo DEBE proponer la columna según el principio X |

**Cambios de esquema propuestos en esta feature: ninguno.**
