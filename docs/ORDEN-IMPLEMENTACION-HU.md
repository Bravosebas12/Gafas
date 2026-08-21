# Orden de Implementación de Historias Técnicas — Sistema Óptica

**Fecha:** 2026-08-20
**Fuente:** `HU/Historias_Tecnicas/` (26 historias), `ESPECIFICACION_TECNICA.md`, `Scripts/SQL/001_modelo_datos_optica.sql`
**Objetivo:** definir una secuencia de implementación que respete dependencias reales de datos y de dominio, empezando por autenticación y cerrando el primer CRUD vertical completo lo antes posible.

---

## 1. Criterios usados para ordenar

El orden **no** sigue solo la prioridad declarada en cada historia (Crítica / Alta / Media). Se ordenó combinando cuatro criterios, en este peso:

1. **Dependencia de datos.** Una historia no puede ir antes que las tablas que consume. `COMERCIAL_VENTAS.OrdenesVentaDetalle` referencia `INVENTARIO.Productos`; por eso todo el POS va después de inventario, aunque el POS tenga prioridad más alta.
2. **Dependencia de seguridad.** Casi toda historia incluye el criterio no funcional *"la acción se valida por rol y se registra en auditoría cuando aplica"*. Sin RBAC y sin auditoría implementados, ninguna historia posterior puede cerrar sus criterios de aceptación. Ambos se adelantan.
3. **Valor de patrón (efecto plantilla).** Se priorizan las historias que establecen un patrón reutilizable — el primer CRUD completo define cómo se hacen los otros seis. Pagar ese costo temprano abarata todo lo que sigue.
4. **Prioridad de negocio declarada**, como criterio de desempate dentro de una misma fase.

**Consecuencia de esto:** las dos historias marcadas **Crítica** (RF-CLI-02 y RF-VNT-02, fórmula optométrica) aparecen en las fases 4 y 5, no al inicio. No es una degradación de prioridad: la fórmula optométrica depende de `CLINICA.FormulasOptometricas` → `CLIENTES.Clientes`, y en el caso de RF-VNT-02 además de `COMERCIAL_VENTAS.OrdenesVenta`. Es la última pieza de esa cadena, no la primera.

---

## 2. Resumen del orden

| # | Historia | Módulo | Prioridad | Fase | Depende de |
|---|----------|--------|-----------|------|-----------|
| 0 | *Habilitador técnico* (sin HU) | — | Bloqueante | 0 | — |
| 1 | RF-LOG-01 Autenticación nativa SQL | LOG | Alta | 1 | Fase 0 |
| 2 | RF-LOG-02 JWT + refresh tokens | LOG | Alta | 1 | 1 |
| 3 | RF-LOG-04 Roles RBAC *(backend)* | LOG | Alta | 1 | 2 |
| 4 | RF-LOG-03 Bloqueo de cuenta | LOG | Alta | 1 | 1 |
| 5 | RF-CFG-02 Cambio de contraseña | CFG | Alta | 1 | 1, 2 |
| 6 | **RF-USR-01 CRUD Usuarios** | USR | Alta | 2 | 3 |
| 7 | RF-USR-02 Perfiles de empleado | USR | Alta | 2 | 6 |
| 8 | RF-USR-03 Logs de auditoría | USR | Alta | 2 | 6 |
| 9 | RF-CFG-01 Contacto de usuario | CFG | Media | 2 | 7 |
| 10 | RF-INV-01 Registro de monturas | INV | Alta | 3 | 6, 8 |
| 11 | RF-INV-02 Gestión matricial de lentes | INV | Alta | 3 | 10 |
| 12 | RF-INV-03 Kárdex histórico | INV | Alta | 3 | 10, 11 |
| 13 | RF-CLI-01 Ficha de clientes | CLI | Alta | 4 | 8 |
| 14 | RF-CLI-02 Registro de fórmula optométrica | CLI | **Crítica** | 4 | 13 |
| 15 | RF-VNT-01 Órdenes de venta | VNT | Alta | 5 | 12, 13 |
| 16 | RF-VNT-02 Fórmula optométrica obligatoria | VNT | **Crítica** | 5 | 14, 15 |
| 17 | RF-VNT-03 Pagos parciales y estados de cuenta | VNT | Alta | 5 | 15 |
| 18 | RF-VNT-04 Multi-método de pago y tickets | VNT | Alta | 5 | 17 |
| 19 | RF-VNT-05 Devoluciones | VNT | Media | 6 | 12, 18 |
| 20 | RF-VNT-06 Anulaciones | VNT | Media | 6 | 18 |
| 21 | RF-VNT-07 Cambios de venta | VNT | Media | 6 | 19 |
| 22 | RF-VNT-08 Notas de crédito | VNT | Media | 6 | 18 |
| 23 | RF-DSH-03 Alertas de stock mínimo | DSH | Alta | 7 | 12 |
| 24 | RF-DSH-01 Métricas financieras | DSH | Alta | 7 | 17 |
| 25 | RF-DSH-02 Gráfico de líneas de ventas | DSH | Media | 7 | 24 |
| 26 | RF-CLI-03 Consolidado comercial del cliente | CLI | Media | 7 | 17 |

