# RF-VNT-07 Cambios de Producto en Venta

Como vendedor o administrador, quiero gestionar cambios de productos en ventas, para permitir modificar ítems de una orden no entregada.

**Descripción / Contexto**

Implementar proceso de cambio que permita modificar productos de una venta. El cambio debe actualizar el estado de la orden, ajustar el inventario (entrada del producto devuelto, salida del nuevo) y registrar la transacción en el kárdex.

**Ruta**

API: `/api/sales/orders/{id}/changes`
UI: `/ventas/{id}/cambios`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que una orden no está entregada
Cuando se registra un cambio de producto
Entonces el sistema actualiza el detalle y ajusta inventario.
Escenario: validación funcional
Dado que se registra un cambio
Cuando se procesa
Entonces se generan movimientos de kárdex: entrada del producto devuelto y salida del nuevo.
Escenario: validación funcional
Dado que se intenta cambiar productos de una orden entregada
Cuando se ejecuta
Entonces el sistema rechaza la operación.
Escenario: validación funcional
Dado que se consulta una orden con cambios
Cuando se abre el detalle
Entonces se muestra historial de cambios realizados.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Validar permisos de vendedor o administrador.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Auditoría de cambios con tabla propia (antes/después).
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Consistencia transaccional entre cambio, orden y kárdex.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de cambio con selección de productos a devolver y nuevos productos a agregar.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST`
- Ruta: `/api/sales/orders/{id}/changes`
- Request: Detalles de cambio
- Response esperado: Cambio registrado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: RegisterSaleChangeCommand, SaleChangeRepository, KardexAdjustmentHandler.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, SaleItems, KardexMovements, SaleChanges.

**Consideraciones Funcionales**

Implementar proceso de cambio que actualice orden, ajuste inventario con entradas y salidas, y registre movimientos en kárdex.
Entregables:
- Modelo de cambio de venta.
- Comandos de registro de cambio.
- Ajuste automático de inventario.
- Pruebas de cambio y consistencia.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Media.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.