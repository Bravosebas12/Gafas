# RF-CLI-02 Registro de Fórmula Optométrica

Como vendedor, quiero registrar fórmulas optométricas traídas por el cliente, para asociar datos de fórmula a las ventas de lentes.

**Descripción / Contexto**

Alcance: solo fórmulas optométricas traídas por el cliente. La fórmula debe incluir campos obligatorios: OD/OI, Esfera/SPH incluso 0.00/Neutro/Plano, Distancia Pupilar DP/DNP. Campos condicionales: Cilindro/CYL si hay astigmatismo; Eje/AXIS obligatorio solo si hay cilindro; Adición/ADD para cerca/bifocal/multifocal/progresivo; Prisma y Base para desalineación ocular clínica. Vendedor puede ver y registrar fórmula completa; Optómetra solo puede ver y dar recomendaciones.

**Ruta**

API: `/api/customers/{id}/optometric-formulas`, `/api/sales/orders/{id}/prescription`
UI: `/clientes/{id}/formulas`, `/pos`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que se registra una fórmula con OD/OI, Esfera/SPH y DP/DNP
Cuando se guarda
Entonces el sistema crea el registro de fórmula.
Escenario: validación funcional
Dado que se registra una fórmula con Cilindro/CYL
Cuando se guarda
Entonces Eje/AXIS se vuelve obligatorio.
Escenario: validación funcional
Dado que se registra una fórmula sin Cilindro/CYL
Cuando se guarda
Entonces Eje/AXIS es opcional.
Escenario: validación funcional
Dado que un Optómetra consulta una fórmula
Cuando accede al registro
Entonces solo puede ver la fórmula y agregar recomendaciones.
Escenario: validación funcional
Dado que un Vendedor consulta una fórmula
Cuando accede al registro
Entonces puede ver la fórmula completa y registrar nuevas.
Escenario: validación funcional
Dado que se vincula fórmula a una venta
Cuando se confirma
Entonces queda asociada al cliente y a la orden de trabajo.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Alcance limitado a fórmulas traídas por cliente.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Campos obligatorios: OD/OI, Esfera/SPH (incluye 0.00/Neutro/Plano), DP/DNP.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Cilindro/CYL condicional: si existe, Eje/AXIS es obligatorio.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Addición/ADD para cerca/bifocal/multifocal/progresivo.
Escenario: restricción técnica
Dado el contexto de implementación de Gestión de Clientes e Historial Clínico
Cuando se revisa la solución técnica
Entonces Prisma y Base para desalineación ocular clínica.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Ficha de cliente con sección de fórmulas optométricas y registro modal con OD/OI, Esfera/SPH, DP/DNP, y campos condicionales.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST/GET`
- Ruta: `/api/customers/{id}/optometric-formulas` y `/api/sales/orders/{id}/prescription`
- Request: Fórmula optométrica
- Response esperado: Fórmula registrada o historial de fórmulas
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: RegisterOptometricFormulaCommand, OptometricFormulaRepository, FormulaValidator.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Customers, OptometricFormulas, SalesOrders.

**Consideraciones Funcionales**

Alcance: solo fórmulas optométricas traídas por el cliente. Vendedor puede ver y registrar fórmulas completas; Optómetra solo puede ver y recomendar.
Entregables:
- Modelo de fórmula optométrica.
- Validador de campos obligatorios y condicionales.
- Comandos de registro.
- Consultas de historial por cliente.
- Pruebas de validación y permisos por rol.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Gestión de Clientes e Historial Clínico.
- Prioridad definida en la segmentación original: Crítica.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.