---

## 3. Fase 0 — Habilitador técnico (no es una HU, pero bloquea todas)

No existe historia que cubra esto y ninguna de las 26 puede cerrar sin ello. Debe ejecutarse antes de la primera.

- Solución `OpticaSolution` con la estructura exacta de `ESPECIFICACION_TECNICA.md` (`1. Core` / `2. Infrastructure` / `3. Presentation`).
- Wiring de MediatR, validadores y pipeline de comportamientos (validación, logging, transacción).
- Persistencia: ejecución de `Scripts/SQL/001_modelo_datos_optica.sql` y estrategia de migraciones desde ese punto.
- Middleware de errores con contrato de respuesta homogéneo (400 de validación, 401/403, 500).
- Proyecto Blazor WebAssembly con MudBlazor, layout base y cliente HTTP con manejo de estados de carga y error.
- **Seed de un usuario Administrador inicial.** Sin esto, RF-LOG-01 no es probable: no habría con qué iniciar sesión, y RF-USR-01 (que crea usuarios) exige estar autenticado como administrador. El seed rompe ese círculo.

**Criterio de salida:** la solución compila, se conecta a la base y responde un endpoint de salud.

---

## 4. Fase 1 — Autenticación y control de acceso

El arranque solicitado. Es la fase correcta para empezar porque produce el eje de seguridad que todas las demás historias declaran como criterio no funcional.

| Orden | Historia | Por qué aquí |
|-------|----------|--------------|
| 1 | **RF-LOG-01** Autenticación nativa SQL | Base de todo: hash + salt, validación de credenciales contra `ADMINISTRACION_USUARIOS.Usuarios`, error genérico que no revela existencia de cuenta. Define el servicio de hashing que reutilizan RF-USR-02 y RF-CFG-02. |
| 2 | **RF-LOG-02** JWT + refresh tokens | Sin token no hay sesión, y sin sesión no se puede proteger ningún endpoint posterior. Consume `RefreshTokens`, ya presente en el modelo. |
| 3 | **RF-LOG-04** Roles RBAC — *solo backend* | Claims de rol en el JWT y políticas de autorización. Es el guardián de las 22 historias siguientes. |
| 4 | **RF-LOG-03** Bloqueo de cuenta | Depende del flujo de login ya funcionando; consume `LoginAttempts`. Va después porque necesita contar intentos sobre un login que ya existe. |
| 5 | **RF-CFG-02** Cambio de contraseña | Cierra el ciclo de credenciales y reutiliza el hasher y la política fuerte (mayúscula, número, carácter especial). Es autogestión: no depende del CRUD de usuarios. |

**Nota sobre RF-LOG-04 (división recomendada).** Esta historia declara UI en `/admin/usuarios`, que es la misma pantalla de RF-USR-01. Se recomienda partirla:

- **Fase 1:** claims, políticas y validación por rol en backend. Es lo que desbloquea al resto.
- **Fase 2:** pantalla de asignación de roles, que entra junto a RF-USR-01 y evita construir esa vista dos veces.

**Criterio de salida de la fase:** un usuario del seed inicia sesión, recibe JWT, lo renueva, es bloqueado a los 5 intentos fallidos en 15 minutos, cambia su contraseña, y un endpoint restringido a Administrador rechaza con 403 un token de Vendedor.

---

## 5. Fase 2 — Primer CRUD vertical completo + auditoría

Aquí está el CRUD solicitado. **La recomendación es RF-USR-01 (Usuarios) como primer CRUD**, no inventario, por tres razones:

