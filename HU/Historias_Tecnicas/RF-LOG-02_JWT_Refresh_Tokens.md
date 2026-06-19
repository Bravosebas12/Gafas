# RF-LOG-02 JWT Corto y Refresh Tokens Seguros

Como usuario autenticado del sistema, quiero recibir un JWT de corta duración con refresh token almacenado de forma segura, para operar con sesiones protegidas y renovar acceso sin exponer credenciales.

**Descripción / Contexto**

Emitir tokens JWT de corta duración y gestionar refresh tokens con almacenamiento seguro, idealmente hasheados en SQL y asociados al usuario. El refresh token debe permitir renovación controlada y revocación.

**Ruta**

API: `/api/auth`
UI: `/login`, `/configuracion-perfil`

**Criterios de Aceptación Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: validación funcional
Dado que el login es exitoso
Cuando se responde al cliente
Entonces se entrega un JWT de corta duración.
Escenario: validación funcional
Dado que el JWT expira
Cuando el cliente presenta un refresh token válido
Entonces se emite un nuevo par de tokens.
Escenario: validación funcional
Dado que el refresh token está vencido, revocado o no pertenece al usuario
Cuando se intenta renovar sesión
Entonces se rechaza la renovación.
Escenario: validación funcional
Dado que se rota un refresh token
Cuando se usa el token anterior
Entonces el sistema lo rechaza.

**Criterios de Aceptación no Funcionales**

Estos son los criterios mínimos que debe cumplir la historia para ser considerada completa y aprobada.

Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces JWT con expiración corta.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Refresh token almacenado de forma segura, preferiblemente hasheado.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Soporte para revocación y rotación.
Escenario: restricción técnica
Dado el contexto de implementación de Autenticación y Control de Acceso
Cuando se revisa la solución técnica
Entonces Los tokens no deben contener información sensible innecesaria.
Escenario: seguridad y trazabilidad
Dado que la historia pertenece al sistema de óptica
Cuando un usuario ejecuta una operación sensible
Entonces la acción se valida por rol y se registra en auditoría cuando aplica
Escenario: rendimiento y usabilidad
Dado que la historia se consume desde Blazor WebAssembly
Cuando se solicita información al backend
Entonces la respuesta debe ser eficiente y mostrar estados de carga o error controlados

**Prototipo/Mockup (Historias de GUI)**

Flujo silencioso de renovación de sesión; no requiere mockup visible salvo manejo de redirección a login cuando expira la sesión.

**Estructura de peticiones y respuestas (Swagger) (Historias de Servicios)**

- Método: `POST`
- Ruta: `/api/auth/refresh`
- Request: Refresh token válido
- Response esperado: Nuevo JWT y refresh token
- Response 400: errores de validación de negocio.
- Response 401/403: usuario no autenticado o sin permiso.

**Diagramas C4 (Cuatro Niveles)**

Nivel 1 - Sistema: Sistema Integral de Gestión de Inventario y Punto de Venta para Óptica.
Nivel 2 - Contenedores: Optica.Client.Blazor, Optica.API, Optica.Infrastructure y base de datos SQL.
Nivel 3 - Componentes: JwtTokenService, RefreshTokenService, TokenRepository.
Nivel 4 - Código: entidades, value objects, handlers de MediatR, validadores, repositorios y endpoints correspondientes.

**MER - Modelo entidad relación**

Entidades principales: Users, RefreshTokens, JWTClaims.

**Consideraciones Funcionales**

Emitir tokens JWT de corta duración y gestionar refresh tokens con almacenamiento seguro, idealmente hasheados en SQL y asociados al usuario. El refresh token debe permitir renovación controlada y revocación.
Entregables:
- Servicio de emisión y validación de JWT.
- Servicio de refresh token.
- Modelo de persistencia para refresh tokens.
- Pruebas de expiración, renovación, revocación y rotación.

**Notas Adicionales**

- Fuente: Especificación_Requerimientos_Optica_Blazor_v1.1.pdf, sección relacionada con Autenticación y Control de Acceso.
- Prioridad definida en la segmentación original: Alta.
- Esta historia sigue la plantilla de Historia Técnica proporcionada.
