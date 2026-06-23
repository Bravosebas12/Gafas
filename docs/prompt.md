# Prompt para Google Stitch - Sistema Óptica

## Módulo 1: Autenticación (`/login`)

Pantalla de login con:
- Logo de óptica (SVG inline)
- Campo usuario (input text)
- Campo contraseña (input password con toggle mostrar/ocultar)
- Botón "Ingresar"
- Mensaje de error genérico (oculto por defecto)
- Indicador de carga (spinner)
- Diseño limpio, profesional, responsive

## Módulo 2: Dashboard (`/dashboard`, `/index`)

Layout con:
- Header con menú lateral colapsable (hamburguesa en móvil)
- Hero: título "Panel de Control", fecha actual
- 3 Cards de métricas:
  1. Volumen de ventas - número de órdenes pendientes de pago total
  2. Ventas del mes - gráfico de líneas SVG (barras por día)
  3. Productos bajo stock - alerta roja si hay productos con stock ≤ mínimo
- Sección de alertas críticas: lista de productos con stock bajo (si existen)
- Notificación interna visible en cualquier pantalla si hay alertas

## Módulo 3: Clientes (`/clientes`)

Pantalla con:
- Header con botón "Nuevo Cliente"
- Búsqueda en tiempo real (filtro por nombre, documento, teléfono)
- Tabla/lista con columnas: Nombre, Documento, Teléfono, Email
- Acciones por fila: Ver detalle, Ver historial clínico
- Modal de ficha cliente con tabs:
  - Tab 1: Datos de contacto (tipo identificación, número, nombre, teléfono, email, dirección)
  - Tab 2: Historial clínico (fórmulas optométricas del cliente)
  - Tab 3: Historial comercial (compras previas, saldos)

## Módulo 4: Inventario (`/inventario`)

Tabs principales: Monturas, Lentes, Servicios

### Monturas
- Tabla con: Marca, Modelo, SKU, Color, Talla, Precio, Stock, Stock Mínimo
- Columna color en rojo cuando Stock ≤ Stock Mínimo
- Botones: Editar, Desactivar

### Lentes (Matriz)
- Filtro por tipo: Blue, Antireflejo, Normal + Marca
- Tabla con: Filtro, Marca, Costo, Precio, Stock

## Módulo 5: Punto de Venta (`/pos`)

Layout dividido:
```
┌─────────────┬──────────────┐
│   Cliente   │    Carrito   │
├─────────────┴──────────────┤
│    Productos/Servicios    │
└───────────────────────────┘
```

- Header cliente: selector/cliente + botón nueva ficha
- Carrito: lista de ítems con cantidad +/-, subtotal, total general
- Grid de productos: monturas (con foto placeholder), lentes, servicios
- Modal de pago: métodos (efectivo, transferencia, Nequi, tarjeta), monto parcial, botón registrar
- Modal fórmula optométrica (obligatorio si hay lentes):
  - OD/OI (ojo derecho/izquierdo) - radio buttons
  - Esfera/SPH - input decimal (acepta 0.00)
  - Cilindro/CYL - input decimal (opcional)
  - Eje/AXIS - input número 1-180 (requerido si hay CYL)
  - Adición/ADD - input decimal (para cerca/multifocal)
  - DP/DNP (Distancia Pupilar) - input decimal
  - Prisma y Base - inputs opcionales
- Estado de la orden: Pendiente → En elaboración → Entregado
- Sección inferior: botones "Cancelar orden", "Registrar pago parcial", "Confirmar venta completa"
- Alerta visual si falta fórmula para lentes
- Cálculo automático saldo pendiente al agregar pagos

## Módulo 6: Ventas (`/ventas`)

- Filtros: rango fechas, estado (Pendiente, Pagado, Entregado, Anulado), cliente
- Tabla: Fecha, Cliente, Total, Pagado, Saldo, Estado
- Acciones: Ver detalle, Devolución (Admin), Anular (Admin), Nota crédito (Admin)
- Modal detalle venta:
  - Ítems vendidos (con foto thumbnail)
  - Pagos registrados (split por método)
  - Estado de cuenta (total, abonos, saldo)
  - Fórmula asociada (si aplica)
  - Botón imprimir ticket
- Modal devolución:
  - Selección de ítems a devolver
  - Cantidad (validar disponibilidad)
  - Motivo/observaciones
  - Botón "Procesar devolución"
- Modal anulación:
  - Confirmación con mensaje de advertencia
  - Motivo obligatorio
  - Botón "Anular venta"

