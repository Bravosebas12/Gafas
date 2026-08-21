# Contratos — Roles (`/api/roles`, `/api/users/{id}/roles`)

**Feature**: 001-auth-rbac-foundation
**Base**: rutas declaradas en RF-LOG-04.

**Alcance**: solo la capa de servidor. La pantalla `/admin/usuarios` que RF-LOG-04 menciona **no**
se implementa aquí; llega con la feature del módulo RF-USR, tal como el checklist de la
especificación dejó registrado. Estos endpoints quedan verificados por prueba automática invocada
directamente contra el servidor, que es lo que exige la compuerta G6.

---

## `GET /api/roles`

Lista los tres roles del sistema. **Requiere rol Administrador.**

**Respuesta 200**

```json
[
  { "codigo": "Administrador", "nombre": "Administrador" },
  { "codigo": "Vendedor",      "nombre": "Vendedor" },
  { "codigo": "Optometra",     "nombre": "Optómetra" }
]
```

El conjunto es cerrado y ya está sembrado por `002_seed_catalogos_merge.sql`. Este endpoint no crea
ni modifica roles: no existe `POST /api/roles` y no debe existir (FR-024).

Recordatorio que ahorra un defecto silencioso: el código es `Optometra` sin tilde y el nombre
visible `Optómetra` con tilde. Las políticas de autorización usan el **código**.

---

## `PUT /api/users/{id}/roles`

Reemplaza el conjunto completo de roles de un usuario. **Requiere rol Administrador** (FR-027).

**Petición**

```json
{ "roles": ["Vendedor", "Optometra"] }
```

Se eligió reemplazo del conjunto completo en lugar de dos endpoints de agregar y quitar, porque la
regla del último administrador (FR-030) debe evaluarse sobre el estado final. Con operaciones
sueltas, una secuencia de dos llamadas puede dejar el sistema sin administradores entre la primera
y la segunda.

**Respuesta 200**

```json
{
  "usuarioId": 42,
  "roles": ["Vendedor", "Optometra"],
  "credencialesRevocadas": 3
}
```

`credencialesRevocadas` informa cuántas sesiones se cortaron por FR-032a.

**Respuesta 400** — un código de rol fuera del conjunto cerrado (FR-024), o la operación dejaría el
sistema sin ningún usuario activo con rol Administrador (FR-030).

```json
{
  "type": "https://optica/errors/ultimo-administrador",
  "title": "La operación dejaría el sistema sin ningún administrador activo",
  "status": 400
}
```

**Respuesta 403** — el solicitante no es Administrador. Se verifica en el servidor aunque la
interfaz haya ocultado el control (FR-026).

**Respuesta 404** — el usuario no existe.

**Comportamiento transaccional**, todo en una sola transacción:

1. Se calcula el conjunto de roles a agregar y a quitar.
2. Se verifica la regla del último administrador **sobre el estado final** (FR-030). Cubre el caso
   borde del administrador que intenta quitarse su propio rol.
3. Se aplican los cambios en `UsuariosRoles`.
4. Se escribe la auditoría con valor anterior y posterior (FR-028), en la misma transacción
   (compuerta G7).
5. Se revocan las credenciales de renovación activas del usuario afectado (FR-032a).

Si cualquier paso falla, se revierte todo, incluida la auditoría.

---

## Políticas de autorización

Esta feature define el **mecanismo**; el mapa completo de operaciones por rol se completa en las
features de cada módulo, según lo asumido en la especificación.

| Política | Requiere | Uso en esta feature |
|---|---|---|
| `SoloAdministrador` | Rol `Administrador` | `GET /api/roles`, `PUT /api/users/{id}/roles` |
| `Autenticado` | Cualquier sesión válida | `POST /api/auth/logout`, `GET /api/auth/session` |

Reglas del mecanismo:

- **Un usuario con varios roles se autoriza si cualquiera de ellos satisface la política**
  (FR-025).
- **Un usuario sin roles no satisface ninguna política salvo `Autenticado`** (FR-031).
- Los roles se leen de los claims del token de acceso, no de la base de datos, de modo que un
  cambio de roles surte efecto al vencer el token, con un máximo de 15 minutos (FR-032). La
  revocación inmediata de credenciales de renovación impide extender esa ventana (FR-032a).
