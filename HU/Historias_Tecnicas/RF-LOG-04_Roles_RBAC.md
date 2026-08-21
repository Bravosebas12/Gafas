# RF-LOG-04 Roles Administrador, Vendedor y Optómetra

Como administrador del negocio, quiero asignar roles estrictos de Administrador, Vendedor y Optómetra, para controlar qué funcionalidades puede usar cada usuario.

**Descripción / Contexto**

Implementar RBAC con roles estrictos. Un usuario puede tener uno o varios roles. Solo el Administrador puede asignar o quitar roles. El backend debe validarlos en consultas y comandos críticos.

**Ruta**

API: `/api/roles`, `/api/users/{id}/roles`
UI: `/admin/usuarios`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un usuario tiene rol Administrador
Cuando accede al sistema
Entonces puede usar todas las funcionalidades del sistema.
Escenario: validación funcional
Dado que un usuario tiene rol Vendedor
Cuando accede al sistema
Entonces puede usar inventario y ventas autorizadas.
Escenario: validación funcional
Dado que un usuario tiene rol Optómetra
Cuando accede al sistema
Entonces puede usar reportes de fórmulas y recomendaciones de fórmula autorizadas.
Escenario: validación funcional
Dado que un usuario no tiene permiso para una operación
Cuando intenta ejecutarla
Entonces el sistema rechaza la acción.
Escenario: validación funcional
Dado que un usuario no es Administrador
Cuando intenta asignar o quitar roles
Entonces el sistema rechaza la operación.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Roles permitidos: Administrador, Vendedor, Optómetra.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Validar permisos en backend, no solo en UI.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Los roles deben estar vinculados al perfil de empleado.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Auditoría de cambios de rol.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de administración de usuarios con selector de rol: Administrador, Vendedor y Optómetra. Solo visible para Administrador.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `GET/PUT`
- Ruta: `/api/roles`, `/api/users/{id}/roles`
- Request: Asignación o consulta de roles
- Response esperado: Roles aplicados o error 403
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: RoleService, PermissionEvaluator, AuthorizationPolicies.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, Roles, UserRoles.

**Consideraciones Funcionales**

Implementar RBAC con roles estrictos. Un usuario puede tener uno o varios roles. Solo el Administrador puede asignar o quitar roles. El backend debe validarlos en consultas y comandos críticos.
Entregables:
- Catálogo de roles.
- Claims o permisos asociados a cada rol.
- Autorización en API y Blazor.
- Pruebas de acceso por rol.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Autenticación y Control de Acceso.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
