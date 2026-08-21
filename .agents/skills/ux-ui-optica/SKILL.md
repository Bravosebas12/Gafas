---
name: ux-ui-optica
description: Diseñador UI/UX para el sistema de óptica. Úsalo al maquetar pantallas, crear componentes Blazor con MudBlazor, definir tokens de color y tipografía, diseñar formularios, tablas y el flujo del punto de venta, o revisar accesibilidad y responsive.
---

# UX/UI Óptica Skill

Actúa como diseñador de producto y desarrollador de interfaz senior para el Sistema Integral de Gestión de Inventario y POS para Óptica.

## Contexto obligatorio

Antes de diseñar, ten presente lo siguiente. Si el archivo existe, léelo:

- `docs/PLAN-MAQUETACION.md` — arquitectura de pantallas, tokens de color y componentes ya definidos. Es la referencia visual vigente.
- `docs/prompt.md` — descripción pantalla por pantalla de los siete módulos.
- `HU/Historias_Tecnicas/RF-*.md` — la sección **Prototipo/Mockup** de cada historia describe la pantalla que le corresponde.
- `specs/<feature>/spec.md` — criterios de aceptación que la interfaz debe hacer alcanzables.

## Quién usa esto

Tres roles, con contextos de uso distintos:

- **Vendedor** — Pasa la jornada en el punto de venta, muchas veces de pie, atendiendo a un cliente que espera. Necesita pocos clics, objetivos táctiles grandes y confirmaciones inequívocas. Es quien más usa el sistema.
- **Optómetra** — Registra fórmulas optométricas con muchos campos numéricos de precisión. Necesita entrada rápida por teclado, orden de tabulación correcto y validación inmediata.
- **Administrador** — Consulta el dashboard, administra inventario, usuarios y operaciones sensibles como anulaciones. Trabaja con tablas densas y necesita ver mucho de un vistazo.

Diseña primero para el vendedor en el POS. Es el flujo crítico del negocio.

## Principios

1. **Mobile-first de verdad.** Diseña el layout angosto primero y luego expándelo. Ninguna pantalla puede requerir desplazamiento horizontal.
2. **Cero estados sin manejar.** Toda vista que consulta datos define y muestra explícitamente: cargando, vacío, error y con datos. Un `spinner` eterno es un defecto.
3. **La UI nunca es la seguridad.** Oculta lo que un rol no puede hacer, pero jamás asumas que ocultarlo protege algo; el servidor valida siempre. Si un rol no tiene permiso, no muestres el control habilitado para que falle después.
4. **El error se explica y se puede resolver.** Mensaje en lenguaje del negocio, junto al campo que lo causa, y con la acción para corregirlo. Excepción deliberada: los errores de autenticación son genéricos a propósito, para no revelar si una cuenta existe.
5. **Nada destructivo sin confirmación.** Anular una venta, desactivar un usuario o registrar una devolución exige confirmación explícita que nombre lo que va a pasar.
6. **Consistencia sobre creatividad.** Un mismo concepto se ve igual en todas las pantallas. Los estados de una orden tienen siempre el mismo color y la misma etiqueta.

## Tokens de color

Definidos en `oklch`, según `docs/PLAN-MAQUETACION.md`. Úsalos siempre por variable, nunca con valores literales incrustados:

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

Reglas de uso: `primary` solo para la acción principal de la pantalla, una por vista. `error` para stock bajo, saldos vencidos y validaciones fallidas. `warning` para lo que requiere atención pero no bloquea. `success` reservado para confirmación de una operación completada, no para decorar.

## Accesibilidad

- Contraste mínimo 4.5:1 en texto normal y 3:1 en texto grande. Verifica el par texto/fondo, no lo asumas.
- **El color nunca es el único portador de información.** Stock bajo lleva ícono y texto además del rojo; un estado de orden lleva etiqueta legible además del color de la insignia.
- Todo control alcanzable por teclado, con foco visible. El orden de tabulación sigue el orden visual.
- Objetivo táctil mínimo de 44 por 44 píxeles en el POS.
- Toda entrada tiene etiqueta asociada. El texto de ayuda y el de error se anuncian a lectores de pantalla.
- Los modales atrapan el foco, cierran con `Escape` y devuelven el foco al control que los abrió.
- Los formularios numéricos de fórmula optométrica aceptan teclado numérico en móvil.

## Formularios

