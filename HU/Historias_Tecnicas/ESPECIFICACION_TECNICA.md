# Especificación Técnica: Sistema Integral de Gestión de Inventario y POS para Óptica

## Identificación
- **Documento fuente:** Especificación_Requerimientos_Optica_Blazor_v1.1.pdf
- **Versión fuente:** v1.1
- **Fecha de generación fuente:** Junio 2026
- **Ecosistema tecnológico:** .NET 10 y Blazor Web App
- **Desviación respecto a la fuente:** el PDF v1.1 fija Blazor WebAssembly con proyecto de API separado. Este documento adopta el modelo unificado de Blazor Web App de .NET 10. La decisión, su motivo y sus consecuencias están registradas en `docs/adr/ADR-001-modelo-presentacion-blazor-web-app.md`.

## Alcance
El sistema centraliza la operación comercial y clínica de una óptica, incluyendo inventario especializado, punto de venta, facturación, clientes, historial clínico, autenticación interna, roles y dashboard operativo.

## Arquitectura Propuesta
- **Patrón arquitectónico:** Clean Architecture / Arquitectura Cebolla.
- **Patrón de negocio:** CQRS para separar comandos y consultas.
- **Orquestación de casos de uso:** MediatR en la capa de Application.
- **Frontend:** Blazor Web App con .NET 10, en un único proyecto web con modos de render mixtos.
- **Modos de render:** render estático en servidor para pantallas de consulta y listados; render interactivo WebAssembly para las pantallas que exigen interacción sin recargas, principalmente el punto de venta y los formularios de fórmula optométrica.
- **UI:** MudBlazor para componentes interactivos.
- **Estado en cliente:** Contenedores de estado en memoria para gestión del POS sin recargas, en las islas interactivas WebAssembly.
- **Endpoints HTTP:** hospedados en el mismo proyecto web. Las islas WebAssembly consumen los endpoints por HTTP igual que antes; lo que desaparece es el proyecto de API separado, no los contratos.
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
│       ├── Optica.Web/           (Blazor Web App: componentes SSR + endpoints HTTP)
│       └── Optica.Web.Client/    (islas interactivas WebAssembly)
```

## Capas Técnicas

### Optica.Domain
Contiene entidades, value objects, enumeraciones, reglas de dominio y contratos de agregados. Debe ser independiente de frameworks, UI y persistencia.

### Optica.Application
Contiene casos de uso bajo CQRS, validadores, DTOs, handlers de MediatR, interfaces de repositorios y servicios. Aquí se implementan las reglas funcionales principales.

### Optica.Infrastructure
Implementa persistencia SQL, autenticación, JWT, refresh tokens, auditoría, repositorios, servicios externos internos y configuraciones técnicas.

### Optica.Shared
Comparte contratos, DTOs, validaciones comunes, constantes, tipos y utilidades entre el servidor y las islas WebAssembly. Debe poder compilar para WebAssembly, por lo que no admite dependencias exclusivas de servidor.

### Optica.Web
Proyecto web único. Cumple dos funciones. Como interfaz, aloja el enrutado, el layout, los componentes de render estático en servidor y la configuración del tema de MudBlazor. Como backend, expone los endpoints HTTP de autenticación, CRUD, ventas, inventario, clientes, dashboard y auditoría, que las islas WebAssembly consumen. Debe validar autorización y consistencia transaccional en cada endpoint.

### Optica.Web.Client
Contiene los componentes que requieren interactividad en el navegador, ejecutados en WebAssembly: punto de venta, formularios de fórmula optométrica, filtros en vivo y gráficos del dashboard. Aquí viven los contenedores de estado en memoria del POS.

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
- Compatibilidad con Blazor Web App: el código compartido debe compilar para WebAssembly.
- Diseño extensible para futuras funcionalidades de óptica.

## Criterios de Aceptación Técnicos Generales
- La solución respeta la estructura de carpetas definida.
- Los casos de uso están organizados bajo CQRS y MediatR.
- La autenticación no depende de proveedores externos.
- Las reglas críticas están validadas en backend.
- Las historias técnicas de usuario tienen criterios de aceptación verificables.
- La UI está implementada como Blazor Web App, con componentes reutilizables y el modo de render declarado por pantalla.
- Los datos sensibles tienen trazabilidad y control de acceso por rol.

## Entregables Esperados
- Solución .NET 10 con la estructura propuesta.
- Endpoints HTTP para operaciones de negocio, hospedados en el proyecto web.
- Islas interactivas WebAssembly para punto de venta, fórmula optométrica y gráficos.
- Modelos de dominio para óptica, inventario, ventas, clientes y usuarios.
- Implementación de autenticación nativa y RBAC.
- Dashboard con KPIs, gráficos y alertas.
- POS con órdenes, fórmula optométrica, pagos parciales y tickets.
- Historial clínico de fórmulas optométricas vinculado a clientes y ventas.
