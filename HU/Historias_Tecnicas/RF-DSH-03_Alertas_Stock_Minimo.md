# RF-DSH-03 Alertas de Stock Mínimo

Como encargado de inventario, quiero recibir alertas críticas para productos con existencias por debajo del stock mínimo, para evitar quiebres de inventario.

**Descripción / Contexto**

Crear consulta que compare stock actual contra stock mínimo de monturas y lentes. El dashboard debe mostrar alertas críticas con información suficiente para actuar. Las alertas también deben mostrarse como notificación interna en la aplicación.

**Ruta**

API: `/api/dashboard/alerts`
UI: `/dashboard`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un producto tiene stock menor al mínimo
Cuando se carga el dashboard
Entonces aparece en la sección de alertas críticas.
Escenario: validación funcional
Dado que un producto tiene stock menor al mínimo
Cuando el usuario está en cualquier pantalla
Entonces aparece una notificación interna en la aplicación.
Escenario: validación funcional
Dado que un producto tiene stock igual o superior al mínimo
Cuando se carga el dashboard
Entonces no aparece como alerta crítica.
Escenario: validación funcional
Dado que no existen productos bajo mínimo
Cuando se consulta la sección
Entonces se muestra estado sin alertas.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Stock mínimo debe estar definido por producto.
Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Alerta basada en datos actualizados.
Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Priorizar consultas eficientes.
Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Incluir identificador, nombre, stock actual y stock mínimo.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Dashboard con panel de alertas críticas de stock mínimo y notificación interna visible en cualquier pantalla.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `GET`
- Ruta: `/api/dashboard/stock-alerts`
- Request: Sin parámetros
- Response esperado: Productos bajo mínimo
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: StockAlertQuery, InventoryRepository, StockAlertDto, NotificationService.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: InventoryItems, Frames, Lenses, StockLevels.

**Consideraciones Funcionales**

Crear consulta que compare stock actual contra stock mínimo de monturas y lentes. El dashboard debe mostrar alertas críticas; también debe haber notificación interna en la aplicación.
Entregables:
- Query CQRS de alertas de stock.
- DTO de alerta crítica.
- Componente de alertas en dashboard.
- Servicio de notificación interna.
- Pruebas de comparación stock vs mínimo.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Panel de Control.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
