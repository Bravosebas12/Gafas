# RF-CLI-03 Consolidado Comercial para Vendedor

Como vendedor, quiero ver un consolidado comercial del cliente con su histórico de compras, para atender mejor al cliente y conocer su relación comercial con la óptica.

**Descripción / Contexto**

Construir una vista consolidada del cliente que agrupe compras, órdenes, saldos y datos relevantes visibles para el rol vendedor.

**Ruta**

API: `/api/customers/{id}/commercial-summary`
UI: `/clientes`, `/clientes/{id}`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un cliente tiene compras previas
Cuando se consulta su consolidado
Entonces se muestra el histórico de compras.
Escenario: validación funcional
Dado que el cliente tiene saldos o estados de cuenta
Cuando se consulta
Entonces se muestra información comercial relevante.
Escenario: validación funcional
Dado que un vendedor no tiene permiso clínico
Cuando abre el consolidado
Entonces solo ve información comercial autorizada.
Escenario: validación funcional
Dado que el cliente no tiene compras
Cuando se consulta
Entonces se muestra estado vacío sin errores.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Visibilidad limitada por rol vendedor.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces No exponer información clínica sensible si no está autorizada.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Consultas eficientes por cliente.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Integrar datos de ventas y pagos.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Ficha de cliente con pestaña comercial, histórico de compras y saldos.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `GET`
- Ruta: `/api/customers/{id}/commercial-summary`
- Request: Identificador de cliente
- Response esperado: Consolidado comercial
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: CommercialSummaryQuery, CustomerSalesRepository, BalanceDto.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Customers, SalesOrders, Payments, Balances.

**Consideraciones Funcionales**

Construir una vista consolidada del cliente que agrupe compras, órdenes, saldos y datos relevantes visibles para el rol vendedor.
Entregables:
- Query CQRS de consolidado comercial.
- DTO de histórico de compras y saldos.
- Vista de cliente para vendedor.
- Pruebas de permisos y datos vacíos.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Gestión de Clientes e Historial Clínico.
- Prioridad definida en la segmentación original: Media.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
