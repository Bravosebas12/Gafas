# RF-LOG-01 Autenticación Nativa con SQL Propia

Como administrador de seguridad de la óptica, quiero que el sistema valide credenciales de forma nativa consultando una base de datos SQL propia, para mantener el control interno de identidades sin depender de proveedores externos.

**Descripción / Contexto**

Implementar un flujo de autenticación local que consulte usuarios y credenciales almacenadas en SQL Server. El sistema debe rechazar integraciones con proveedores externos de autenticación como Google, Microsoft o Facebook.

**Ruta**

API: `/api/auth`
UI: `/login`, `/configuracion-perfil`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un usuario existe en la base SQL propia
Cuando ingresa credenciales correctas
Entonces el sistema permite continuar el flujo de autenticación.
Escenario: validación funcional
Dado que las credenciales son incorrectas
Cuando se intenta iniciar sesión
Entonces el sistema devuelve un error genérico sin revelar si el usuario existe.
Escenario: validación funcional
Dado que se configura un proveedor externo de autenticación
Cuando el sistema arranca o ejecuta login
Entonces la configuración es rechazada o no es utilizada.
Escenario: validación funcional
Dado que el usuario no existe
Cuando intenta iniciar sesión
Entonces el sistema no expone información sensible sobre la existencia de cuentas.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Usar base de datos SQL propia.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces No integrar Google, Microsoft, Facebook ni proveedores externos de identidad.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Almacenar contraseñas con hash seguro y salt.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Separar capa de aplicación, infraestructura y presentación siguiendo Clean Architecture.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de login con campos de usuario, contraseña, botón de ingresar y mensajes de error genéricos.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST`
- Ruta: `/api/auth/login`
- Request: Credenciales del usuario
- Response esperado: JWT corto o error 401
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: LoginHandler, CredentialValidator, UserRepository, AuthService.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, Roles, UserRoles, Credentials.

**Consideraciones Funcionales**

Implementar un flujo de autenticación local que consulte usuarios y credenciales almacenadas en SQL Server. El sistema debe rechazar integraciones con proveedores externos de autenticación como Google, Microsoft o Facebook.
Entregables:
- Endpoint o caso de uso de login nativo.
- Modelo de usuario compatible con RBAC.
- Validación de configuración que impida proveedores externos.
- Pruebas de autenticación exitosa, fallida y configuración inválida.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Autenticación y Control de Acceso.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