1. **Cierra el ciclo de autenticación.** Hasta que exista el CRUD solo se puede probar con el usuario del seed. Con RF-USR-01 se pueden crear un Vendedor y un Optómetra reales, y solo entonces RBAC es verificable de verdad en lugar de teóricamente.
2. **Reutiliza dominio ya construido.** `Usuarios`, `Roles`, `UsuariosRoles` y `Empleados` ya se modelaron en la fase 1. El costo marginal es la capa CQRS y la UI, no el dominio.
3. **Introduce la desactivación lógica** (no física), que es el patrón de borrado de todo el sistema. Definirlo en el primer CRUD evita reescribir los siguientes.

| Orden | Historia | Alcance del incremento |
|-------|----------|------------------------|
| 6 | **RF-USR-01** CRUD Usuarios | Vertical completo: comandos Create/Update/Deactivate y consultas List/GetById con MediatR, validadores, repositorio, endpoints en `/api/users` y pantalla MudBlazor en `/admin/usuarios` con tabla, formulario y confirmación. Incluye la UI de asignación de roles heredada de RF-LOG-04. |
| 7 | **RF-USR-02** Perfiles de empleado | Extiende la misma pantalla con nombre, apellido y política de contraseña segura. No abre pantalla nueva. |
| 8 | **RF-USR-03** Logs de auditoría | **Adelantada a propósito.** El modelo tiene seis tablas de log (`LogUsuarios`, `LogProductos`, `LogClientes`, `LogOrdenesVenta`, `LogPagos`, `LogFormulasOptometricas`). Si el mecanismo de auditoría se construye ahora como interceptor o behavior de MediatR con valores antes/después, cada historia posterior solo registra su tabla. Si se deja para el final, hay que volver a abrir las 18 historias intermedias. |
| 9 | **RF-CFG-01** Contacto de usuario | Barata y de bajo riesgo: reutiliza el perfil de la historia anterior y la restricción de que solo el propietario edita. Cierra el módulo de configuración personal. |

**Criterio de salida:** un administrador crea un Vendedor desde la UI, ese Vendedor inicia sesión, no ve `/admin/usuarios`, y la creación quedó registrada en `AUDITORIA.LogUsuarios` con valores antes y después.

---

## 6. Fase 3 — Inventario (segundo patrón CRUD y prerequisito del POS)

| Orden | Historia | Por qué en este punto |
|-------|----------|----------------------|
| 10 | RF-INV-01 Monturas | Primer CRUD de catálogo de negocio, con SKU único y stock mínimo. Reutiliza el patrón de la fase 2. |
| 11 | RF-INV-02 Lentes matricial | Más complejo: matriz marca × filtro, `LenteParticiones`, `LenteCOMPLEMENTOs`. Va después de monturas para no aprender el patrón sobre el caso difícil. |
| 12 | RF-INV-03 Kárdex histórico | Debe existir **antes** de la primera venta: toda salida de stock del POS tiene que generar movimiento de kárdex. Si el POS se construye primero, hay que reabrirlo. |

**Criterio de salida:** existe stock real cargado y todo movimiento manual queda en `KardexMovimientos` de forma inmutable.

---

## 7. Fase 4 — Clientes e historial clínico

| Orden | Historia | Por qué |
|-------|----------|---------|
| 13 | RF-CLI-01 Ficha de clientes | Prerequisito duro del POS: no hay orden de venta sin cliente. Documento único y teléfono obligatorios. |
| 14 | **RF-CLI-02** Fórmula optométrica *(Crítica)* | Aquí se paga la complejidad clínica real: OD/OI, esfera y DP/DNP obligatorios, cilindro y AXIS condicionales, ADD, prisma y base. Se implementa **antes** del POS y de forma independiente, para que RF-VNT-02 solo tenga que *exigir* la fórmula, no *inventarla* bajo la presión del flujo de venta. |

**Criterio de salida:** se registra una fórmula completa desde `/clientes/{id}/formulas` y se recupera íntegra; los campos condicionales se validan en backend.

---

## 8. Fase 5 — Punto de venta (núcleo transaccional)

