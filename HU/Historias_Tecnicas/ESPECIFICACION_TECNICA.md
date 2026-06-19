# Especificación Técnica: Sistema Integral de Gestión de Inventario y POS para Óptica

## Identificación
- **Documento fuente:** Especificación_Requerimientos_Optica_Blazor_v1.1.pdf
- **Versión fuente:** v1.1
- **Fecha de generación fuente:** Junio 2026
- **Ecosistema tecnológico:** .NET 10 y Blazor WebAssembly

## Alcance
El sistema centraliza la operación comercial y clínica de una óptica, incluyendo inventario especializado, punto de venta, facturación, clientes, historial clínico, autenticación interna, roles y dashboard operativo.

## Arquitectura Propuesta
- **Patrón arquitectónico:** Clean Architecture / Arquitectura Cebolla.
- **Patrón de negocio:** CQRS para separar comandos y consultas.
- **Orquestación de casos de uso:** MediatR en la capa de Application.
- **Frontend:** Blazor WebAssembly con .NET 10.
- **UI:** MudBlazor para componentes interactivos.
- **Estado en cliente:** Contenedores de estado en memoria para gestión del POS sin recargas.
- **Persistencia:** Base de datos SQL propia.
- **Autenticación:** Nativa, sin proveedores externos.

## Estructura de Solución
```text
OpticaSolution/
├── src/
│   ├── 1. Core/
│   │   ├── Optica.Domain/
│   │   └── Optica.Application/
│   ├── 2. Infrastructure/
│   │   ├── Optica.Infrastructure/
│   │   └── Optica.Shared/
│   └── 3. Presentation/
│       ├── Optica.API/
│       └── Optica.Client.Blazor/
```

## Capas Técnicas

### Optica.Domain
Contiene entidades, value objects, enumeraciones, reglas de dominio y contratos de agregados. Debe ser independiente de frameworks, UI y persistencia.

### Optica.Application
Contiene casos de uso bajo CQRS, validadores, DTOs, handlers de MediatR, interfaces de repositorios y servicios. Aquí se implementan las reglas funcionales principales.

### Optica.Infrastructure
Implementa persistencia SQL, autenticación, JWT, refresh tokens, auditoría, repositorios, servicios externos internos y configuraciones técnicas.

### Optica.Shared
Comparte contratos, DTOs, validaciones comunes, constantes, tipos y utilidades entre API y Blazor Client cuando sea apropiado.

### Optica.API
Expone endpoints para autenticación, CRUD, ventas, inventario, clientes, dashboard y auditoría. Debe validar autorización y consistencia transaccional.

### Optica.Client.Blazor
Implementa la interfaz de usuario con Blazor WebAssembly, MudBlazor y estado en memoria para flujos POS.

## Seguridad
- Autenticación nativa contra SQL propia.
- Prohibido integrar Google, Microsoft, Facebook u otros proveedores externos.
- JWT de corta duración.
- Refresh tokens almacenados de forma segura.
- Bloqueo de cuenta tras cinco intentos fallidos en quince minutos.
- Roles estrictos: Administrador, Vendedor y Optómetra.
- Logs de auditoría para transacciones críticas.
- Contraseñas con hash seguro y salt.

## Módulos Funcionales
1. Autenticación y Control de Acceso.
2. Dashboard.
3. Gestión de Inventario Especializado.
4. Administración de Usuarios Internos.
5. Configuración Personal.
6. Punto de Venta.
7. Gestión de Clientes e Historial Clínico.

## Requisitos Técnicos Transversales
- Separación clara entre comandos, consultas, dominio e infraestructura.
- Validación en backend para reglas críticas.
- Persistencia SQL con integridad referencial.
- Trazabilidad de operaciones sensibles.
- Consultas eficientes para dashboard y POS.
- Compatibilidad con Blazor WebAssembly.
- Diseño extensible para futuras funcionalidades de óptica.

## Criterios de Aceptación Técnicos Generales
- La solución respeta la estructura de carpetas definida.
- Los casos de uso están organizados bajo CQRS y MediatR.
- La autenticación no depende de proveedores externos.
- Las reglas críticas están validadas en backend.
- Las historias técnicas de usuario tienen criterios de aceptación verificables.
- La UI está implementada en Blazor WebAssembly con componentes reutilizables.
- Los datos sensibles tienen trazabilidad y control de acceso por rol.

## Entregables Esperados
- Solución .NET 10 con la estructura propuesta.
- API REST para operaciones de negocio.
- Cliente Blazor WebAssembly.
- Modelos de dominio para óptica, inventario, ventas, clientes y usuarios.
- Implementación de autenticación nativa y RBAC.
- Dashboard con KPIs, gráficos y alertas.
- POS con órdenes, fórmula optométrica, pagos parciales y tickets.
- Historial clínico de fórmulas optométricas vinculado a clientes y ventas.
