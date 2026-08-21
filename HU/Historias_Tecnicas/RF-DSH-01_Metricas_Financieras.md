# RF-DSH-01 Métricas de Volumen de Ventas del Dashboard

Como gerente o administrador de la óptica, quiero visualizar métricas de volumen de ventas en el dashboard, para identificar tendencias comerciales.

**Descripción / Contexto**

Construir consultas optimizadas que muestren el volumen de ventas, entendido como cantidad de órdenes pendientes de pago total. El frontend debe presentar los KPIs en el dashboard como visualización aparte del acto de vender.

**Ruta**

API: `/api/dashboard/metrics`
UI: `/dashboard`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que existen órdenes pendientes de pago total
Cuando se abre el dashboard
Entonces se muestra el conteo de órdenes pendientes.
Escenario: validación funcional
Dado que no existen órdenes pendientes
Cuando se consulta el dashboard
Entonces se muestra cero sin errores.
Escenario: validación funcional
Dado que las órdenes cambian de estado
Cuando se recalcula
Entonces el conteo se actualiza automáticamente.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Volumen de ventas = cantidad de órdenes pendientes de pago total.
Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Consultas eficientes para dashboard.
Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Separar consulta de métricas en Application usando CQRS.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Dashboard con tarjeta que muestra volumen de ventas (órdenes pendientes de pago total).

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `GET`
- Ruta: `/api/dashboard/sales-volume`
- Request: Sin parámetros
- Response esperado: KPI de volumen de ventas
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: SalesVolumeQuery, DashboardRepository, SalesVolumeDto.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders.

**Consideraciones Funcionales**

Visualizar métricas de volumen de ventas, entendido como cantidad de órdenes pendientes de pago total, en el dashboard como visualización aparte del acto de vender.
Entregables:
- Query CQRS para volumen de ventas.
- DTO de KPI de ventas.
- Componente de dashboard para volumen.
- Pruebas de cálculo y casos sin datos.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Panel de Control.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
