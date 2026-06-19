# Plan: Maquetación Estática Interactiva - Sistema Óptica

## Objetivo
Crear prototipo HTML/CSS/JS puro con interacciones para demostrar UI del sistema de gestión de inventario y POS para óptica.

## Arquitectura de Páginas (SPAs estáticas)

```
/prototipo/
├── index.html           (Dashboard principal)
├── login.html           (Autenticación)
├── clientes.html        (Gestión de clientes)
├── inventario.html      (Gestión de monturas/lentes)
├── pos.html             (Punto de venta)
├── ventas.html          (Historial de ventas)
├── admin-usuarios.html  (Administración)
├── styles/
│   ├── tokens.css       (Variables CSS oklch)
│   ├── base.css         (Reset + utilidades)
│   └── components.css   (Componentes UI)
└── scripts/
    ├── router.js        (Navegación SPA)
    ├── storage.js       (Mock de datos en LocalStorage)
    └── app.js           (Interacciones generales)
```

## Diseño Visual (oklch tokens)

```css
:root {
  /* Neutros */
  --color-bg: oklch(97% 0.01 90);
  --color-surface: oklch(94% 0.015 90);
  --color-text: oklch(15% 0.02 90);
  --color-muted: oklch(40% 0.02 90);

  /* Marca - Azul profesional */
  --color-primary: oklch(55% 0.12 250);
  --color-primary-hover: oklch(48% 0.14 250);
  --color-accent: oklch(70% 0.15 340);

  /* Estados */
  --color-success: oklch(65% 0.15 150);
  --color-warning: oklch(75% 0.12 60);
  --color-error: oklch(60% 0.18 30);
}
```

## Página 1: Login (`login.html`)

### Elementos UI
- Logo de la óptica (SVG inline)
- Campo usuario (input text)
- Campo contraseña (input password con toggle)
- Botón "Ingresar"
- Mensaje error genérico (oculto por defecto)
- Indicador de carga (spinner CSS)

### Interacciones JS
```javascript
// Validación cliente (demo)
document.getElementById('loginForm').addEventListener('submit', e => {
  e.preventDefault();
  // Mock: redirigir a dashboard si credenciales coinciden
  window.location.href = 'index.html';
});

// Toggle mostrar contraseña
document.querySelector('.toggle-password').addEventListener('click', togglePassword);
```

## Página 2: Dashboard (`index.html`)

### Secciones
1. **Header** - Menú lateral colapsable, usuario logueado
2. **Hero** - Título "Panel de Control", fecha actual
3. **Métricas** (cards):
   - Volumen de ventas (órdenes pendientes)
   - Ventas del mes (gráfico de líneas SVG)
   - Productos bajo stock (alerta roja si hay)
4. **Gráfico** - Ventas por día (SVG responsive)
5. **Alertas** - Lista de alertas stock mínimo (si existen)

### Interacciones
- Menú móvil hamburguesa
- Notificaciones de alertas (localStorage mock)
- Animación de entrada de cards (IntersectionObserver)

## Página 3: Clientes (`clientes.html`)

### Secciones
1. **Header** con botón "Nuevo Cliente"
2. **Búsqueda** - Filtro por nombre/documento/teléfono
3. **Tabla/Lista**:
   - Nombre, Documento, Teléfono, Email
   - Acciones: Ver detalle, Ver historial clínico
4. **Modal** - Ficha cliente con pestañas:
   - Datos de contacto
   - Historial clínico (fórmulas)
   - Historial comercial (compras)

### Interacciones
- Búsqueda en tiempo real (filtro DOM)
- Modal con tabs (JS puro)
- Formulario validación (HTML5 + CSS)

## Página 4: Inventario (`inventario.html`)

### Tabs Principales
- Monturas
- Lentes
- Servicios

### Monturas
- Tabla: Marca, Modelo, SKU, Color, Talla, Precio, Stock, Stock Mínimo
- Columna color indica alerta si Stock ≤ Stock Mínimo
- Botones: Editar, Desactivar

### Lentes (Matriz)
- Filtro: Filtro (Blue/Antireflejo/Normal) + Marca
- Tabla: Filtro, Marca, Costo, Precio, Stock
- Nota: Las lentes usan sistema matricial

### Interacciones
- Tabs sin recarga (JS show/hide)
- Filtro por filtro/marca
- Formulario de stock con validación de no negativo

## Página 5: POS (`pos.html`)

