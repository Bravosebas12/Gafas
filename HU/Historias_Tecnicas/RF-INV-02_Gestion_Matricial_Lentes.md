# RF-INV-02 Gestión Matricial de Lentes

Como encargado de inventario clínico, quiero gestionar lentes mediante una matriz basada en marca y filtro, para controlar correctamente la fabricación y existencia de lentes.

**Descripción / Contexto**

Modelar lentes con atributos de marca y filtro. Filtro valores: Blue (azul), Antireflejo (antirreflectante), Normal. Marca puede ser Transitions u otra marca. No se usa material ni tratamiento. La gestión debe permitir registrar, consultar y actualizar combinaciones técnicas de lentes.

**Ruta**

API: `/api/inventory/lenses`
UI: `/inventario`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se registra un lente con marca y filtro (Blue/Antireflejo/Normal)
Cuando se guarda
Entonces el sistema almacena la matriz técnica.
Escenario: validación funcional
Dado que se consulta un lente por combinación técnica
Cuando se busca
Entonces se devuelve el registro correspondiente.
Escenario: validación funcional
Dado que se modifica la marca o filtro
Cuando se guarda
Entonces se actualiza la ficha del lente.
Escenario: validación funcional
Dado que falta información técnica obligatoria
Cuando se intenta guardar
Entonces el sistema rechaza el registro.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Diferenciar lentes de monturas.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Valores de filtro soportados: Blue, Antireflejo, Normal.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Marca puede ser Transitions u otra; no usar material ni tratamiento.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Stock mínimo obligatorio por lente.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Inventario Especializado
Cuando se revisa la solución técnica
Entonces Preparar datos para venta y fabricación.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Formulario matricial de lentes con marca y filtro (Blue, Antireflejo, Normal).

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/PUT/GET`
- Ruta: `/api/inventory/lenses`
- Request: Datos técnicos del lente (marca y filtro)
- Response esperado: Lente creado, actualizado o consultado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: CreateLensCommand, LensMatrixService, LensRepository, LensDto.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Lenses, LensFilters, StockLevels.

**Consideraciones Funcionales**

Modelar lentes con marca y filtro (Blue, Antireflejo, Normal). Marca puede ser Transitions u otra. No usar material ni tratamiento.
Entregables:
- Modelo de lente con matriz técnica.
- Catálogo de filtros de lentes.
- Comandos y consultas de lentes.
- Pruebas de combinación técnica.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Gestión de Inventario Especializado.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
