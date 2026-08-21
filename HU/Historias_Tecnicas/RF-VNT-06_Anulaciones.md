# RF-VNT-06 Anulaciones de Venta

Como administrador, quiero anular ventas en estados permitidos, para gestionar operaciones no completadas.

**Descripción / Contexto**

Implementar proceso de anulación que permita cancelar una venta en estado permitido. La anulación debe actualizar el estado de la orden, devolver productos al inventario cuando aplique y registrar la transacción en el kárdex.

**Ruta**

API: `/api/sales/orders/{id}/void`
UI: `/ventas/{id}`, `/anulaciones`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que una venta está en estado pendiente
Cuando se anula
Entonces el sistema actualiza el estado a anulado y devuelve productos al inventario cuando aplique.
Escenario: validación funcional
Dado que se anula una venta
Cuando se procesa
Entonces se genera movimiento de kárdex de entrada.
Escenario: validación funcional
Dado que se intenta anular una venta entregada
Cuando se ejecuta
Entonces el sistema rechaza la operación.
Escenario: validación funcional
Dado que se consulta una venta anulada
Cuando se abre el detalle
Entonces se muestra el estado anulado y fecha de anulación.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Solo administradores pueden anular ventas.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Auditoría de anulaciones con tabla propia (antes/después).
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Consistencia transaccional entre anulación, orden y kárdex.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de anulación con confirmación de la operación.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `DELETE`
- Ruta: `/api/sales/orders/{id}/void`
- Request: Identificador de venta
- Response esperado: Venta anulada
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: VoidSaleCommand, SaleRepository, KardexAdjustmentHandler.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, KardexMovements, AuditTrail.

**Consideraciones Funcionales**

Implementar proceso de anulación que actualice estado de orden, devuelva inventario cuando aplique y registre movimiento en kárdex.
Entregables:
- Modelo de anulación.
- Comando de anulación.
- Ajuste automático de inventario.
- Pruebas de anulación y consistencia.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Media.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.