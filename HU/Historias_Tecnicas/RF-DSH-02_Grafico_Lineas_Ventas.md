# RF-DSH-02 Gráfico de Ventas por Mes Actual

Como usuario del dashboard, quiero ver un gráfico de ventas del mes actual, para identificar tendencias comerciales.

**Descripción / Contexto**

Implementar una consulta que agrupe ventas por día del mes actual y un componente de gráfico compatible con Blazor WebAssembly. El gráfico debe actualizarse al cargar el dashboard.

**Ruta**

API: `/api/dashboard/charts`
UI: `/dashboard`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que existen ventas agrupadas por día del mes actual
Cuando se renderiza el dashboard
Entonces se muestra un gráfico con el comportamiento de ventas por día.
Escenario: validación funcional
Dado que no hay datos para el mes actual
Cuando se consulta
Entonces el gráfico muestra estado vacío.
Escenario: validación funcional
Dado que la consulta falla
Cuando se carga el dashboard
Entonces se muestra un mensaje de error controlado.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Gráfico dinámico en Blazor WebAssembly.
Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Datos ordenados cronológicamente por día del mes.
Escenario: restricción técnica
Dado el contexto de implementación de Panel de Control
Cuando se revisa la solución técnica
Entonces Separar datos y presentación.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Dashboard con gráfico de ventas por día del mes actual.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `GET`
- Ruta: `/api/dashboard/sales-by-month`
- Request: Sin parámetros (mes actual)
- Response esperado: Serie temporal de ventas del mes
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: SalesByMonthQuery, DashboardRepository, SalesTrendDto.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, SaleItems, Customers.

**Consideraciones Funcionales**

Implementar una consulta que agrupe ventas por día del mes actual y un componente de gráfico compatible con Blazor WebAssembly.
Entregables:
- Query CQRS de series temporales de ventas por día del mes.
- DTO de puntos para gráfico.
- Componente de gráfico de líneas.
- Pruebas de agrupación y ordenamiento.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Panel de Control.
- Prioridad definida en la segmentación original: Media.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