## Módulo 7: Admin Usuarios (`/admin/usuarios`)

- Tabla: Usuario, Nombre, Apellido, Rol, Estado
- Badges por rol: Administrador (morado), Vendedor (azul), Optómetra (verde)
- Acciones: Editar, Desactivar
- Modal usuario:
  - Usuario (único, campo texto)
  - Nombre, Apellido (campos texto)
  - Contraseña (con toggle)
  - Selector múltiple de roles (checkbox: Administrador, Vendedor, Optómetra)
  - Estado activo/inactivo (toggle)
  - Validación visual: mayúscula, número, carácter especial en contraseña

## Módulo 8: Configuración Personal (`/configuracion-perfil`)

- Perfil del usuario autenticado:
  - Nombre completo (solo lectura)
  - Email editable (validación formato)
  - Teléfono editable (formato numérico)
  - Formulario de contacto editable
  - Validación en tiempo real de formato
  - Botón "Guardar cambios"
- Cambio de contraseña:
  - Campo contraseña actual (requerido)
  - Campo nueva contraseña (validación fortaleza)
  - Requisitos visuales: ✓ mayúscula, ✓ número, ✓ carácter especial
  - Botón "Cambiar contraseña"

## Módulo 9: Auditoría (`/admin/auditoria`)

- Filtros: usuario, rango fechas, entidad, acción, valor
- Tabla: Usuario, Fecha/hora, Entidad, Acción, Valor anterior, Valor nuevo
- Entidades auditadas: usuario, cliente, montura, lente, venta, pago, fórmula
- Vista solo para Administrador
- Exportar logs (CSV)
- Paginación (20 registros por página)

## Componentes UI Detallados

### Botones
- `.btn-primary`: azul principal (acciones principales)
- `.btn-secondary`: gris (acciones secundarias)
- `.btn-success`: verde (confirmar/guardar)
- `.btn-danger`: rojo (eliminar/anular)
- `.btn-warning`: ámbar (alertas)

### Formularios
- `.form-control`: inputs con borde redondeado
- `.form-error`: borde rojo + mensaje error
- `.form-success`: borde verde + mensaje éxito
- Validación visual en tiempo real

### Tabs
- `.tab-active`: fondo primario, texto blanco
- `.tab-inactive`: fondo surface, texto muted
- Transición suave entre tabs

### Badges de Estado de Venta
- Pendiente: ámbar warning
- Pagado: verde success
- Entregado: azul primary
- Anulado: gris muted

## Validaciones de Negocio (UI)

- SKU duplicado: mensaje error al crear/editar montura
- Campo obligatorio vacío: borde rojo + mensaje
- Contraseña débil: indicador con requisitos (✓ mayúscula, ✓ número, ✓ especial)
- Stock insuficiente: deshabilitar botón agregar al carrito + tooltip
- Identificación cliente duplicada: error al crear
- Fórmula obligatoria: modal bloqueado si venta tiene lentes + mensaje
- Solo admin: ocultar botones de anular/devolución para no-admins
- Optómetra: solo lectura en fórmulas + botón "Agregar recomendación"
- Cuenta bloqueada: mensaje "Cuenta temporalmente bloqueada, intente en 15 minutos"
- Token expirado: redirección silenciosa al login

## Estados de Órdenes

- **Pendiente**: naranja (oklch 65% 0.15 30)
- **En elaboración**: azul (oklch 55% 0.12 250)
- **Pagado**: verde (oklch 65% 0.15 150)
- **Entregado**: morado (oklch 55% 0.15 280)
- **Anulado**: gris (oklch 40% 0.02 90)
- **Crédito**: rosa (oklch 70% 0.15 340)

## Interacciones Especiales

- Autenticación silenciosa con refresh token
- Notificación de alertas stock en header (badge rojo)
- Confirmación antes de anular/devolver
- Impresión de ticket (PDF o nueva ventana)
- Sticky header en móvil
- Collapsible sidebar en desktop

## Flujos de Usuario

### Vendedor
1. Login → Dashboard
2. Dashboard → Inventario → Agregar montura/lente
3. POS → Seleccionar cliente → Agregar productos → Registrar pago/fórmula → Confirmar venta

### Optómetra
1. Login → Dashboard
2. Clientes → Ver fórmulas → Agregar recomendación
3. No acceso a inventario ni anulaciones

### Administrador
1. Login → Dashboard
2. Admin usuarios → CRUD + roles
3. Auditoría → Ver logs
4. Ventas → Anular, devoluciones, notas crédito

## Permisos por Rol (UI)

