# language: es
@feature-001 @RF-LOG-04 @autorizacion
Característica: Control de acceso por rol validado en el servidor
  Como administrador del negocio
  quiero que cada operación verifique el rol del solicitante en el servidor
  para que ocultar un control en la pantalla no sea nunca lo que protege el sistema.

  Requisitos cubiertos: FR-024 a FR-032a.
  Contrato: GET /api/roles, PUT /api/users/{id}/roles

  Antecedentes:
    Dado que existen las cuentas activas "admin1", "vend1", "opto1" y "sinrol1"
    Y que "admin1" tiene el rol "Administrador"
    Y que "vend1" tiene el rol "Vendedor"
    Y que "opto1" tiene el rol "Optometra"
    Y que "sinrol1" no tiene ningún rol asignado
    Y que existe la cuenta activa "admin2" con el rol "Administrador"

  # ---------------------------------------------------------------------------
  # Conjunto cerrado de roles (FR-024)
  # ---------------------------------------------------------------------------

  @FR-024 @smoke
  Escenario: El sistema reconoce exactamente tres roles
    Dado que "admin1" tiene una sesión activa
    Cuando se envía GET /api/roles
    Entonces la respuesta tiene código 200
    Y el cuerpo contiene exactamente 3 elementos
    Y los códigos devueltos son exactamente "Administrador", "Vendedor" y "Optometra"

  @FR-024 @equivalencia
  Esquema del escenario: Un código de rol fuera del conjunto cerrado se rechaza
    Dado que "admin1" tiene una sesión activa
    Cuando se envía PUT /api/users/{id}/roles para "vend1" con los roles ["<codigo>"]
    Entonces la respuesta tiene código 400
    Y los roles de "vend1" siguen siendo exactamente ["Vendedor"]

    Ejemplos:
      | codigo        | clase de entrada                            |
      | Supervisor    | rol inexistente con nombre plausible        |
      | administrador | código válido con la caja alterada          |
      | Optómetra     | nombre visible con tilde en vez del código  |
      |               | cadena vacía                                |
      | Vendedor'--   | intento de inyección en el código de rol    |

  @FR-024
  Escenario: No existe forma de crear un rol nuevo
    Dado que "admin1" tiene una sesión activa
    Cuando se envía POST /api/roles con el código "Supervisor"
    Entonces la respuesta tiene código 404 o 405
    Y la tabla Roles sigue conteniendo exactamente 3 filas

  # ---------------------------------------------------------------------------
  # Autorización por rol, verificada en el servidor (FR-025, FR-026)
  # ---------------------------------------------------------------------------

  @FR-026 @seguridad @smoke
  Escenario: Un usuario sin el rol requerido es rechazado al invocar directamente el servidor
    Dado que "vend1" tiene una sesión activa
    Cuando se envía GET /api/roles omitiendo por completo la interfaz de usuario
    Entonces la respuesta tiene código 403

  @FR-026 @seguridad
  Escenario: Ocultar el control en la interfaz no sustituye la validación del servidor
    Dado que "vend1" tiene una sesión activa
    Y que la interfaz no muestra a "vend1" ningún control de administración de roles
    Cuando se envía PUT /api/users/{id}/roles para "opto1" con los roles ["Vendedor"]
    Entonces la respuesta tiene código 403
    Y los roles de "opto1" siguen siendo exactamente ["Optometra"]

  @FR-025
  Escenario: Un usuario con dos roles se autoriza si cualquiera de ellos satisface la política
    Dado que "admin1" asignó a "vend1" los roles ["Vendedor", "Administrador"]
    Y que "vend1" volvió a autenticarse
    Cuando se envía GET /api/roles con la sesión de "vend1"
    Entonces la respuesta tiene código 200

  @FR-031 @limite
  Escenario: Un usuario sin roles ingresa pero no autoriza ninguna operación protegida
    Cuando "sinrol1" ingresa correctamente
    Entonces la respuesta tiene código 200
    Y el cuerpo contiene el campo "roles" con una lista vacía
    Cuando se envía GET /api/auth/session con la sesión de "sinrol1"
    Entonces la respuesta tiene código 200
    Cuando se envía GET /api/roles con la sesión de "sinrol1"
    Entonces la respuesta tiene código 403

  # ---------------------------------------------------------------------------
  # Quién puede cambiar roles (FR-027)
  # ---------------------------------------------------------------------------

  @FR-027 @seguridad
  Esquema del escenario: Solo el Administrador puede modificar roles
    Dado que "<solicitante>" tiene una sesión activa
    Cuando se envía PUT /api/users/{id}/roles para "opto1" con los roles ["Vendedor"]
    Entonces la respuesta tiene código <codigo>

    Ejemplos:
      | solicitante | codigo | motivo                                  |
      | admin1      | 200    | tiene rol Administrador                 |
      | vend1       | 403    | autenticado sin rol Administrador        |
      | opto1       | 403    | autenticado sin rol Administrador        |
      | sinrol1     | 403    | autenticado sin ningún rol               |

  @FR-027 @seguridad
  Escenario: Sin sesión no se puede modificar roles
    Dado que no hay ninguna sesión activa
    Cuando se envía PUT /api/users/{id}/roles para "opto1" con los roles ["Vendedor"]
    Entonces la respuesta tiene código 401

  @FR-027
  Escenario: Modificar los roles de un usuario inexistente devuelve no encontrado
    Dado que "admin1" tiene una sesión activa
    Y que no existe ningún usuario con identificador 999999
    Cuando se envía PUT /api/users/999999/roles con los roles ["Vendedor"]
    Entonces la respuesta tiene código 404

  # ---------------------------------------------------------------------------
  # Auditoría del cambio de roles (FR-028)
  # ---------------------------------------------------------------------------

  @FR-028 @auditoria
  Escenario: Un cambio de roles se audita con valor anterior y posterior
    Dado que "admin1" tiene una sesión activa
    Y que los roles de "opto1" son exactamente ["Optometra"]
    Cuando se envía PUT /api/users/{id}/roles para "opto1" con los roles ["Optometra", "Vendedor"]
    Entonces la respuesta tiene código 200
    Y la tabla de auditoría de usuarios contiene exactamente 1 fila nueva
    Y esa fila registra "admin1" como usuario responsable
    Y esa fila registra "opto1" como usuario afectado
    Y esa fila registra el valor anterior ["Optometra"]
    Y esa fila registra el valor posterior ["Optometra", "Vendedor"]
    Y esa fila registra la marca de tiempo en UTC

  @FR-028 @auditoria @transaccion
  Escenario: Si el cambio de roles falla, no queda auditoría huérfana
    Dado que "admin1" tiene una sesión activa
    Y que la escritura en UsuariosRoles fallará al aplicarse
    Cuando se envía PUT /api/users/{id}/roles para "opto1" con los roles ["Vendedor"]
    Entonces la respuesta tiene un código de error
    Y los roles de "opto1" siguen siendo exactamente ["Optometra"]
    Y la tabla de auditoría de usuarios no contiene ninguna fila nueva

  # ---------------------------------------------------------------------------
  # Roles atribuidos a la cuenta, no al empleado (FR-029)
  # ---------------------------------------------------------------------------

  @FR-029
  Escenario: Un empleado sin cuenta de usuario no tiene roles ni acceso
    Dado que existe el empleado "Carla Ruiz" sin ninguna cuenta de usuario asociada
    Cuando se consultan los roles atribuibles a "Carla Ruiz"
    Entonces el resultado es una lista vacía
    Y no existe ninguna credencial con la que "Carla Ruiz" pueda ingresar

  @FR-029
  Escenario: Los roles de un empleado se determinan a través de su cuenta
    Dado que el empleado "Juan Pérez" tiene la cuenta "vend1" con el rol "Vendedor"
    Cuando se consultan los roles atribuibles a "Juan Pérez"
    Entonces el resultado es exactamente ["Vendedor"]

  # ---------------------------------------------------------------------------
  # Último administrador (FR-030) — el borde está en pasar de 1 a 0
  # ---------------------------------------------------------------------------

  @FR-030 @limite
  Escenario: Con dos administradores activos se puede quitar el rol a uno
    Dado que los únicos usuarios con rol "Administrador" activos son "admin1" y "admin2"
    Y que "admin1" tiene una sesión activa
    Cuando se envía PUT /api/users/{id}/roles para "admin2" con los roles ["Vendedor"]
    Entonces la respuesta tiene código 200
    Y queda exactamente 1 usuario activo con rol "Administrador"

  @FR-030 @limite
  Escenario: Con un único administrador activo no se puede dejar el sistema sin ninguno
    Dado que el único usuario con rol "Administrador" activo es "admin1"
    Y que "admin1" tiene una sesión activa
    Cuando se envía PUT /api/users/{id}/roles para "admin1" con los roles ["Vendedor"]
    Entonces la respuesta tiene código 400
    Y el campo "type" del cuerpo es exactamente "https://optica/errors/ultimo-administrador"
    Y los roles de "admin1" siguen siendo exactamente ["Administrador"]

  @FR-030 @limite
  Escenario: La regla del último administrador se evalúa sobre el estado final, no paso a paso
    Dado que el único usuario con rol "Administrador" activo es "admin1"
    Y que "admin1" tiene una sesión activa
    Cuando se envía PUT /api/users/{id}/roles para "admin1" con los roles ["Vendedor", "Administrador"]
    Entonces la respuesta tiene código 200
    Y los roles de "admin1" son exactamente ["Vendedor", "Administrador"]

  @FR-030 @concurrencia
  Escenario: Dos administradores quitándose el rol a la vez no dejan el sistema sin ninguno
    Dado que los únicos usuarios con rol "Administrador" activos son "admin1" y "admin2"
    Cuando "admin1" y "admin2" envían de forma simultánea la petición de quitarse su propio rol de administrador
    Entonces exactamente 1 respuesta tiene código 200
    Y exactamente 1 respuesta tiene código 400
    Y queda al menos 1 usuario activo con rol "Administrador"

  # ---------------------------------------------------------------------------
  # Efecto de desactivar o cambiar roles (FR-032, FR-032a)
  # ---------------------------------------------------------------------------

  @FR-032a @seguridad @smoke
  Escenario: Cambiar los roles revoca en el acto las credenciales de renovación del afectado
    Dado que "opto1" ingresó correctamente y su credencial de renovación es "RT-1"
    Y que "admin1" tiene una sesión activa
    Cuando se envía PUT /api/users/{id}/roles para "opto1" con los roles ["Vendedor"]
    Entonces la respuesta tiene código 200
    Y el campo "credencialesRevocadas" del cuerpo es igual a 1
    Cuando se envía POST /api/auth/refresh con la credencial "RT-1"
    Entonces la respuesta tiene código 401

  @FR-032 @limite
  Escenario: El cambio de roles surte efecto a más tardar al vencer el token de acceso
    Dado que "opto1" ingresó correctamente a las "2026-08-20T10:00:00Z"
    Y que "admin1" quitó a "opto1" todos sus roles a las "2026-08-20T10:01:00Z"
    Cuando el reloj de prueba avanza a "2026-08-20T10:14:59Z"
    Y se envía GET /api/auth/session con el token de acceso de "opto1"
    Entonces la respuesta tiene código 200
    Cuando el reloj de prueba avanza a "2026-08-20T10:15:00Z"
    Y se envía GET /api/auth/session con el mismo token de acceso
    Entonces la respuesta tiene código 401
    Y la ventana de exposición no superó los 15 minutos

  @FR-032a @seguridad
  Escenario: Desactivar un usuario revoca sus credenciales y le impide renovar
    Dado que "vend1" ingresó correctamente y su credencial de renovación es "RT-1"
    Cuando se desactiva la cuenta "vend1"
    Entonces ninguna fila de RefreshTokens de "vend1" queda con REVOKED_AT nulo
    Cuando se envía POST /api/auth/refresh con la credencial "RT-1"
    Entonces la respuesta tiene código 401
    Cuando se envía POST /api/auth/login con nombreUsuario "vend1" y su contraseña correcta
    Entonces la respuesta tiene código 401
