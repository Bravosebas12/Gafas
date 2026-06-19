# RF-INV-01 Registro de Monturas

Como encargado de inventario, quiero registrar monturas parametrizadas por marca, modelo, SKU, color, tamaño, costo, precio, stock y stock mínimo, para mantener un catálogo comercial confiable.

**Descripción / Contexto**

Implementar CRUD de monturas con campos obligatorios y validaciones de negocio. Cada montura debe diferenciarse como tipo de ítem de inventario separado de lentes. El stock mínimo es obligatorio para generar alertas.

**Ruta**

API: `/api/inventory/frames`
UI: `/inventario`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se registran datos completos de una montura
Cuando se guarda
Entonces el sistema crea el ítem con todos los parámetros.
Escenario: validación funcional
Dado que falta un campo obligatorio
Cuando se intenta guardar
Entonces el sistema rechaza el registro.
Escenario: validación funcional
Dado que se registra un SKU duplicado
Cuando se intenta guardar
Entonces el sistema evita la duplicidad.
Escenario: validación funcional
Dado que se modifica una montura
Cuando se guarda
Entonces se conserva trazabilidad del cambio.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces SKU único.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Validar costo, precio y stock no negativos.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Stock mínimo obligatorio por producto.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Diferenciar monturas de lentes en modelo de dominio.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Persistencia SQL.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Formulario y listado de monturas con campos de marca, modelo, SKU, color, tamaño, costo, precio, stock y stock mínimo.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/PUT/GET`
- Ruta: `/api/inventory/frames`
- Request: Datos de montura
- Response esperado: Montura creada, actualizada o consultada
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: CreateFrameCommand, UpdateFrameCommand, FrameRepository, FrameDto.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Frames, InventoryItems, StockLevels.

**Consideraciones Funcionales**

Implementar CRUD de monturas con campos obligatorios y validaciones de negocio. Cada montura debe diferenciarse como tipo de ítem de inventario separado de lentes. El stock mínimo es obligatorio.
Entregables:
- Entidad y DTO de montura.
- Comandos de creación, actualización y desactivación.
- Consultas de listado y detalle.
- Pruebas de validación y duplicidad.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Gestión de Inventario Especializado.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