| Orden | Historia | Por qué en este orden |
|-------|----------|----------------------|
| 15 | RF-VNT-01 Órdenes de venta | La orden es el agregado raíz de todo el módulo. Consume inventario (descuento de stock y kárdex) y cliente. |
| 16 | RF-VNT-02 Fórmula obligatoria *(Crítica)* | Regla de negocio sobre la orden: si incluye lentes, exige fórmula. Solo tiene sentido cuando la orden y la fórmula ya existen. Validación en backend, no solo en UI. |
| 17 | RF-VNT-03 Pagos parciales y estados de cuenta | Introduce `Pagos` y el saldo pendiente; habilita entregar órdenes con saldo. |
| 18 | RF-VNT-04 Multi-método de pago y tickets | Extiende pagos con efectivo, transferencia, Nequi y tarjeta, y emite el ticket interno. Cierra el flujo de venta completo. |

**Criterio de salida:** venta completa de montura y lente con fórmula, pagada en dos abonos con dos métodos distintos, con ticket emitido, stock descontado y kárdex registrado.

---

## 9. Fase 6 — Post-venta

Todas dependen de una venta cerrada, por eso van después. Orden interno por acoplamiento a inventario:

| Orden | Historia | Nota |
|-------|----------|------|
| 19 | RF-VNT-05 Devoluciones | Devuelve stock: movimiento de kárdex de entrada. |
| 20 | RF-VNT-06 Anulaciones | Solo administrador y solo en estados permitidos; reversa completa. |
| 21 | RF-VNT-07 Cambios de venta | El más complejo: salida y entrada en kárdex más diferencia de precio. Va después de devoluciones porque reutiliza su lógica de reversa. |
| 22 | RF-VNT-08 Notas de crédito | Puramente monetaria, sin movimiento de inventario. La más aislada del grupo. |

---

## 10. Fase 7 — Dashboard y consolidados

Deliberadamente al final: son historias de **lectura** sobre datos que las fases anteriores producen. Construirlas antes obliga a poblar datos ficticios y a rehacer las consultas cuando el modelo real se estabilice.

| Orden | Historia | Nota |
|-------|----------|------|
| 23 | RF-DSH-03 Alertas de stock mínimo | Solo necesita inventario (fase 3), así que puede adelantarse si se quiere valor visible temprano. Usa `NOTIFICACIONES.NotificacionesStock`. |
| 24 | RF-DSH-01 Métricas financieras | Requiere ventas y pagos reales. |
| 25 | RF-DSH-02 Gráfico de líneas | Se apoya en las consultas de la historia anterior. |
| 26 | RF-CLI-03 Consolidado comercial del cliente | Cruza cliente, órdenes, pagos y saldos. Es la lectura más transversal del sistema; con todo lo anterior cerrado es casi solo una consulta. |

---

## 11. Riesgos del orden propuesto

| Riesgo | Impacto | Mitigación |
|--------|---------|-----------|
| El valor visible para el negocio llega tarde (el POS es la fase 5) | Percepción de poco avance en fases 1–3 | Usar el prototipo de `docs/PLAN-MAQUETACION.md` como demo de UI en paralelo, y adelantar RF-DSH-03 al final de la fase 3 |
| La auditoría adelantada puede sobre-diseñarse sin conocer todos los casos | Retrabajo en el interceptor | Implementar el mecanismo genérico con una sola tabla (`LogUsuarios`) y extenderlo por tabla en cada fase |
| RF-LOG-04 dividida en dos fases puede quedar a medias | Historia sin cerrar formalmente | Aceptarla solo cuando entre la UI en la fase 2; hasta entonces marcar el backend como parcial explícito |
| Las dos historias Críticas quedan en fases 4–5 | Cuestionamiento de prioridades | Está justificado por dependencia de datos; RF-CLI-02 se adelanta lo más posible dentro de su cadena |
| La fase 0 no tiene historia que la respalde | Trabajo invisible en el backlog | Crearla como historia técnica habilitadora usando `HU/Plantilla/Plantilla_Historia_Tecnica.md` |

---

## 12. Siguientes pasos sugeridos

1. Validar el orden con el equipo, en particular la elección de RF-USR-01 como primer CRUD frente a la alternativa RF-INV-01.
2. Crear la historia técnica habilitadora de la fase 0 con la plantilla del proyecto.
3. Dividir formalmente RF-LOG-04 en backend y UI.
4. Generar los casos de prueba de la fase 1 con la skill `test-case-management` instalada en `.agents/skills/`, tomando como entrada los criterios Dado/Cuando/Entonces que ya traen las historias.
