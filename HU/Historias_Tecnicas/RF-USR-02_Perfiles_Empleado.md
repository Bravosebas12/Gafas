# RF-USR-02 Perfil de Empleado con Contraseña Segura

Como administrador de la óptica, quiero crear perfiles de empleado con usuario, nombre, apellido y contraseña segura, para asegurar que los accesos correspondan al personal autorizado.

**Descripción / Contexto**

Cada usuario interno debe tener un perfil de empleado con usuario, nombre, apellido y contraseña. La contraseña debe guardarse con hash y salt. El sistema debe impedir usuarios huérfanos o vinculaciones duplicadas inválidas.

**Ruta**

API: `/api/users`, `/api/audit-logs`
UI: `/admin/usuarios`, `/admin/auditoria`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se crea un perfil de empleado con usuario, nombre, apellido y contraseña válidos
Cuando se guarda
Entonces la contraseña se almacena con hash y salt.
Escenario: validación funcional
Dado que se ingresa una contraseña que no cumple la política
Cuando se guarda
Entonces el sistema rechaza la operación.
Escenario: validación funcional
Dado que se intenta crear un usuario sin empleado
Cuando se guarda
Entonces el sistema rechaza la operación.
Escenario: validación funcional
Dado que se consulta un usuario
Cuando se obtiene el detalle
Entonces se muestra su perfil de empleado asociado.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Perfil con campos: usuario, nombre, apellido, contraseña.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Contraseña con hash + salt.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Política de contraseña: al menos una mayúscula, un número y un carácter especial.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Validar existencia y estado del empleado.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Auditoría de cambios de vinculación.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Formulario de usuario con selector obligatorio de perfil de empleado.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `PUT/GET`
- Ruta: `/api/users/{id}/employee-profile`
- Request: Identificador de empleado
- Response esperado: Vínculo usuario-empleado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: LinkEmployeeProfileCommand, EmployeeProfileRepository, UserValidator.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, EmployeeProfiles.

**Consideraciones Funcionales**

Cada usuario interno debe tener un perfil de empleado con usuario, nombre, apellido y contraseña. La contraseña debe guardarse con hash y salt. El sistema debe impedir usuarios huérfanos o vinculaciones duplicadas inválidas.
Entregables:
- Modelo de perfil de empleado.
- Validación de vinculación usuario-empleado.
- Comandos de asignación y actualización.
- Pruebas de relaciones válidas e inválidas.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Administración de Usuarios Internos.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
