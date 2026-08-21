# language: es
@feature-001 @RF-LOG-02 @sesion
Característica: Sesión protegida con renovación, rotación y cierre
  Como empleado autenticado
  quiero que mi sesión se renueve sola durante la jornada
  para trabajar sin reescribir la contraseña, y que una credencial robada deje de servir.

  Requisitos cubiertos: FR-007 a FR-016, y FR-009a sobre vencimiento absoluto de sesión.
  Contrato: POST /api/auth/refresh, POST /api/auth/logout, GET /api/auth/session

  Antecedentes:
    Dado que existe la cuenta "jperez" activa con contraseña "Optica2026#Segura"
    Y que "jperez" tiene asignado el rol "Vendedor"
    Y que el reloj de prueba marca "2026-08-20T08:00:00Z"

  # ---------------------------------------------------------------------------
  # Emisión (FR-007, FR-008, FR-009, FR-015)
  # ---------------------------------------------------------------------------

  @FR-007 @smoke
  Escenario: El ingreso emite un token de acceso y una credencial de renovación
    Cuando "jperez" ingresa correctamente
    Entonces la respuesta tiene código 200
    Y la respuesta incluye la cookie "optica_at"
    Y la respuesta incluye la cookie "optica_rt"
    Y la tabla RefreshTokens contiene exactamente 1 fila activa para "jperez"

  @FR-008 @limite
  Escenario: El token de acceso vence exactamente a los 15 minutos
    Dado que "jperez" ingresó correctamente a las "2026-08-20T08:00:00Z"
    Cuando el reloj de prueba avanza a "2026-08-20T08:14:59Z"
    Y se envía GET /api/auth/session con la cookie "optica_at" vigente
    Entonces la respuesta tiene código 200
    Cuando el reloj de prueba avanza a "2026-08-20T08:15:00Z"
    Y se envía GET /api/auth/session con la misma cookie "optica_at"
    Entonces la respuesta tiene código 401

  @FR-009 @FR-009a @limite
  Escenario: La sesión vence 8 horas después de la autenticación, no del último uso
    Dado que "jperez" ingresó correctamente a las "2026-08-20T08:00:00Z"
    Y que renovó su sesión cada 14 minutos de forma ininterrumpida
    Cuando el reloj de prueba avanza a "2026-08-20T15:59:00Z"
    Y se envía POST /api/auth/refresh con la credencial vigente
    Entonces la respuesta tiene código 200
    Cuando el reloj de prueba avanza a "2026-08-20T16:00:00Z"
    Y se envía POST /api/auth/refresh con la credencial vigente
    Entonces la respuesta tiene código 401
    Y el usuario debe autenticarse con su contraseña para obtener una sesión nueva

  @FR-009a
  Escenario: La credencial rotada hereda el vencimiento de la sesión sin recalcularlo
    Dado que "jperez" ingresó correctamente a las "2026-08-20T08:00:00Z"
    Cuando el reloj de prueba avanza a "2026-08-20T12:00:00Z"
    Y se envía POST /api/auth/refresh con la credencial vigente
    Entonces la respuesta tiene código 200
    Y la nueva fila de RefreshTokens para "jperez" tiene EXPIRES_AT igual a "2026-08-20T16:00:00Z"

  @FR-015 @seguridad
  Escenario: El token de acceso no transporta datos personales
    Dado que "jperez" ingresó correctamente
    Cuando se decodifica el contenido del token de acceso
    Entonces contiene el identificador del usuario
    Y contiene la lista de roles
    Y contiene las marcas de emisión y vencimiento
    Y no contiene el nombre completo, ni el número de identificación, ni el hash de la contraseña

  @FR-010 @seguridad
  Escenario: La credencial de renovación se almacena hasheada
    Dado que "jperez" ingresó correctamente y su credencial de renovación en claro es conocida por la prueba
    Cuando se consulta la fila activa de RefreshTokens para "jperez"
    Entonces la columna TOKEN_HASH no contiene el valor en claro de la credencial en ninguna codificación
    Y la fila tiene EXPIRES_AT con valor
    Y la fila tiene REVOKED_AT nulo

  # ---------------------------------------------------------------------------
  # Renovación y rotación (FR-011, FR-012, FR-013)
  # ---------------------------------------------------------------------------

  @FR-011 @smoke
  Escenario: Renovar con una credencial válida emite un par nuevo y anula el anterior
    Dado que "jperez" ingresó correctamente y su credencial de renovación es "RT-1"
    Cuando el reloj de prueba avanza 15 minutos
    Y se envía POST /api/auth/refresh con la credencial "RT-1"
    Entonces la respuesta tiene código 200
    Y la respuesta incluye una cookie "optica_rt" con un valor distinto de "RT-1"
    Y la fila de "RT-1" en RefreshTokens tiene REVOKED_AT con valor

  @FR-012 @equivalencia
  Esquema del escenario: Las cuatro clases de credencial inválida rechazan la renovación
    Dado que la credencial presentada está en el estado "<estado>"
    Cuando se envía POST /api/auth/refresh con esa credencial
    Entonces la respuesta tiene código 401
    Y no se emite ninguna cookie de sesión nueva

    Ejemplos:
      | estado                                        |
      | vencida por haberse cumplido las 8 horas      |
      | revocada por un cierre de sesión previo       |
      | ya rotada en una renovación anterior          |
      | perteneciente a otro usuario distinto         |

  @FR-012
  Escenario: Renovar sin presentar credencial se rechaza
    Cuando se envía POST /api/auth/refresh sin la cookie "optica_rt"
    Entonces la respuesta tiene código 401

  @FR-013 @seguridad
  Escenario: Reutilizar una credencial ya rotada revoca toda la cadena del usuario
    Dado que "jperez" ingresó correctamente y su credencial de renovación es "RT-1"
    Y que renovó su sesión y obtuvo la credencial "RT-2"
    Y que renovó de nuevo y obtuvo la credencial "RT-3"
    Cuando se envía POST /api/auth/refresh con la credencial "RT-1"
    Entonces la respuesta tiene código 401
    Y ninguna fila de RefreshTokens de "jperez" queda con REVOKED_AT nulo
    Cuando se envía POST /api/auth/refresh con la credencial "RT-3"
    Entonces la respuesta tiene código 401

  @FR-011 @concurrencia
  Escenario: Dos renovaciones simultáneas con la misma credencial solo permiten una
    Dado que "jperez" ingresó correctamente y su credencial de renovación es "RT-1"
    Cuando se envían 2 peticiones POST /api/auth/refresh con la credencial "RT-1" de forma simultánea
    Entonces exactamente 1 respuesta tiene código 200
    Y exactamente 1 respuesta tiene código 401
    Y la tabla RefreshTokens no contiene más de 1 fila activa para "jperez"

  # ---------------------------------------------------------------------------
  # Cierre de sesión (FR-014)
  # ---------------------------------------------------------------------------

  @FR-014 @smoke
  Escenario: Cerrar sesión revoca la credencial de renovación
    Dado que "jperez" ingresó correctamente y su credencial de renovación es "RT-1"
    Cuando se envía POST /api/auth/logout
    Entonces la respuesta tiene código 204
    Y la fila de "RT-1" en RefreshTokens tiene REVOKED_AT con valor
    Y las cookies "optica_at" y "optica_rt" se eliminan en la respuesta
    Cuando se envía POST /api/auth/refresh con la credencial "RT-1"
    Entonces la respuesta tiene código 401

  @FR-014
  Escenario: Cerrar sesión sin sesión activa responde igual y no falla
    Dado que no hay ninguna sesión activa
    Cuando se envía POST /api/auth/logout
    Entonces la respuesta tiene código 204

  # ---------------------------------------------------------------------------
  # Rechazo de tokens de acceso inválidos (FR-016)
  # ---------------------------------------------------------------------------

  @FR-016 @equivalencia @seguridad
  Esquema del escenario: Los tokens de acceso inválidos se rechazan sin revelar el motivo
    Cuando se envía GET /api/auth/session con un token de acceso "<condicion>"
    Entonces la respuesta tiene código 401
    Y el cuerpo de la respuesta no menciona la firma, ni el vencimiento, ni el formato del token

    Ejemplos:
      | condicion                                  |
      | firmado con una clave distinta              |
      | vencido hace 1 minuto                      |
      | con el cuerpo alterado tras la firma       |
      | con una cadena que no es un token válido   |
      | ausente                                    |

  @FR-016 @seguridad
  Escenario: Un token válido de otra instalación no concede acceso
    Dado un token de acceso bien formado emitido con el emisor "otra-optica"
    Cuando se envía GET /api/auth/session con ese token
    Entonces la respuesta tiene código 401
