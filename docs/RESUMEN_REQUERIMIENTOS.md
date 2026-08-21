# Resumen de Requerimientos - Sistema Óptica

## Módulos Funcionales

### 1. Autenticación y Control de Acceso (RF-LOG)
- **RF-LOG-01**: Autenticación nativa con base de datos SQL propia (sin proveedores externos)
- **RF-LOG-02**: JWT de corta duración + refresh tokens seguros con hash
- **RF-LOG-03**: Bloqueo automático de cuenta tras 5 intentos fallidos en 15 minutos
- **RF-LOG-04**: Roles RBAC: Administrador, Vendedor y Optómetra (validación backend requerida)

### 2. Dashboard (RF-DSH)
- **RF-DSH-01**: Métricas de volumen de ventas (órdenes pendientes de pago total)
- **RF-DSH-02**: Gráfico de ventas por día del mes actual
- **RF-DSH-03**: Alertas de stock mínimo con notificaciones internas

### 3. Inventario Especializado (RF-INV)
- **RF-INV-01**: CRUD de monturas (marca, modelo, SKU único, color, tamaño, costo, precio, stock, stock mínimo)
- **RF-INV-02**: Gestión matricial de lentes (marca, filtro: Blue/Antireflejo/Normal)
- **RF-INV-03**: Histórico de movimientos de kárdex (entrada/salida/ajuste, inmutable)

### 4. Usuarios Internos (RF-USR)
- **RF-USR-01**: CRUD de usuarios con desactivación lógica (no física)
- **RF-USR-02**: Perfil de empleado con contraseña segura (hash + salt, política: mayúscula/número/caracter especial)
- **RF-USR-03**: Logs de auditoría inalterables con tabla propia (antes/después)

### 5. Configuración Personal (RF-CFG)
- **RF-CFG-01**: Actualización de información de contacto (autogestión, solo propietario)
- **RF-CFG-02**: Cambio de contraseña con política fuerte (mínimo una mayúscula, un número y un carácter especial)

### 6. Gestión de Clientes e Historial Clínico (RF-CLI)
- **RF-CLI-01**: Ficha de cliente (tipo identificación, número único, nombre, teléfono obligatorios; email/dirección opcionales)
- **RF-CLI-02**: Registro de fórmula optométrica (OD/OI, Esfera, DP/DNP obligatorios; Cilindro/AXIS condicionales, ADD, Prisma/Base)
- **RF-CLI-03**: Consolidado comercial del cliente (histórico de compras, saldos)

### 7. Punto de Venta (RF-VNT)
- **RF-VNT-01**: Órdenes de venta con monturas, lentes y servicios (con stock)
- **RF-VNT-02**: Fórmula optométrica obligatoria cuando venta incluye lentes
- **RF-VNT-03**: Pagos parciales y estados de cuenta (se pueden entregar órdenes con saldo pendiente)
- **RF-VNT-04**: Multi-método de pago y ticket interno (efectivo, transferencia, Nequi, tarjeta)
- **RF-VNT-05**: Devoluciones de venta (ajuste inventario y kárdex)
- **RF-VNT-06**: Anulaciones de venta (solo administradores, solo en estados permitidos)
- **RF-VNT-07**: Cambios de producto en venta (entrada/salida en kárdex)
- **RF-VNT-08**: Notas de crédito (compensación monetaria sin devolución)

## Requisitos Transversales

### Arquitectura
- Clean Architecture / Arquitectura Cebolla
- Patrón CQRS (comandos/consultas separados)
- Orquestación con MediatR
- Blazor Web App (.NET 10) con MudBlazor: render en servidor para consultas, islas WebAssembly para POS y formularios
- Base de datos SQL propia

### Seguridad
- Autenticación nativa (sin Google/Microsoft/Facebook)
- JWT corta duración
- Refresh tokens con hash
- Bloqueo tras 5 intentos fallidos en 15 minutos
- Roles estrictos con validación backend
- Contraseñas con hash + salt
- Política fuerte: mayúscula, número, carácter especial

### Auditoría
- Logs inalterables
- Tablas propias con antes/después para cambios críticos
- Usuario creación/actualización/fecha en todas las tablas

### UX/UI
- Blazor Web App con modo de render declarado por pantalla
- Mobile-first responsive
- Estados de carga y error controlados
- Componentes reutilizables