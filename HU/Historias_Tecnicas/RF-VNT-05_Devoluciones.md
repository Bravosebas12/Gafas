# RF-VNT-05 Devoluciones de Venta

Como vendedor o administrador, quiero registrar devoluciones de productos vendidos, para gestionar devoluciones de clientes.

**Descripción / Contexto**

Implementar proceso de devolución que permita devolver productos de una venta anterior. La devolución debe actualizar el estado de la orden, ajustar el inventario y registrar la transacción en el kárdex.

**Ruta**

API: `/api/sales/orders/{id}/returns`
UI: `/ventas/{id}`, `/devoluciones`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un cliente solicita devolución de un producto
Cuando se registra la devolución
Entonces el sistema actualiza el estado de la orden y devuelve productos al inventario.
Escenario: validación funcional
Dado que se registra una devolución
Cuando se procesa
Entonces se genera movimiento de kárdex de entrada.
Escenario: validación funcional
Dado que una venta tiene pagos parciales
Cuando se registra la devolución
Entonces se ajusta el saldo correspondiente.
Escenario: validación funcional
Dado que se consulta una venta devuelta
Cuando se abre el detalle
Entonces se muestra información de la devolución.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Validar permisos de vendedor o administrador.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Auditoría de devoluciones con tabla propia (antes/después).
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Consistencia transaccional entre devolución, orden y kárdex.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de registro de devolución con selección de productos, cantidades y observaciones.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST`
- Ruta: `/api/sales/orders/{id}/returns`
- Request: Detalles de devolución
- Response esperado: Devolución registrada
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: RegisterReturnCommand, ReturnRepository, KardexAdjustmentHandler.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, SaleItems, KardexMovements, Returns.

**Consideraciones Funcionales**

Implementar proceso de devolución que actualice estado de orden, ajuste inventario y registre movimiento en kárdex.
Entregables:
- Modelo de devolución.
- Comandos de registro.
- Ajuste automático de inventario.
- Pruebas de devolución y consistencia.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Media.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.