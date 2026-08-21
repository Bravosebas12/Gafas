# RF-VNT-04 Multi-método de Pago y Ticket Interno

Como vendedor, quiero registrar pagos con múltiples métodos y generar tickets internos, para cerrar ventas de forma clara y cumplida.

**Descripción / Contexto**

Implementar soporte para pagos con distintos métodos y generación de ticket de venta. Métodos de pago: efectivo, transferencia, Nequi, tarjeta. La liquidación debe ser consistente con los totales de la orden. Impuestos fuera de esta fase.

**Ruta**

API: `/api/sales/orders/{id}/payments`, `/api/sales/orders/{id}/ticket`
UI: `/pos`, `/ventas`, `/ventas/{id}`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que una venta usa uno o varios métodos de pago
Cuando se registra el pago
Entonces el sistema distribuye correctamente los montos.
Escenario: validación funcional
Dado que la venta se confirma
Cuando se genera ticket
Entonces el documento incluye ítems, pagos y totales.
Escenario: validación funcional
Dado que los pagos no cubren el total requerido
Cuando se intenta cerrar
Entonces el sistema permite el cierre con estado pendiente de pago.
Escenario: validación funcional
Dado que se usa método de pago válido (efectivo, transferencia, Nequi, tarjeta)
Cuando se registra
Entonces el sistema acepta el método.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Multi-método de pago.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Generación de ticket interno.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces No calcular impuestos en esta fase.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Validar consistencia entre pagos y total.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Modal de pago con métodos múltiples (efectivo, transferencia, Nequi, tarjeta), total y opción de generar ticket.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST`
- Ruta: `/api/sales/orders/{id}/payments` y `/api/sales/orders/{id}/ticket`
- Request: Métodos de pago
- Response esperado: Pago liquidado y ticket generado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: RegisterPaymentCommand, TicketGenerator, PaymentRepository.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, Payments, PaymentMethods, Tickets.

**Consideraciones Funcionales**

Implementar soporte para pagos con múltiples métodos (efectivo, transferencia, Nequi, tarjeta) y generación de ticket interno. Impuestos fuera de esta fase.
Entregables:
- Modelo de pago con método.
- Generador de ticket.
- Pruebas de liquidación y ticket.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
