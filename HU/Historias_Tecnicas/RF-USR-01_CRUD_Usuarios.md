# RF-USR-01 CRUD y Desactivación Lógica de Usuarios

Como administrador, quiero crear, modificar y desactivar lógicamente usuarios internos, para administrar el acceso del personal sin perder el historial de operaciones.

**Descripción / Contexto**

Implementar CRUD de usuarios internos con desactivación lógica en lugar de eliminación física. El usuario desactivado no debe poder iniciar sesión ni ejecutar operaciones.

**Ruta**

API: `/api/users`, `/api/audit-logs`
UI: `/admin/usuarios`, `/admin/auditoria`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un administrador crea un usuario con datos válidos
Cuando se guarda
Entonces el usuario queda activo y disponible.
Escenario: validación funcional
Dado que un administrador modifica datos de un usuario activo
Cuando se guarda
Entonces los cambios se reflejan.
Escenario: validación funcional
Dado que un administrador desactiva un usuario
Cuando el usuario intenta iniciar sesión
Entonces el acceso es rechazado.
Escenario: validación funcional
Dado que se consulta el historial de transacciones
Cuando el usuario fue desactivado
Entonces sus registros históricos permanecen visibles.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces No eliminar físicamente usuarios.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Validar datos obligatorios.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Desactivación afecta login y permisos.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Auditoría de cambios.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla CRUD de usuarios con botón de desactivación lógica.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/PUT/DELETE`
- Ruta: `/api/users`
- Request: Datos de usuario
- Response esperado: Usuario activo, actualizado o desactivado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: CreateUserCommand, UpdateUserCommand, DeactivateUserCommand, UserRepository.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, EmployeeProfiles, Roles.

**Consideraciones Funcionales**

Implementar CRUD de usuarios internos con desactivación lógica en lugar de eliminación física. El usuario desactivado no debe poder iniciar sesión ni ejecutar operaciones.
Entregables:
- Comandos de crear, actualizar y desactivar usuario.
- Consultas de listado y detalle.
- Estado activo/inactivo en modelo.
- Pruebas de desactivación lógica.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Administración de Usuarios Internos.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
