# RF-INV-03 Histórico de Movimientos de Kárdex

Como responsable de inventario, quiero registrar históricamente los movimientos de kárdex, para auditar entradas, salidas y ajustes de existencias.

**Descripción / Contexto**

Implementar un registro inmutable de movimientos de inventario asociado a monturas, lentes o servicios según corresponda. Cada movimiento debe afectar el stock de forma controlada. El kárdex debe registrar: fecha de transacción, concepto/justificación, número de comprobante/documento, cantidad entrada/salida, costo unitario, costo total.

**Ruta**

API: `/api/inventory/kardex`
UI: `/inventario`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se realiza una entrada de inventario
Cuando se registra
Entonces se crea un movimiento de kárdex con fecha, concepto, número de comprobante, cantidad entrada, costo unitario y costo total; aumenta el stock.
Escenario: validación funcional
Dado que se realiza una salida por venta
Cuando se registra
Entonces se crea un movimiento de kárdex y disminuye el stock.
Escenario: validación funcional
Dado que se consulta el historial
Cuando se filtra por producto
Entonces se muestran los movimientos ordenados por fecha.
Escenario: validación funcional
Dado que se intenta eliminar o alterar un movimiento histórico
Cuando se ejecuta
Entonces el sistema lo impide.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Registro histórico e inalterable.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Trazabilidad por usuario, fecha, tipo de movimiento, cantidad, costo unitario y costo total.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Consistencia transaccional entre stock y kárdex.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Auditoría de ajustes con tabla propia de auditoría (antes/después).
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de historial de kárdex con filtros por producto, tipo de movimiento y fecha. Muestra fecha de transacción, concepto, número de comprobante, cantidad entrada/salida, costo unitario y costo total.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/GET`
- Ruta: `/api/inventory/kardex`
- Request: Movimiento de inventario
- Response esperado: Movimiento registrado o historial
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: RegisterKardexMovementCommand, KardexRepository, KardexMovementDto.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: InventoryItems, KardexMovements, Users, AuditTrail.

**Consideraciones Funcionales**

Implementar un registro inmutable de movimientos de inventario. El kárdex debe registrar: fecha de transacción, concepto/justificación, número de comprobante/documento, cantidad entrada/salida, costo unitario, costo total.
Entregables:
- Entidad de movimiento de kárdex.
- Comandos transaccionales de entrada/salida/ajuste.
- Consultas de historial.
- Pruebas de consistencia e inmutabilidad.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Gestión de Inventario Especializado.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
