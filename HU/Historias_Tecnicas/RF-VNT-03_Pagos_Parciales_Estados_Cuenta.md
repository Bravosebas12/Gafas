# RF-VNT-03 Pagos Parciales y Estados de Cuenta de Orden

Como vendedor o administrador, quiero registrar pagos parciales y abonos para órdenes de venta, para controlar saldos pendientes y estados de cuenta.

**Descripción / Contexto**

Implementar pagos parciales asociados a órdenes de venta. Cada abono debe afectar el estado de cuenta de la orden y permitir consultar saldo pendiente. Los pagos parciales/abonos están permitidos. Se puede entregar una orden con saldo pendiente.

**Ruta**

API: `/api/sales/orders/{id}/payments`
UI: `/pos`, `/ventas`, `/ventas/{id}`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que una orden tiene saldo pendiente
Cuando se registra un abono válido
Entonces el sistema reduce el saldo pendiente.
Escenario: validación funcional
Dado que se registra un abono parcial
Cuando se guarda
Entonces el producto sale del stock disponible y queda en estado de elaboración/pendiente de entrega; no se revierte por pagos parciales.
Escenario: validación funcional
Dado que se consulta una orden
Cuando se abre su estado de cuenta
Entonces se muestran abonos, total pagado y saldo pendiente.
Escenario: validación funcional
Dado que se entrega una orden con saldo pendiente
Cuando se procesa
Entonces el sistema permite la entrega con estado pendiente de pago.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Soportar pagos parciales y abonos.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Validar montos no negativos.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Registrar auditoría de pagos.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Mantener consistencia entre orden y pagos.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de estado de cuenta de orden con abonos, total pagado y saldo pendiente.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/GET`
- Ruta: `/api/sales/orders/{id}/payments`
- Request: Abono
- Response esperado: Estado de cuenta actualizado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: RegisterPaymentCommand, AccountStateService, PaymentRepository.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, Payments, PaymentMethods, AuditTrail.

**Consideraciones Funcionales**

Implementar pagos parciales asociados a órdenes de venta. Cada abono debe afectar el estado de cuenta de la orden y permitir consultar saldo pendiente. Los pagos parciales hacen que el producto salga del stock disponible y quede en elaboración/pendiente de entrega; no se revierte. Se puede entregar orden con saldo pendiente.
Entregables:
- Modelo de pago/abono.
- Comandos de registrar abono.
- Consulta de estado de cuenta.
- Pruebas de saldo y pago completo.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
