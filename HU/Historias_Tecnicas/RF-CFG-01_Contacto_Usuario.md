# RF-CFG-01 Actualización de Información de Contacto

Como usuario autenticado, quiero actualizar mi información básica de contacto, para mantener mis datos personales correctos dentro del sistema.

**Descripción / Contexto**

Implementar autogestión de perfil para el usuario autenticado. Solo el usuario propietario debe poder modificar sus datos básicos de contacto.

**Ruta**

API: `/api/profile`
UI: `/configuracion-perfil`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un usuario autenticado actualiza su contacto
Cuando guarda
Entonces los datos se actualizan en su perfil.
Escenario: validación funcional
Dado que un usuario intenta modificar datos de otro usuario
Cuando lo intenta
Entonces el sistema rechaza la operación.
Escenario: validación funcional
Dado que se ingresan datos inválidos
Cuando se guarda
Entonces el sistema muestra errores de validación.
Escenario: validación funcional
Dado que se actualiza la información
Cuando se consulta el perfil
Entonces se muestran los datos actualizados.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces Solo usuarios autenticados.
Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces Validar formato de contacto.
Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces No permitir cambios de identidad o rol desde este flujo.
Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces Auditoría de cambios relevantes.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de configuración personal con datos de contacto editables.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `PUT/GET`
- Ruta: `/api/profile/contact`
- Request: Datos de contacto
- Response esperado: Perfil actualizado
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: UpdateContactCommand, ProfileRepository, ProfileDto.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, ContactInfo.

**Consideraciones Funcionales**

Implementar autogestión de perfil para el usuario autenticado. Solo el usuario propietario debe poder modificar sus datos básicos de contacto.
Entregables:
- Endpoint o comando de actualización de contacto.
- Formulario de configuración personal.
- Validaciones de entrada.
- Pruebas de autorización y validación.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Configuración Personal.
- Prioridad definida en la segmentación original: Media.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
