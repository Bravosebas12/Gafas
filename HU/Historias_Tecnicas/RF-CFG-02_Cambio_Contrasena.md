# RF-CFG-02 Cambio de Contraseña con Política Fuerte

Como usuario autenticado, quiero cambiar mi contraseña aplicando una política de complejidad fuerte, para proteger mi cuenta frente a accesos no autorizados.

**Descripción / Contexto**

Implementar cambio de contraseña verificando la contraseña actual y aplicando reglas de complejidad fuerte para la nueva contraseña. La nueva contraseña debe guardarse hasheada.

**Ruta**

API: `/api/profile`
UI: `/configuracion-perfil`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que un usuario ingresa su contraseña actual correcta y una nueva contraseña válida
Cuando cambia la contraseña
Entonces el sistema actualiza la credencial.
Escenario: validación funcional
Dado que la contraseña actual es incorrecta
Cuando se intenta cambiar
Entonces el sistema rechaza la operación.
Escenario: validación funcional
Dado que la nueva contraseña no cumple la política fuerte
Cuando se intenta guardar
Entonces el sistema rechaza el cambio.
Escenario: validación funcional
Dado que se cambia la contraseña
Cuando el usuario inicia sesión nuevamente
Entonces la nueva contraseña es válida.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces Política de contraseña: debe contener al menos una mayúscula, un número y un carácter especial.
Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces Hash seguro con salt.
Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces No mostrar contraseñas.
Escenario: restricción técnica
Dado el contexto de implementación de Configuración Personal
Cuando se revisa la solución técnica
Entonces Auditoría de cambio de contraseña.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde la interfaz Blazor Web App
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Pantalla de cambio de contraseña con validación de contraseña actual y política fuerte.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `PUT`
- Ruta: `/api/profile/password`
- Request: Contraseña actual y nueva contraseña
- Response esperado: Contraseña actualizada
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Web (Blazor Web App con render en servidor y endpoints HTTP), Optica.Web.Client (islas interactivas WebAssembly), Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: ChangePasswordCommand, PasswordPolicyValidator, CredentialRepository.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, Credentials.

**Consideraciones Funcionales**

Implementar cambio de contraseña verificando la contraseña actual y aplicando reglas de complejidad fuerte para la nueva contraseña. La nueva contraseña debe guardarse hasheada.
Entregables:
- Comando de cambio de contraseña.
- Validador de política fuerte.
- Formulario de configuración personal.
- Pruebas de contraseña válida e inválida.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Configuración Personal.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
