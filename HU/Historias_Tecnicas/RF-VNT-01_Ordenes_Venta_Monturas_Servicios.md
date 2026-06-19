# RF-VNT-01 Órdenes de Venta con Monturas y Servicios

Como vendedor, quiero crear órdenes de venta asociando monturas de inventario, lentes y servicios con stock, para procesar ventas completas del negocio.

**Descripción / Contexto**

Implementar creación de órdenes de venta que permitan agregar ítems de inventario tipo montura, lentes y servicios. Los servicios tienen stock. La orden debe calcular totales y, al confirmar el primer cobro, descontar inventario y pasar los productos a estado de elaboración/pendiente de entrega.

**Ruta**

API: `/api/sales/orders`
UI: `/pos`, `/ventas`, `/ventas/{id}`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se seleccionan monturas, lentes y servicios válidos
Cuando se crea la orden
Entonces el sistema genera una orden de venta.
Escenario: validación funcional
Dado que un ítem no tiene stock suficiente
Cuando se intenta agregar o confirmar
Entonces el sistema rechaza la confirmación por falta de stock.
Escenario: validación funcional
Dado que se confirma el primer cobro de una venta
Cuando se procesa
Entonces se descuentan existencias correspondientes y los productos pasan a estado de elaboración/pendiente de entrega.
Escenario: validación funcional
Dado que se consulta la orden
Cuando se abre el detalle
Entonces se muestran ítems, servicios, fórmula y totales.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Soportar monturas, lentes e inventario de servicios con stock.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Cálculo consistente de totales.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Impacto transaccional en kárdex.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Validar permisos de vendedor.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla POS con carrito de venta, selección de monturas, lentes, servicios con stock y totales.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/GET`
- Ruta: `/api/sales/orders`
- Request: Orden de venta
- Response esperado: Orden creada o consultada
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: CreateSalesOrderCommand, SalesOrderRepository, InventoryValidator.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, SaleItems, Frames, Lenses, Services, Prescriptions.

**Consideraciones Funcionales**

Implementar creación de órdenes de venta con monturas, lentes y servicios con stock. Al confirmar el primer cobro, se descuenta inventario y los productos pasan a elaboración/pendiente de entrega.
Entregables:
- Modelo de orden de venta.
- Comandos de creación y confirmación.
- Consultas de detalle de venta.
- Pruebas de cálculo e impacto de inventario.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
