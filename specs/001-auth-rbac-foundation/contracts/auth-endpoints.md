# Contratos — Autenticación (`/api/auth`)

**Feature**: 001-auth-rbac-foundation
**Base**: rutas declaradas en RF-LOG-01 y RF-LOG-02. Hospedadas en `Optica.Web`, no en un proyecto
de API separado ([ADR-001](../../../docs/adr/ADR-001-modelo-presentacion-blazor-web-app.md)).

## Convenciones comunes

- Todas las respuestas de error usan el formato de problema de HTTP (`application/problem+json`).
- El token de acceso y la credencial de renovación **no aparecen en ningún cuerpo de respuesta**.
  Viajan en cookies `HttpOnly; Secure; SameSite=Strict` (decisión D-04 en
  [research.md](../research.md)).
- Los endpoints que modifican estado exigen token antifalsificación, consecuencia obligada de
  autenticar por cookie.
- Ningún mensaje de error distingue entre usuario inexistente, contraseña incorrecta, cuenta
  inactiva y cuenta bloqueada (FR-004).

| Cookie | Contenido | Vida |
|---|---|---|
| `optica_at` | JWT de acceso firmado | 15 minutos |
| `optica_rt` | Credencial opaca de 256 bits | 8 horas |

---

## `POST /api/auth/login`

Autentica con credenciales propias y abre sesión. **Anónimo.**

**Petición**

```json
{
  "nombreUsuario": "jperez",
  "contrasena": "········"
}
```

| Campo | Regla de validación |
|---|---|
| `nombreUsuario` | Obligatorio, 1 a 100 caracteres |
| `contrasena` | Obligatorio, **máximo 12 caracteres** (FR-003a). Se acepta cualquier carácter, incluidos espacios, acentos y no latinos. El mínimo de 8 **no** se valida aquí: una contraseña más corta se rechaza como credencial incorrecta con 401, no con 400, para no revelar la política ni romper el mensaje genérico de FR-004 |

**Respuesta 200**

```json
{
  "expiraEn": "2026-08-20T15:45:00Z",
  "sesionExpiraEn": "2026-08-20T23:30:00Z",
  "roles": ["Vendedor"]
}
```

Más las cabeceras `Set-Cookie` de `optica_at` y `optica_rt`.

Los roles se devuelven para que la interfaz decida qué mostrar. **No son control de acceso**: el
servidor los verifica en cada operación (FR-026), y ocultar un control no autoriza nada.

**Respuesta 401** — credenciales inválidas, cuenta inactiva o cuenta bloqueada. Cuerpo idéntico en
los cuatro casos:

```json
{
  "type": "https://optica/errors/credenciales-invalidas",
  "title": "Usuario o contraseña incorrectos",
  "status": 401
}
```

**Respuesta 400** — la petición no cumple las reglas de validación de formato.

**Comportamiento**

1. Se registra el intento en `LoginAttempts` **siempre**, exista o no la cuenta (FR-006, FR-022).
2. Si el nombre de usuario no existe, se ejecuta igualmente una derivación PBKDF2 señuelo para que
   el tiempo de respuesta no delate la ausencia de la cuenta (D-02, SC-004).
3. El contador de fallos se incrementa y el bloqueo se decide en una sola sentencia atómica (D-06).
4. Un ingreso correcto pone el contador en cero y limpia el bloqueo (FR-020).

---

## `POST /api/auth/refresh`

Renueva la sesión rotando la credencial. **Anónimo**, porque se invoca justo cuando el token de
acceso ya venció. La credencial de renovación llega en la cookie; no se acepta en el cuerpo.

**Petición**: sin cuerpo.

**Respuesta 200**: igual que el ingreso, con cookies nuevas. La credencial anterior queda
inservible en el acto (FR-011).

**Respuesta 401** — credencial ausente, vencida, revocada, ya rotada o de otro usuario (FR-012).

**Comportamiento crítico**: si la credencial presentada existe pero ya estaba revocada, se
interpreta como reutilización y se revocan **todas** las credenciales activas del usuario
(FR-013, regla R-C3). El efecto práctico es que el usuario legítimo pierde la sesión, lo cual es
deliberado: significa que alguien más tenía su credencial.

---

## `POST /api/auth/logout`

Cierra la sesión. **Requiere autenticación.**

**Respuesta 204**: revoca las credenciales de renovación activas del usuario (FR-014) y borra
ambas cookies.

Es idempotente: invocarlo sin sesión activa responde igual, sin error.

---

## `GET /api/auth/session`

Devuelve la sesión actual, para que las islas WebAssembly conozcan usuario y roles sin decodificar
el token, que además no pueden leer por ser `HttpOnly`. **Requiere autenticación.**

**Respuesta 200**

```json
{
  "usuarioId": 42,
  "nombreUsuario": "jperez",
  "nombreCompleto": "Juan Pérez",
  "roles": ["Vendedor", "Optometra"],
  "expiraEn": "2026-08-20T15:45:00Z"
}
```

**Respuesta 401** — sin sesión válida.

Los roles se leen del token, no de la base de datos. Es la consecuencia directa de FR-032: un
cambio de roles se refleja aquí a más tardar al vencer el token, dentro de la ventana de 15
minutos.

---

## Casos que ningún endpoint debe filtrar

| Situación | Respuesta | Requisito |
|---|---|---|
| Token con firma inválida, vencido o malformado | 401 sin detalle del motivo | FR-016 |
| Petición sin credencial a un endpoint protegido | 401 | FR-016 |
| Usuario autenticado sin el rol requerido | 403 | FR-026 |
| Usuario sin ningún rol asignado | 401 en el ingreso: **no**. Ingresa bien, pero recibe 403 en toda operación protegida | FR-031 |
