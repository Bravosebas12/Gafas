# RF-CLI-01 Ficha de Registro de Clientes

Como vendedor u optómetra, quiero registrar clientes con datos de contacto, para identificar correctamente a cada persona y vincular sus compras y registros clínicos.

**Descripción / Contexto**

Implementar ficha de cliente con tipo de identificación, número de identificación, nombre y teléfono obligatorios. Email y dirección son opcionales. El número de identificación debe ser único. El cliente debe ser reutilizable en ventas, historial clínico y estados de cuenta.

**Ruta**

API: `/api/customers`, `/api/customers/{id}/optometric-formulas`, `/api/customers/{id}/commercial-summary`
UI: `/clientes`, `/clientes/{id}`, `/clientes/{id}/historial-clinico`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se registran todos los campos obligatorios (tipo identificación, número identificación, nombre, teléfono)
Cuando se guarda
Entonces el sistema crea la ficha de cliente.
Escenario: validación funcional
Dado que se intenta registrar un número de identificación duplicado
Cuando se guarda
Entonces el sistema rechaza la operación.
Escenario: validación funcional
Dado que se busca un cliente
Cuando se consulta por nombre, teléfono o documento
Entonces se muestran resultados coincidentes.
Escenario: validación funcional
Dado que se consulta una ficha
Cuando se abre
Entonces se muestran datos de contacto y vínculos comerciales/clínicos.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Tipo de identificación obligatorio.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Número de identificación obligatorio y único.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Nombre obligatorio.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Teléfono obligatorio; Email y dirección opcionales.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Cliente vinculado a ventas, fórmula y estado de cuenta.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Formulario y ficha de cliente con tipo identificación, número identificación único, nombre, teléfono, email y dirección.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/PUT/GET`
- Ruta: `/api/customers`
- Request: Datos del cliente
- Response esperado: Cliente creado, actualizado o consultado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: CreateCustomerCommand, CustomerRepository, CustomerValidator.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Customers, ContactInfo.

**Consideraciones Funcionales**

Implementar ficha de cliente con campos obligatorios (tipo identificación, número identificación único, nombre, teléfono) y opcionales (email, dirección). El cliente debe ser reutilizable en ventas, fórmulas y estados de cuenta.
Entregables:
- Modelo de cliente.
- Comandos de creación y actualización.
- Consultas de búsqueda y detalle.
- Pruebas de validación y búsqueda.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Gestión de Clientes e Historial Clínico.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
