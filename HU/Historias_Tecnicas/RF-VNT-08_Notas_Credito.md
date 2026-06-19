# RF-VNT-08 Notas de Crédito

Como administrador, quiero generar notas de crédito para ventas, para gestionar compensaciones monetarias sin devolución de productos.

**Descripción / Contexto**

Implementar proceso de nota de crédito que permita otorgar un valor monetario a una venta. La nota debe actualizar el estado de la orden, ajustar el saldo y registrar la transacción con auditoría.

**Ruta**

API: `/api/sales/orders/{id}/credit-notes`
UI: `/ventas/{id}/notas-credito`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se genera una nota de crédito válida
Cuando se registra
Entonces el sistema actualiza el saldo de la orden.
Escenario: validación funcional
Dado que una venta tiene saldo pendiente
Cuando se aplica nota de crédito
Entonces se reduce el saldo correspondiente.
Escenario: validación funcional
Dado que se consulta una venta con notas de crédito
Cuando se abre el detalle
Entonces se muestra historial de notas aplicadas.
Escenario: validación funcional
Dado que se intenta generar nota de crédito sin autorización
Cuando se ejecuta
Entonces el sistema rechaza la operación.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Solo administradores pueden generar notas de crédito.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Auditoría de notas de crédito con tabla propia (antes/después).
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Consistencia transaccional entre nota, orden y saldo.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de generación de nota de crédito con valor y motivo.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST`
- Ruta: `/api/sales/orders/{id}/credit-notes`
- Request: Detalles de nota de crédito
- Response esperado: Nota registrada
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: CreateCreditNoteCommand, CreditNoteRepository, BalanceAdjustmentHandler.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, CreditNotes, AuditTrail.

**Consideraciones Funcionales**

Implementar proceso de nota de crédito que actualice orden y saldo, con auditoría completa.
Entregables:
- Modelo de nota de crédito.
- Comandos de generación.
- Ajuste automático de saldo.
- Pruebas de generación y consistencia.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Media.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.