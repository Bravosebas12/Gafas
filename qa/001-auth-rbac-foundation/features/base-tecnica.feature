# language: es
@feature-001 @base-tecnica
Característica: Base técnica verificable de la solución
  Como equipo de desarrollo
  quiero que la estructura, la cobertura y la observabilidad se comprueben automáticamente
  para que las compuertas de la constitución no dependan de la buena voluntad de quien revisa.

  Requisitos cubiertos: FR-033 a FR-037.
  Compuertas de la constitución: G1, G3, G5, G8, G10.

  # ---------------------------------------------------------------------------
  # Regla de dependencias (FR-033, compuerta G1)
  # ---------------------------------------------------------------------------

  @FR-033 @arquitectura @G1
  Escenario: El dominio no depende de ninguna otra capa
    Cuando se analizan las referencias del proyecto "Optica.Domain"
    Entonces no referencia "Optica.Application"
    Y no referencia "Optica.Infrastructure"
    Y no referencia "Optica.Shared"
    Y no referencia ningún paquete de acceso a datos ni de serialización

  @FR-033 @arquitectura @G1
  Esquema del escenario: Cada proyecto solo referencia lo que la tabla del principio II permite
    Cuando se analizan las referencias del proyecto "<proyecto>"
    Entonces no referencia "<referencia prohibida>"

    Ejemplos:
      | proyecto            | referencia prohibida  |
      | Optica.Application  | Optica.Infrastructure |
      | Optica.Application  | Optica.Web            |
      | Optica.Shared       | Optica.Infrastructure |
      | Optica.Shared       | Optica.Application    |
      | Optica.Web.Client   | Optica.Infrastructure |
      | Optica.Web.Client   | Optica.Application    |

  @FR-033 @arquitectura @G1
  Escenario: Una referencia que viola la regla de dependencias falla la compilación
    Dado que se agrega en "Optica.Domain" una referencia a "Optica.Infrastructure"
    Cuando se ejecuta la compilación de la solución
    Entonces la compilación falla
    Y el mensaje de error nombra el proyecto y la referencia que viola la regla

  @FR-033 @arquitectura
  Escenario: El dominio no lee el reloj del sistema
    Cuando se analiza el código de "Optica.Domain"
    Entonces no contiene ningún uso de DateTime.Now, DateTime.UtcNow ni DateTimeOffset.Now
    Y toda obtención de la hora pasa por la abstracción de reloj

  @FR-033 @arquitectura
  Escenario: El código compartido compila para WebAssembly
    Cuando se compila "Optica.Shared" con destino WebAssembly
    Entonces la compilación termina sin errores
    Y no se reporta ninguna dependencia no soportada en el navegador

  # ---------------------------------------------------------------------------
  # Modelo de datos (FR-034, compuerta G10)
  # ---------------------------------------------------------------------------

  @FR-034 @G10
  Escenario: El esquema aplicado coincide con el script de modelo de datos
    Dado una base de datos creada con los scripts numerados de Scripts/SQL
    Cuando se comparan las tablas del esquema "ADMINISTRACION_USUARIOS" con el script 001
    Entonces existen las tablas Empleados, Roles, Usuarios, UsuariosRoles, RefreshTokens y LoginAttempts
    Y ninguna tabla tiene columnas que no estén declaradas en el script
    Y el modelo de la aplicación no declara ninguna tabla adicional

  @FR-034 @G10
  Escenario: La unicidad del nombre de usuario la impone la base de datos
    Dado que existe la cuenta "jperez"
    Cuando se intenta insertar directamente en Usuarios otra fila con NOMBRE_USUARIO igual a "jperez"
    Entonces la base de datos rechaza la inserción por violación de la restricción UQ_Usuarios_NOMBRE_USUARIO

  @FR-034 @G10 @concurrencia
  Escenario: Dos cuentas con el mismo nombre creadas a la vez solo permiten una
    Cuando se envían de forma simultánea 2 peticiones de creación de la cuenta "nuevo1"
    Entonces exactamente 1 petición termina con éxito
    Y la tabla Usuarios contiene exactamente 1 fila con NOMBRE_USUARIO igual a "nuevo1"

  # ---------------------------------------------------------------------------
  # Primer administrador (FR-035)
  # ---------------------------------------------------------------------------

  @FR-035 @seguridad
  Escenario: Se puede crear el primer Administrador sin credenciales en el código
    Dado una base de datos sin ninguna fila en la tabla Usuarios
    Cuando se ejecuta la herramienta de creación del primer Administrador con la contraseña recibida por parámetro
    Entonces la tabla Usuarios contiene exactamente 1 fila
    Y esa cuenta tiene el rol "Administrador"
    Y esa cuenta puede ingresar con la contraseña indicada
    Y ninguna contraseña aparece en el código fuente ni en archivos de configuración versionados

  @FR-035 @seguridad
  Escenario: La herramienta no crea un segundo administrador inicial
    Dado una base de datos que ya contiene al menos una cuenta con rol "Administrador"
    Cuando se ejecuta la herramienta de creación del primer Administrador
    Entonces la herramienta termina con error
    Y el número de filas de la tabla Usuarios no cambia

  @FR-035 @seguridad @G5
  Escenario: No hay secretos en el repositorio
    Cuando se analiza el contenido versionado del repositorio
    Entonces no aparece ninguna cadena de conexión con contraseña
    Y no aparece ninguna clave de firma de token
    Y el archivo .mcp.json no está versionado

  # ---------------------------------------------------------------------------
  # Cobertura (FR-036, compuerta G3)
  # ---------------------------------------------------------------------------

  @FR-036 @G3
  Escenario: La medición de cobertura se ejecuta automáticamente
    Cuando se ejecuta la suite de pruebas con medición de cobertura
    Entonces se genera un reporte de cobertura
    Y el reporte informa el porcentaje global y el porcentaje por proyecto

  @FR-036 @G3 @limite
  Esquema del escenario: El umbral de cobertura falla el proceso por debajo del límite
    Dado que la cobertura medida de "<ambito>" es <porcentaje> por ciento
    Cuando se evalúa la compuerta de cobertura
    Entonces el resultado del proceso es "<resultado>"

    Ejemplos:
      | ambito              | porcentaje | resultado |
      | global              | 84.9       | falla     |
      | global              | 85.0       | pasa      |
      | Optica.Domain       | 89.9       | falla     |
      | Optica.Domain       | 90.0       | pasa      |
      | Optica.Application  | 89.9       | falla     |
      | Optica.Application  | 90.0       | pasa      |

  # ---------------------------------------------------------------------------
  # Observabilidad (FR-037, compuerta G8)
  # ---------------------------------------------------------------------------

  @FR-037 @G8 @observabilidad
  Esquema del escenario: Cada operación sensible emite registro estructurado con correlación
    Cuando se ejecuta la operación "<operacion>"
    Entonces se emite al menos un registro estructurado
    Y ese registro contiene un identificador de correlación
    Y ese identificador es el mismo en todos los registros de la operación

    Ejemplos:
      | operacion                    |
      | ingreso exitoso              |
      | ingreso fallido              |
      | renovación de sesión         |
      | bloqueo de cuenta            |
      | cambio de roles              |

  @FR-037 @G8 @seguridad
  Escenario: Los registros no contienen datos sensibles
    Cuando se ejecutan un ingreso exitoso, un ingreso fallido y una renovación de sesión
    Y se inspecciona la totalidad de los registros emitidos
    Entonces ningún registro contiene la contraseña en claro
    Y ningún registro contiene el valor del token de acceso
    Y ningún registro contiene el valor de la credencial de renovación
    Y ningún registro contiene el hash ni el salt de la contraseña