| Sección/Función | Administrador | Vendedor | Optómetra |
|-----------------|---------------|----------|-----------|
| Login | ✓ | ✓ | ✓ |
| Dashboard | ✓ | ✓ | ✓ |
| Inventario | ✓ | ✓ | ✗ |
| CRUD Monturas/Lentes | ✓ | ✓ | ✗ |
| Clientes | ✓ | ✓ | ✓ |
| Fórmulas (lectura) | ✓ | ✓ | ✓ |
| Fórmulas (escritura) | ✓ | ✓ | ✗ |
| POS | ✓ | ✓ | ✗ |
| Ventas | ✓ | ✓ | ✗ |
| Devoluciones | ✓ | ✓ | ✗ |
| Anulaciones | ✓ | ✗ | ✗ |
| Notas crédito | ✓ | ✗ | ✗ |
| Admin usuarios | ✓ | ✗ | ✗ |
| Auditoría | ✓ | ✗ | ✗ |
| Config. personal | ✓ | ✓ | ✓ |

## Requisitos de Diseño

- Mobile-first responsive
- Paleta de colores oklch profesional para óptica
- Componentes reutilizables: tarjetas, botones, formularios, modales
- Estados de carga y error controlados
- Iconos SVG inline
- Tabs funcionales sin recarga
- Interacciones suaves con CSS transitions

## Paleta de Colores OKLCH (Professional Optical)

```css
:root {
  /* Neutros - base limpia y moderna */
  --color-bg: oklch(97% 0.01 90);
  --color-surface: oklch(94% 0.015 90);
  --color-text: oklch(15% 0.02 90);
  --color-muted: oklch(40% 0.02 90);

  /* Marca - Azul profesional (confianza + salud visual) */
  --color-primary: oklch(55% 0.12 250);
  --color-primary-hover: oklch(48% 0.14 250);
  --color-primary-active: oklch(42% 0.15 250);
  
  /* Accent - Verde suave (salud, éxito) */
  --color-success: oklch(65% 0.15 150);
  --color-success-bg: oklch(95% 0.08 150);
  
  /* Warning - Ámbar (alerta stock) */
  --color-warning: oklch(75% 0.12 60);
  --color-warning-bg: oklch(96% 0.08 60);
  
  /* Error - Rojo (errores, bloqueo cuenta) */
  --color-error: oklch(60% 0.18 30);
  --color-error-bg: oklch(97% 0.08 30);

  /* Accent - Rosa profesional (destacar acciones importantes) */
  --color-accent: oklch(70% 0.15 340);
  --color-accent-hover: oklch(65% 0.16 340);

  /* Rol badges */
  --color-role-admin: oklch(55% 0.15 280);
  --color-role-seller: oklch(55% 0.12 250);
  --color-role-optometra: oklch(65% 0.15 170);
}
```

## Roles y Badges

- **Administrador**: morado (confianza, autoridad)
- **Vendedor**: azul (acción, ventas)
- **Optómetra**: verde azulado (salud, precisión)

## Entidades Principales

- **Users**: Id, Username, Email, Phone, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
- **EmployeeProfiles**: Id, FirstName, LastName, UserId (FK)
- **Roles**: Id, Name (Administrador/Vendedor/Optómetra)
- **UserRoles**: UserId, RoleId (tabla intermedia)
- **Customers**: Id, IdType, IdNumber (único), Name, Phone, Email, Address
- **OptometricFormulas**: Id, CustomerId, OD, OI, Sphere, Cylinder (nullable), Axis (nullable), Add, DP, Prism, Base
- **Frames**: Id, Brand, Model, SKU (único), Color, Size, Cost, Price, Stock, MinStock
- **Lenses**: Id, Brand, Filter (Blue/Antireflejo/Normal), Cost, Price, Stock, MinStock
- **Services**: Id, Name, Price, Stock, MinStock
- **SalesOrders**: Id, CustomerId, Status, Total, PaidAmount, PendingBalance
- **SaleItems**: Id, SalesOrderId, ProductId, Quantity, UnitPrice
- **Payments**: Id, SalesOrderId, Method, Amount
- **CreditNotes**: Id, SalesOrderId, Amount, Reason
- **Returns**: Id, SalesOrderId, ProductId, Quantity, Reason
- **KardexMovements**: Id, ProductId, Type (Entrada/Salida/Ajuste), Concept, DocumentNumber, QuantityIn, QuantityOut, UnitCost, TotalCost
- **AuditLogs**: Id, UserId, Entity, Action, OldValues, NewValues, Timestamp