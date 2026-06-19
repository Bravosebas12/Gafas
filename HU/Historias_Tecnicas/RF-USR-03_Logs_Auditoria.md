# RF-USR-03 Logs de Auditoría Inalterables

Como administrador o auditor interno, quiero registrar logs inalterables de transacciones críticas, para revisar cambios sensibles y detectar operaciones no autorizadas.

**Descripción / Contexto**

Implementar auditoría para operaciones críticas como login, cambios de usuario, cambios de roles, ajustes de inventario, ventas y fórmulas. Los logs no deben poder editarse ni eliminarse desde la aplicación. Cada tabla debe persistir UsuarioCreacion, FechaCreacion, UsuarioActualizacion, FechaActualizacion. Para cambios sensibles de inventario, clientes, ventas, pagos, fórmulas y servicios debe existir tabla propia de auditoría con antes/después.

**Ruta**

API: `/api/audit-logs`
UI: `/admin/auditoria`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que ocurre una transacción crítica
Cuando se ejecuta
Entonces se registra un log con usuario, fecha, acción y datos relevantes.
Escenario: validación funcional
Dado que se consulta un log
Cuando se filtra por transacción
Entonces se muestra la información auditada.
Escenario: validación funcional
Dado que un usuario intenta modificar o eliminar un log
Cuando lo intenta
Entonces el sistema lo impide; no existen endpoints UI/API para editar o eliminar logs.
Escenario: validación funcional
Dado que ocurre un error crítico
Cuando se registra
Entonces se conserva evidencia para auditoría.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Registro inalterable desde interfaz de aplicación.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Incluir usuario, fecha, entidad, acción y cambios.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Tabla de auditoría propia para cambios sensibles con antes/después (precio/cantidad/nombre de lente/montura/servicio/cliente/venta/pago/fórmula).
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Proteger logs contra edición no autorizada.
Escenario: restricción técnica
Dado el contexto de implementación de Administración de Usuarios Internos
Cuando se revisa la solución técnica
Entonces Cada tabla debe tener UsuarioCreacion, FechaCreacion, UsuarioActualizacion, FechaActualizacion.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de auditoría con filtros por usuario, fecha, entidad y acción. Solo visible para Administrador.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `GET`
- Ruta: `/api/audit-logs`
- Request: Filtros de auditoría
- Response esperado: Logs inalterables consultados
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: AuditLogService, AuditLogRepository, AuditLogQuery.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: AuditLogs, AuditTrail, Users, Entities.

**Consideraciones Funcionales**

Implementar auditoría para operaciones críticas. Los logs no deben poder editarse ni eliminarse desde la aplicación. Cada tabla debe tener campos de auditoría estándar; tablas sensibles deben tener auditoría propia con antes/después.
Entregables:
- Entidad de log de auditoría.
- Tablas de auditoría con antes/después para cambios sensibles.
- Servicio de auditoría transaccional.
- Consultas de auditoría.
- Pruebas de inmutabilidad y trazabilidad.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Administración de Usuarios Internos.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
