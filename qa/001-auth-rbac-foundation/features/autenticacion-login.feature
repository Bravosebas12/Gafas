# language: es
@feature-001 @RF-LOG-01 @autenticacion
Característica: Ingreso con credenciales propias
  Como empleado de la óptica
  quiero autenticarme contra la base de datos propia del sistema
  para acceder sin depender de proveedores de identidad externos.

  Requisitos cubiertos: FR-001 a FR-006.
  Contrato: POST /api/auth/login — specs/001-auth-rbac-foundation/contracts/auth-endpoints.md

  Antecedentes:
    Dado que existe el empleado "Juan Pérez" con la cuenta de usuario "jperez" activa
    Y que la contraseña de "jperez" es "Optica2026#Segura"
    Y que "jperez" tiene asignado el rol "Vendedor"
    Y que el contador de intentos fallidos de "jperez" está en 0
    Y que la tabla LoginAttempts está vacía

  # ---------------------------------------------------------------------------
  # Clase válida: credenciales correctas sobre cuenta activa
  # ---------------------------------------------------------------------------

  @FR-001 @equivalencia @smoke
  Escenario: Credenciales correctas sobre cuenta activa conceden acceso
    Cuando se envía POST /api/auth/login con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 200
    Y el cuerpo contiene el campo "roles" con el valor ["Vendedor"]
    Y la respuesta incluye la cookie "optica_at" con los atributos HttpOnly, Secure y SameSite=Strict
    Y la respuesta incluye la cookie "optica_rt" con los atributos HttpOnly, Secure y SameSite=Strict
    Y el cuerpo de la respuesta no contiene el valor de ninguna de las dos cookies

  @FR-001 @seguridad
  Escenario: La validación de credenciales consulta únicamente la base de datos propia
    Cuando se envía POST /api/auth/login con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 200
    Y no se registró ninguna petición de red hacia un host externo durante la autenticación

  # ---------------------------------------------------------------------------
  # Clases inválidas: las cuatro deben ser indistinguibles entre sí (FR-004)
  # ---------------------------------------------------------------------------

  @FR-004 @equivalencia @seguridad
  Esquema del escenario: Las cuatro clases de rechazo devuelven una respuesta idéntica
    Dado que <precondicion>
    Cuando se envía POST /api/auth/login con nombreUsuario "<usuario>" y contraseña "<clave>"
    Entonces la respuesta tiene código 401
    Y el campo "title" del cuerpo es exactamente "Usuario o contraseña incorrectos"
    Y el campo "type" del cuerpo es exactamente "https://optica/errors/credenciales-invalidas"
    Y el cuerpo de la respuesta no contiene los textos "no existe", "inactiv", "bloquead" ni "contraseña incorrecta"

    Ejemplos:
      | precondicion                                              | usuario     | clave                |
      | no existe ninguna cuenta con nombre de usuario "fantasma"  | fantasma    | Optica2026#Segura    |
      | la cuenta "jperez" está activa                            | jperez      | ClaveEquivocada#1    |
      | la cuenta "mlopez" existe con ACTIVO = 0                  | mlopez      | Optica2026#Segura    |
      | la cuenta "rgomez" está bloqueada hasta dentro de 10 min  | rgomez      | Optica2026#Segura    |

  @FR-005 @equivalencia
  Escenario: Una cuenta desactivada lógicamente no puede ingresar
    Dado que la cuenta "mlopez" existe con ACTIVO = 0 y contraseña "Optica2026#Segura"
    Cuando se envía POST /api/auth/login con nombreUsuario "mlopez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 401
    Y no se emitió ninguna cookie de sesión
    Y la fila de "mlopez" en la tabla Usuarios sigue existiendo con ACTIVO = 0

  @FR-004 @seguridad @SC-004
  Escenario: El tiempo de respuesta no permite deducir si una cuenta existe
    Dado que no existe ninguna cuenta con nombre de usuario "fantasma"
    Cuando se envían 50 peticiones de ingreso con nombreUsuario "fantasma" y contraseña "ClaveEquivocada#1"
    Y se envían 50 peticiones de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1"
    Entonces la diferencia entre las medianas de tiempo de respuesta de los dos grupos es menor a 100 milisegundos
    Y ambos grupos devolvieron código 401 en las 100 peticiones

  # ---------------------------------------------------------------------------
  # Validación de formato: 400 por formato frente a 401 por credenciales
  # ---------------------------------------------------------------------------

  @FR-004 @limite
  Esquema del escenario: El límite de longitud del nombre de usuario separa el 400 del 401
    Cuando se envía POST /api/auth/login con un nombreUsuario de <longitud> caracteres y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código <codigo>

    Ejemplos:
      | longitud | codigo | motivo                                                     |
      | 0        | 400    | por debajo del mínimo, falla la validación de formato      |
      | 1        | 401    | mínimo válido, el formato pasa y fallan las credenciales   |
      | 100      | 401    | máximo válido, el formato pasa y fallan las credenciales   |
      | 101      | 400    | por encima del máximo, falla la validación de formato      |

  @FR-004 @limite
  Esquema del escenario: El límite de longitud de la contraseña separa el 400 del 401
    Cuando se envía POST /api/auth/login con nombreUsuario "jperez" y una contraseña de <longitud> caracteres
    Entonces la respuesta tiene código <codigo>

    Ejemplos:
      | longitud | codigo | motivo                                                |
      | 0        | 400    | por debajo del mínimo                                 |
      | 1        | 401    | mínimo válido, credenciales incorrectas               |
      | 256      | 401    | máximo válido, credenciales incorrectas               |
      | 257      | 400    | por encima del máximo                                 |

  @FR-003 @equivalencia
  Escenario: Una contraseña con caracteres no latinos y espacios se acepta sin corrupción
    Dado que existe la cuenta "acruz" activa con contraseña "Ñandú Ópti¢a 2026 ✓"
    Cuando se envía POST /api/auth/login con nombreUsuario "acruz" y contraseña "Ñandú Ópti¢a 2026 ✓"
    Entonces la respuesta tiene código 200
    Y el cuerpo contiene el campo "roles"

  # ---------------------------------------------------------------------------
  # Almacenamiento de contraseñas (FR-003)
  # ---------------------------------------------------------------------------

  @FR-003 @seguridad
  Escenario: La contraseña no se almacena en claro ni de forma reversible
    Cuando se consulta la fila de "jperez" en la tabla Usuarios
    Entonces la columna PASSWORD_HASH no contiene la cadena "Optica2026#Segura" en ninguna codificación
    Y la columna PASSWORD_SALT tiene una longitud de al menos 16 bytes
    Y no existe ninguna columna que permita recuperar la contraseña original

  @FR-003 @seguridad
  Escenario: Dos cuentas con la misma contraseña producen hashes distintos
    Dado que existe la cuenta "bdiaz" activa con contraseña "Optica2026#Segura"
    Cuando se comparan las filas de "jperez" y "bdiaz" en la tabla Usuarios
    Entonces los valores de PASSWORD_SALT de ambas filas son distintos
    Y los valores de PASSWORD_HASH de ambas filas son distintos

  # ---------------------------------------------------------------------------
  # Registro de intentos (FR-006)
  # ---------------------------------------------------------------------------

  @FR-006 @auditoria
  Escenario: Un ingreso exitoso queda registrado
    Cuando se envía POST /api/auth/login con nombreUsuario "jperez" y contraseña "Optica2026#Segura" desde la dirección "192.168.1.40"
    Entonces la tabla LoginAttempts contiene exactamente 1 fila
    Y esa fila tiene NOMBRE_USUARIO igual a "jperez"
    Y esa fila tiene EXITOSO igual a 1
    Y esa fila tiene IP_ORIGEN igual a "192.168.1.40"
    Y esa fila tiene USUARIO_ID igual al identificador de "jperez"
    Y el valor de FECHA_INTENTO de esa fila está expresado en UTC

  @FR-006 @FR-022 @auditoria
  Escenario: Un intento contra una cuenta inexistente queda registrado sin crear la cuenta
    Dado que no existe ninguna cuenta con nombre de usuario "fantasma"
    Cuando se envía POST /api/auth/login con nombreUsuario "fantasma" y contraseña "ClaveEquivocada#1" desde la dirección "192.168.1.41"
    Entonces la tabla LoginAttempts contiene exactamente 1 fila
    Y esa fila tiene NOMBRE_USUARIO igual a "fantasma"
    Y esa fila tiene EXITOSO igual a 0
    Y esa fila tiene USUARIO_ID nulo
    Y la tabla Usuarios no contiene ninguna fila con NOMBRE_USUARIO igual a "fantasma"

  @FR-006 @limite
  Escenario: Una dirección de origen en el límite de 45 caracteres se registra completa
    Cuando se envía POST /api/auth/login con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1" desde la dirección "2001:0db8:85a3:0000:0000:8a2e:0370:7334"
    Entonces la tabla LoginAttempts contiene exactamente 1 fila
    Y esa fila tiene IP_ORIGEN igual a "2001:0db8:85a3:0000:0000:8a2e:0370:7334"
    Y el valor de IP_ORIGEN no está truncado

  # ---------------------------------------------------------------------------
  # Prohibición de proveedores externos (FR-002)
  # ---------------------------------------------------------------------------

  @FR-002 @seguridad
  Escenario: El sistema no expone ninguna vía de ingreso con proveedor externo
    Cuando se inspeccionan los esquemas de autenticación registrados en la aplicación
    Entonces ninguno corresponde a Google, Microsoft, Facebook ni a otro proveedor de identidad externo
    Y la página de ingreso no contiene ningún control de ingreso con proveedor externo

  @FR-002 @seguridad
  Escenario: Una configuración con proveedor externo impide el arranque
    Dado que la configuración declara un proveedor de identidad externo con el nombre "Google"
    Cuando se intenta arrancar la aplicación
    Entonces el arranque falla
    Y el mensaje de error nombra la clave de configuración rechazada
    Y la aplicación no queda escuchando peticiones