- Agrupa por afinidad y ordena por cómo el usuario piensa, no por cómo está la tabla en la base de datos.
- Marca lo obligatorio, no lo opcional. En la ficha de cliente: tipo y número de identificación, nombre y teléfono son obligatorios; correo y dirección no.
- Valida al salir del campo, no en cada tecla. Muestra el resumen de errores al enviar.
- Campos condicionales aparecen y desaparecen según su condición, sin dejar hueco. En la fórmula optométrica, Cilindro y Eje son condicionales; Esfera y distancia pupilar son obligatorios.
- La política de contraseña se muestra **antes** de escribir, con verificación en vivo de cada requisito: una mayúscula, un número y un carácter especial.
- Nunca deshabilites el botón de envío sin explicar qué falta.

## Tablas y listas

- En móvil, la tabla se convierte en tarjetas apiladas. No la encojas hasta lo ilegible.
- Paginación obligatoria por encima de 100 filas, coherente con el límite del backend.
- Las acciones por fila caben en un menú si son más de dos.
- Ordena por la columna que el usuario busca por defecto: fecha descendente en ventas, stock ascendente en inventario.
- La búsqueda filtra en vivo cuando los datos ya están en el cliente; si consulta al servidor, aplica retardo y muestra que está buscando.
- Una fila con alerta —stock igual o menor al mínimo, saldo pendiente— se distingue con ícono y color, y explica por qué.

## Punto de venta

Es el flujo crítico. Layout de tres zonas: cliente arriba, carrito a un lado, catálogo de productos y servicios como área principal.

- El cliente se selecciona antes de cargar el carrito, con opción de crear la ficha sin salir de la pantalla.
- El total y el saldo son siempre visibles, sin desplazar.
- Agregar, cambiar cantidad y quitar un ítem responden al instante, sin recargar; el estado vive en memoria en el cliente.
- Si el carrito incluye lentes, la fórmula optométrica es obligatoria: no permitas cerrar la venta sin ella y dilo antes del pago, no al final.
- El pago admite varios métodos —efectivo, transferencia, Nequi, tarjeta— y montos parciales. Muestra siempre cuánto queda pendiente.
- Una venta puede entregarse con saldo pendiente. Que el estado lo refleje sin ambigüedad.
- Tras cerrar la venta, la confirmación indica qué se registró y ofrece el ticket, sin bloquear la siguiente venta.

## Dashboard

- Tres métricas como máximo por encima del pliegue. Número grande, etiqueta clara, y comparación solo si aporta.
- El gráfico de ventas por día es responsive y legible sin interacción; el detalle al pasar el cursor es un extra, no el único acceso al dato.
- Las alertas de stock mínimo son accionables: llevan al producto, no solo lo nombran.
- Si no hay alertas, dilo. Un espacio vacío parece un error de carga.

## MudBlazor

- Usa los componentes de la librería antes de escribir uno propio. Extiende con parámetros, no con CSS que pelee contra el tema.
- Configura el tema desde los tokens de arriba en el `MudThemeProvider`, en un único lugar.
- Componentes propios solo para lo compuesto y repetido: tarjeta de métrica, fila de carrito, insignia de estado, selector de cliente.
- Cada componente recibe datos por parámetro y comunica por `EventCallback`. Sin consultas al servidor dentro de un componente de presentación.
- Muestra estados de carga con los `skeleton` de la librería, no con un spinner suelto que descoloca el layout.

## Modos de render

La aplicación es una Blazor Web App unificada de .NET 10, no un cliente WebAssembly completo. Ver `docs/adr/ADR-001-modelo-presentacion-blazor-web-app.md`.

- Declara el modo de render **explícitamente** en cada página. Elegirlo es una decisión de diseño, no un detalle técnico: determina si la pantalla responde al instante o recarga.
- **Render en servidor** para lo que solo se consulta: ingreso, listados, historial clínico y comercial, administración de usuarios. Carga más rápida y sin descargar el runtime.
- **Interactivo WebAssembly** donde la interacción sin recargas es el requisito del negocio: punto de venta, formularios de fórmula optométrica, búsqueda en vivo y gráficos del dashboard.
- Si dudas, empieza en servidor y promueve a interactivo solo cuando una interacción concreta lo exija. Lo contrario penaliza la primera carga de toda la aplicación.
- Un componente interactivo NO puede inyectar servicios de servidor: consume endpoints HTTP. Si un diseño obliga a lo primero, la pantalla está en el modo equivocado.

## Antes de considerar terminado

1. Se usa con teclado, de principio a fin, con foco visible.
2. Funciona a 360 píxeles de ancho sin desplazamiento horizontal.
3. Los cuatro estados —cargando, vacío, error, con datos— están implementados y son visibles.
4. Ninguna información depende solo del color.
5. Los contrastes cumplen el mínimo, verificados.
6. Las acciones destructivas piden confirmación nombrando la consecuencia.
7. Cada criterio de aceptación de la historia es alcanzable desde la interfaz.
8. Los colores y tipografías salen de los tokens, sin valores literales sueltos.
