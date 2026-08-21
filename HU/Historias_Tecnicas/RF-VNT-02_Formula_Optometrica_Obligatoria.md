# RF-VNT-02 Fórmula Optométrica Obligatoria para Lentes

Como vendedor u optómetra, quiero que el sistema exija la fórmula optométrica cuando una venta incluya lentes, para garantizar que la fabricación se realice con datos clínicos correctos y trazables.

**Descripción / Contexto**

Validar que toda orden de venta que incluya lentes tenga una fórmula optométrica diligenciada. La fórmula debe guardarse y vincularse al historial del cliente y a la orden de trabajo correspondiente. Alcance: solo fórmulas traídas por el cliente. Vendedor puede ver fórmula completa y registrarla; Optómetra solo puede ver y dar recomendaciones.

**Ruta**

API: `/api/sales/orders/{id}/prescription`
UI: `/pos`, `/ventas/{id}`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que una orden incluye lentes
Cuando se intenta confirmar
Entonces el sistema exige una fórmula optométrica completa.
Escenario: validación funcional
Dado que no existe fórmula optométrica
Cuando se intenta confirmar la orden con lentes
Entonces el sistema bloquea la confirmación.
Escenario: validación funcional
Dado que se guarda una fórmula
Cuando se confirma la venta
Entonces queda vinculada al historial del cliente y a la orden de trabajo.
Escenario: validación funcional
Dado que se consulta la orden
Cuando se abre el detalle
Entonces se muestra la fórmula asociada.
Escenario: validación funcional
Dado que un Optómetra accede a una fórmula
Cuando intenta registrar una nueva fórmula
Entonces el sistema solo permite ver fórmulas existentes y agregar recomendaciones.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Regla obligatoria solo cuando la venta incluye lentes.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Fórmula optométrica inalterable o con versión histórica.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Vinculación a cliente-orden-fórmula.
Escenario: restricción técnica
Dado el contexto de implementación de Punto de Venta
Cuando se revisa la solución técnica
Entonces Validación backend, no solo frontend.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Modal de fórmula optométrica obligatorio antes de confirmar venta con lentes.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `PUT`
- Ruta: `/api/sales/orders/{id}/prescription`
- Request: Fórmula optométrica
- Response esperado: Fórmula vinculada a orden y cliente
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: AttachPrescriptionCommand, PrescriptionValidator, WorkOrderLinker.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: SalesOrders, Prescriptions, Customers, WorkOrders.

**Consideraciones Funcionales**

Validar que toda orden de venta que incluya lentes tenga una fórmula optométrica diligenciada. La fórmula debe guardarse y vincularse al historial del cliente y a la orden de trabajo correspondiente. Vendedor puede ver y registrar fórmula completa; Optómetra solo puede ver y recomendar.
Entregables:
- Modelo de fórmula optométrica.
- Validador de venta con lentes.
- Vínculos cliente-orden-fórmula.
- Pruebas de bloqueo y vinculación.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Punto de Venta.
- Prioridad definida en la segmentación original: Crítica.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
