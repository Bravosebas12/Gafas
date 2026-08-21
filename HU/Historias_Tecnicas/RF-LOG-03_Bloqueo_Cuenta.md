# RF-LOG-03 Bloqueo Automático de Cuenta

Como administrador de seguridad, quiero que el sistema bloquee automáticamente una cuenta tras cinco intentos fallidos consecutivos durante quince minutos, para reducir el riesgo de ataques de fuerza bruta.

**Descripción / Contexto**

Registrar intentos fallidos de autenticación por cuenta o identificador seguro. Al alcanzar cinco fallas consecutivas dentro de una ventana de quince minutos, bloquear temporalmente el acceso.

**Ruta**

API: `/api/auth`
UI: `/login`, `/configuracion-perfil`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que una cuenta acumula cinco intentos fallidos consecutivos en quince minutos
Cuando intenta iniciar sesión nuevamente
Entonces el sistema bloquea la cuenta temporalmente.
Escenario: validación funcional
Dado que transcurren quince minutos desde el bloqueo
Cuando el usuario intenta iniciar sesión
Entonces puede volver a intentarlo.
Escenario: validación funcional
Dado que el usuario inicia sesión correctamente antes de alcanzar el límite
Cuando se registra el intento
Entonces el contador de fallas consecutivas se reinicia.
Escenario: validación funcional
Dado que se supera el límite de intentos
Cuando se responde al cliente
Entonces no se revela información detallada que facilite ataques.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Ventana de bloqueo: 15 minutos.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Límite: 5 intentos fallidos consecutivos.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Registrar auditoría del bloqueo.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Evitar enumeración de usuarios.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de login con mensaje controlado de cuenta bloqueada temporalmente.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST`
- Ruta: `/api/auth/login`
- Request: Intento de autenticación
- Response esperado: Acceso permitido, error 401 o bloqueo temporal
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: LoginAttemptTracker, AccountLockoutService, AuthService.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, LoginAttempts, AccountLockouts.

**Consideraciones Funcionales**

Registrar intentos fallidos de autenticación por cuenta o identificador seguro. Al alcanzar cinco fallas consecutivas dentro de una ventana de quince minutos, bloquear temporalmente el acceso.
Entregables:
- Servicio de control de intentos fallidos.
- Persistencia de contador y fecha de bloqueo.
- Regla de desbloqueo automático.
- Pruebas de límite, reinicio y expiración de bloqueo.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Autenticación y Control de Acceso.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