### Layout
```
┌─────────────┬──────────────┐
│   Cliente   │    Carrito   │
│             │              │
├─────────────┴──────────────┤
│    Productos/Servicios    │
└───────────────────────────┘
```

### Secciones
1. **Header Cliente** - Selector cliente + botón nueva ficha
2. **Carrito** - Lista de ítems con:
   - Cantidad +/-
   - Precio subtotal
   - Total general
3. **Productos** - Grid con:
   - Monturas (con foto placeholder)
   - Lentes (según filtro matricial)
   - Servicios
4. **Modal Pago** - Métodos:
   - Efectivo, Transferencia, Nequi, Tarjeta
   - Campo monto parcial
   - Botón "Registrar Pago"
5. **Modal Fórmula** (obligatorio si hay lentes):
   - OD/OI, Esfera, Cilindro (cond.), Eje (cond.), ADD, DP, Prisma, Base

### Interacciones JS
```javascript
// Agregar al carrito
const addToCart = (product) => { ... }

// Calcular totales
const calculateTotal = () => { ... }

// Validar fórmula obligatoria
const validatePrescriptionRequired = (cart) => { ... }

// Sweet alert de éxito/error sin librerías
```

## Página 6: Ventas (`ventas.html`)

### Filtros
- Rango de fechas
- Estado (Pendiente, Pagado, Entregado, Anulado)
- Cliente

### Tabla de Ventas
- Fecha, Cliente, Total, Pagado, Saldo, Estado
- Acciones: Ver detalle, Devolución (Admin), Anular (Admin), Nota crédito (Admin)

### Detalle de Venta (modal)
- Ítems vendidos
- Pagos registrados
- Estado de cuenta
- Fórmula asociada (si aplica)
- Botón imprimir ticket

## Página 7: Admin Usuarios (`admin-usuarios.html`)

### Tabla
- Usuario, Nombre, Apellido, Rol, Estado
- Roles: Administrador, Vendedor, Optómetra (badges)
- Acciones: Editar, Desactivar

### Modal Usuario
- Usuario, Nombre, Apellido, Contraseña
- Selector de rol (checkbox múltiple)
- Validación: mayúscula, número, carácter especial

## JavaScript Modular (sin frameworks)

```javascript
// storage.js - Mock de datos
const DB = {
  users: [],
  customers: [],
  frames: [],
  lenses: [],
  sales: [],
  load: () => JSON.parse(localStorage.getItem('db')),
  save: (data) => localStorage.setItem('db', JSON.stringify(data))
};

// router.js - SPA básico
window.router = {
  navigate: (page) => { /* cambiar vista */ },
  init: () => { /* setup eventos */ }
};
```

## Componentes UI Recursables

### Button Variants
- `.btn-primary` - Acciones principales
- `.btn-secondary` - Acciones secundarias
- `.btn-success` - Confirmar/Guardar
- `.btn-danger` - Eliminar/Anular

### Cards
- `.card` - Contenedor con sombra sutil
- `.card-metric` - Para KPIs con número grande

### Forms
- `.form-control` - Input estilizado
- `.form-error` - Mensaje de error rojo
- `.form-success` - Mensaje verde

### Tables
- `.table` - Tabla responsive
- `.table-striped` - Filas alternas
- `.badge` - Estados con colores

### Modals
- `.modal` - Overlay oculto
- `.modal-open` - Clase activa
- `.modal-content` - Ventana modal

## Interacciones Sin Instalar

| Funcionalidad | Técnica | Archivo |
|--------------|---------|---------|
| Menú móvil | CSS checkbox hack + JS | app.js |
| Tabs | CSS + JS show/hide | app.js |
| Modal | CSS overlay + JS toggle | app.js |
| Toast | CSS animation + JS | components.js |
| Gráfico SVG | Inline SVG responsive | index.html |
| Filtro tabla | JS DOM querySelector | app.js |
| Validación form | HTML5 constraint validation | base.js |
| Almacenamiento | LocalStorage mock API | storage.js |

## Checklist de Entrega

- [ ] 7 páginas HTML estructuradas
- [ ] CSS tokens con oklch
- [ ] JS modular vanilla (ES6)
- [ ] Formularios con validación HTML5
- [ ] Modales y tabs funcionales
- [ ] Responsive mobile-first
- [ ] Interacciones suaves (transiciones)
- [ ] SVG inline para iconos
- [ ] Lazy loading imágenes