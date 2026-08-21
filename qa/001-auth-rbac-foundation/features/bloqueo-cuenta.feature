# language: es
@feature-001 @RF-LOG-03 @bloqueo
Característica: Bloqueo automático de cuenta ante fuerza bruta
  Como responsable de seguridad
  quiero que una cuenta se bloquee tras cinco intentos fallidos consecutivos
  para reducir el riesgo de adivinación de contraseñas.

  Requisitos cubiertos: FR-017 a FR-023.
  Parámetros de la política: 5 intentos, ventana de 15 minutos, bloqueo de 15 minutos.

  Antecedentes:
    Dado que existe la cuenta "jperez" activa con contraseña "Optica2026#Segura"
    Y que el contador de intentos fallidos de "jperez" está en 0
    Y que "jperez" no está bloqueada
    Y que el reloj de prueba marca "2026-08-20T09:00:00Z"

  # ---------------------------------------------------------------------------
  # Límite de intentos: el borde exacto está entre 4 y 5 (FR-017)
  # ---------------------------------------------------------------------------

  @FR-017 @limite
  Escenario: Cuatro intentos fallidos no bloquean la cuenta
    Cuando se envían 4 intentos de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1"
    Entonces la fila de "jperez" en Usuarios tiene INTENTOS_FALLIDOS igual a 4
    Y la fila de "jperez" tiene BLOQUEADO_HASTA nulo
    Cuando se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 200

  @FR-017 @limite @smoke
  Escenario: El quinto intento fallido consecutivo bloquea la cuenta
    Dado que "jperez" acumula 4 intentos fallidos dentro de la ventana de 15 minutos
    Cuando se envía un quinto intento de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1"
    Entonces la respuesta tiene código 401
    Y la fila de "jperez" en Usuarios tiene BLOQUEADO_HASTA igual a "2026-08-20T09:15:00Z"

  @FR-019 @smoke @seguridad
  Escenario: Durante el bloqueo se rechaza incluso la contraseña correcta
    Dado que "jperez" está bloqueada hasta "2026-08-20T09:15:00Z"
    Y que el reloj de prueba marca "2026-08-20T09:05:00Z"
    Cuando se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 401
    Y no se emitió ninguna cookie de sesión

  @FR-004 @FR-019 @seguridad
  Escenario: El mensaje durante el bloqueo es indistinguible del de credenciales incorrectas
    Dado que "jperez" está bloqueada hasta "2026-08-20T09:15:00Z"
    Cuando se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Y se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1" tras liberarse el bloqueo
    Entonces ambas respuestas tienen el mismo código 401
    Y ambos cuerpos tienen el campo "title" igual a "Usuario o contraseña incorrectos"
    Y ningún cuerpo indica cuánto falta para que el bloqueo termine

  # ---------------------------------------------------------------------------
  # Duración del bloqueo: borde exacto a los 15 minutos (FR-018)
  # ---------------------------------------------------------------------------

  @FR-018 @limite
  Escenario: El bloqueo sigue vigente un segundo antes de cumplirse
    Dado que "jperez" está bloqueada hasta "2026-08-20T09:15:00Z"
    Cuando el reloj de prueba avanza a "2026-08-20T09:14:59Z"
    Y se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 401

  @FR-018 @FR-020 @limite
  Escenario: El bloqueo se libera solo al cumplirse los 15 minutos
    Dado que "jperez" está bloqueada hasta "2026-08-20T09:15:00Z"
    Cuando el reloj de prueba avanza a "2026-08-20T09:15:00Z"
    Y se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 200
    Y la fila de "jperez" en Usuarios tiene INTENTOS_FALLIDOS igual a 0
    Y la fila de "jperez" tiene BLOQUEADO_HASTA nulo
    Y ningún administrador intervino para desbloquear la cuenta

  # ---------------------------------------------------------------------------
  # Ventana deslizante: el borde está en los 15 minutos entre intentos (FR-017)
  # ---------------------------------------------------------------------------

  @FR-017 @limite
  Escenario: Un intento fuera de la ventana de 15 minutos no acumula con los anteriores
    Dado que "jperez" acumuló 4 intentos fallidos entre "2026-08-20T09:00:00Z" y "2026-08-20T09:02:00Z"
    Cuando el reloj de prueba avanza a "2026-08-20T09:17:01Z"
    Y se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1"
    Entonces la fila de "jperez" en Usuarios tiene BLOQUEADO_HASTA nulo
    Y la fila de "jperez" tiene INTENTOS_FALLIDOS igual a 1

  @FR-017 @limite
  Escenario: Un intento en el último segundo de la ventana sí acumula y bloquea
    Dado que "jperez" acumuló 4 intentos fallidos con el último a las "2026-08-20T09:02:00Z"
    Cuando el reloj de prueba avanza a "2026-08-20T09:16:59Z"
    Y se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1"
    Entonces la fila de "jperez" en Usuarios tiene BLOQUEADO_HASTA con valor

  # ---------------------------------------------------------------------------
  # Reinicio del contador (FR-020)
  # ---------------------------------------------------------------------------

  @FR-020
  Escenario: Un ingreso exitoso reinicia el contador de fallas consecutivas
    Dado que "jperez" acumula 3 intentos fallidos
    Cuando se envía un intento de ingreso con nombreUsuario "jperez" y contraseña "Optica2026#Segura"
    Entonces la respuesta tiene código 200
    Y la fila de "jperez" en Usuarios tiene INTENTOS_FALLIDOS igual a 0
    Cuando se envían 4 intentos de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1"
    Entonces la fila de "jperez" tiene BLOQUEADO_HASTA nulo

  # ---------------------------------------------------------------------------
  # Auditoría del bloqueo (FR-021)
  # ---------------------------------------------------------------------------

  @FR-021 @auditoria
  Escenario: El bloqueo queda registrado en auditoría
    Dado que "jperez" acumula 4 intentos fallidos
    Cuando se envía un quinto intento de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1"
    Entonces la tabla de auditoría de usuarios contiene exactamente 1 fila nueva
    Y esa fila identifica la cuenta "jperez"
    Y esa fila registra el momento del bloqueo en UTC
    Y esa fila registra el valor de BLOQUEADO_HASTA resultante

  @FR-021 @auditoria
  Escenario: El registro de auditoría del bloqueo no puede alterarse
    Dado que existe un registro de auditoría del bloqueo de "jperez"
    Cuando se intenta actualizar ese registro
    Entonces la operación es rechazada
    Cuando se intenta eliminar ese registro
    Entonces la operación es rechazada

  # ---------------------------------------------------------------------------
  # Cuentas inexistentes (FR-022)
  # ---------------------------------------------------------------------------

  @FR-022 @seguridad
  Escenario: Cinco intentos contra una cuenta inexistente no crean ni bloquean nada
    Dado que no existe ninguna cuenta con nombre de usuario "fantasma"
    Cuando se envían 5 intentos de ingreso con nombreUsuario "fantasma" y contraseña "ClaveEquivocada#1"
    Entonces las 5 respuestas tienen código 401
    Y las 5 respuestas tienen el campo "title" igual a "Usuario o contraseña incorrectos"
    Y la tabla Usuarios no contiene ninguna fila con NOMBRE_USUARIO igual a "fantasma"
    Y la tabla LoginAttempts contiene exactamente 5 filas con NOMBRE_USUARIO igual a "fantasma"

  # ---------------------------------------------------------------------------
  # Concurrencia (FR-023)
  # ---------------------------------------------------------------------------

  @FR-023 @concurrencia
  Escenario: Cinco intentos fallidos simultáneos cuentan exactamente cinco
    Cuando se envían 5 intentos de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1" de forma simultánea
    Entonces la fila de "jperez" en Usuarios tiene INTENTOS_FALLIDOS igual a 5
    Y la fila de "jperez" tiene BLOQUEADO_HASTA con valor
    Y la tabla LoginAttempts contiene exactamente 5 filas para "jperez"

  @FR-023 @concurrencia
  Escenario: Cuatro intentos fallidos simultáneos no bloquean por conteo de menos ni de más
    Cuando se envían 4 intentos de ingreso con nombreUsuario "jperez" y contraseña "ClaveEquivocada#1" de forma simultánea
    Entonces la fila de "jperez" en Usuarios tiene INTENTOS_FALLIDOS igual a 4
    Y la fila de "jperez" tiene BLOQUEADO_HASTA nulo

  @FR-023 @concurrencia
  Escenario: Un ingreso correcto concurrente con fallos no deja el contador inconsistente
    Dado que "jperez" acumula 3 intentos fallidos
    Cuando se envían de forma simultánea 1 intento con la contraseña correcta y 1 intento con contraseña incorrecta
    Entonces la fila de "jperez" en Usuarios tiene INTENTOS_FALLIDOS igual a 0 o igual a 1
    Y la fila de "jperez" tiene BLOQUEADO_HASTA nulo
