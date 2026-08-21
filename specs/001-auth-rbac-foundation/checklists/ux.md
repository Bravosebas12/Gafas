# UX y Accesibilidad Checklist: Pantalla de Ingreso

**Purpose**: validar la **calidad de los requisitos de interfaz y accesibilidad** de la única pantalla de la feature antes de implementarla. Evalúa si están escritos de forma completa y medible; no verifica la pantalla.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

Referencia normativa: principio IX (Interfaz Accesible y Mobile-First) de [constitution.md](../../../.specify/memory/constitution.md).

**Alcance**: la especificación declara que la pantalla de ingreso es la única interfaz incluida. El checklist es corto por eso, no por falta de rigor.

## Completitud de los requisitos de la pantalla

- [x] CHK001 ¿Están declarados en la **especificación** los requisitos de los cuatro estados de la vista, o solo aparecen en el plan? Si solo están en el plan, ¿es una delegación deliberada? [Traceability, Spec §US1] — **RESUELTO 2026-08-21**: delegación deliberada. La especificación describe comportamiento observable y no interfaz; el principio IX es el que exige los cuatro estados, y la tabla concreta vive en el plan. La compuerta G9 los verifica y los casos CP-E2E-03 a 05 los automatizan.
- [x] CHK002 ¿Está especificado el contenido y el comportamiento requeridos del estado de carga mientras se valida la credencial? [Gap, Spec §US1] — **RESUELTO 2026-08-21**: especificado en la tabla de estados del plan: botón deshabilitado con indicador de progreso mientras se valida. Verificado por CP-E2E-04, que retarda la respuesta a propósito para poder observar el estado intermedio.
- [x] CHK003 ¿Está definido el requisito de impedir un segundo envío del formulario mientras el primero está en curso? [Gap] — **RESUELTO 2026-08-21**: definido en la misma tabla —"el formulario no acepta un segundo envío"— y verificado por CP-E2E-04, que comprueba que el servidor recibió exactamente 1 petición. Importa además por seguridad: un doble envío consumiría el presupuesto de cinco intentos al doble de velocidad.
- [x] CHK004 ¿Está especificado el destino tras un ingreso exitoso, y si debe preservarse la ruta que el usuario intentaba alcanzar? [Gap, Spec §SC-001] — **RESUELTO 2026-08-21**: el destino está definido, la pantalla principal. La preservación de la ruta pretendida **no aplica todavía**: esta feature tiene una sola pantalla, así que no existe ruta protegida desde la que llegar. Pertenece a la primera feature que introduzca pantallas protegidas.
- [x] CHK005 ¿Existe un requisito sobre el control de mostrar u ocultar la contraseña, que aparece en el prototipo pero no en ningún requisito? [Gap, Spec §Assumptions] — **RESUELTO 2026-08-21**: sí existe como requisito, en el recorrido por teclado que fija el plan —usuario, contraseña, mostrar u ocultar contraseña, ingresar— y verificado por CP-E2E-08 en su posición del recorrido y por CP-E2E-14 en su comportamiento de enmascarado.
- [x] CHK006 ¿Está definido el requisito de foco inicial al cargar la pantalla? [Gap, Constitución §IX] — **RESUELTO 2026-08-21**: definido en el estado vacío de la tabla del plan y en T041, con foco inicial en el campo de usuario. Verificado por CP-E2E-03.

## Requisitos del mensaje de error

- [x] CHK007 ¿Está especificado el texto exacto o la plantilla del mensaje genérico, de modo que dos implementaciones no produzcan mensajes distintos? [Clarity, Spec §FR-004] — **RESUELTO 2026-08-21**: el texto exacto está fijado en el contrato: el campo `title` es literalmente "Usuario o contraseña incorrectos", y el `type` es `https://optica/errors/credenciales-invalidas`. Dos implementaciones no pueden divergir.
- [x] CHK008 ¿Está declarado el requisito de que el error se comunique además del color, con ícono o texto, según exige el principio IX? [Traceability, Constitución §IX] — **RESUELTO 2026-08-21**: declarado en T041 —mensaje de error con ícono además del color— y verificado por CP-E2E-05, que inspecciona el bloque del mensaje y exige el ícono.
- [x] CHK009 ¿Está definido si el mensaje de error debe anunciarse a un lector de pantalla, y con qué grado de urgencia? [Gap, Accessibility] — **RESUELTO 2026-08-21**: definido como requisito verificable en CP-E2E-07: la región del mensaje debe estar marcada como región activa de anuncio y el campo debe quedar asociado al error y marcado como inválido. El grado de urgencia concreto es detalle de implementación y no cambia lo observable.
- [x] CHK010 ¿Está especificado el requisito de qué debe hacer el usuario cuando su cuenta está bloqueada, dado que el mensaje no puede revelar el bloqueo? El usuario legítimo queda sin salida visible durante quince minutos. [Coverage, Spec §FR-004, §FR-018] — **RESUELTO 2026-08-21**: se añadió FR-004a. El mensaje genérico incluye una línea de orientación **fija y siempre presente** que indica a quién dirigirse si el problema persiste. Al estar siempre visible no revela el estado de la cuenta, y da salida al usuario legítimo bloqueado que de otro modo queda quince minutos sin ninguna acción posible. Implementado en T041.
- [ ] CHK011 ¿Está definido el idioma de los mensajes y si se requiere alguna capacidad de localización? [Gap]

## Cobertura de escenarios de sesión en la interfaz

- [ ] CHK012 ¿Están definidos los requisitos de lo que ve el usuario cuando la renovación de sesión **falla** en medio de su trabajo? US2 describe la renovación silenciosa exitosa, no el fallo. [Gap, Spec §US2]
- [ ] CHK013 ¿Está especificado qué ocurre en la interfaz cuando la sesión se corta por la revocación en cascada de FR-013, sin que el usuario haya hecho nada? [Coverage, Spec §FR-013]
- [ ] CHK014 ¿Está definido el comportamiento requerido al vencer las ocho horas durante una operación en curso? [Gap, Spec §FR-009]
- [ ] CHK015 ¿Existen requisitos para el cierre de sesión iniciado por el usuario en cuanto a confirmación y destino? [Gap, Spec §FR-014]

## Medibilidad y accesibilidad

- [x] CHK016 ¿Es SC-001, que exige menos de 3 segundos "hasta ver la pantalla principal", medible de forma reproducible, con un punto de inicio y de fin definidos y un entorno de referencia? [Measurability, Spec §SC-001] — **RESUELTO 2026-08-21**: medible de forma reproducible. CP-E2E-02 fija el inicio en el envío del formulario, el fin en la visibilidad de la pantalla principal, la mediana de 5 corridas como estadístico y una petición previa de calentamiento. El entorno de referencia es el host de prueba de T041c.
- [x] CHK017 ¿Están declarados en la especificación los requisitos de contraste, o se heredan del principio IX sin restatement? [Traceability, Constitución §IX] — **RESUELTO 2026-08-21**: herencia deliberada del principio IX, que fija 4.5 a 1 en texto normal, con verificación automatizada en CP-E2E-15 sobre los colores tal como los aplica el navegador. No hace falta repetirlo en el spec para que sea exigible.
- [x] CHK018 ¿Está declarado el requisito de usabilidad a 360 píxeles de ancho sin desplazamiento horizontal para esta pantalla? [Gap, Constitución §IX] — **RESUELTO 2026-08-21**: declarado en dos sitios. El principio IX lo exige de forma general y el plan lo fija en Target Platform, "a partir de 360 píxeles de ancho". Verificado por CP-E2E-09.
- [x] CHK019 ¿Está especificado el orden de tabulación esperado y el requisito de foco visible en cada control? [Gap, Constitución §IX] — **RESUELTO 2026-08-21**: el plan especifica el orden exacto —usuario, contraseña, mostrar u ocultar, ingresar— y exige foco visible en los cuatro. CP-E2E-08 comprueba el orden paso a paso, de modo que un control fuera de secuencia lo detecta.
- [x] CHK020 ¿Puede verificarse objetivamente que la pantalla es alcanzable por completo con teclado, o el requisito depende de una revisión subjetiva? [Measurability, Constitución §IX] — **RESUELTO 2026-08-21**: verificable sin revisión subjetiva. CP-E2E-08 recorre el formulario con tabulaciones y lo envía con Enter sin usar el ratón. La revisión humana de la carta E-06 sigue siendo complementaria, no sustituta.

## Consistencia con el resto del proyecto

- [x] CHK021 ¿Está declarado el requisito de que los colores y las tipografías provengan de los tokens definidos, sin valores literales? [Traceability, Constitución §IX] — **RESUELTO 2026-08-21**: declarado en el principio IX y asignado a T040, que mapea los tokens de `docs/PLAN-MAQUETACION.md` al tema de MudBlazor sin valores literales. Verificado por el tercer paso de CP-E2E-15.
- [x] CHK022 ¿Es consistente el modo de render elegido para esta pantalla con lo que ADR-001 establece, y está registrado el motivo? [Consistency, Spec §Assumptions] — **RESUELTO 2026-08-21**: consistente y con motivo registrado. Assumptions de la especificación declara que el ingreso se resuelve con render en servidor porque no necesita interactividad cliente y así evita descargar el runtime antes de autenticar, alineado con ADR-001 y con la decisión D-13.
- [ ] CHK023 ¿Está definido si esta pantalla debe funcionar sin JavaScript habilitado, dado que se resuelve con render en servidor? [Gap]

## Notes

- Los ítems CHK010 y CHK012 son los de mayor impacto para el usuario real: el primero deja al empleado legítimo sin instrucción alguna durante un bloqueo, y el segundo es un hueco de comportamiento en el escenario más frecuente de una jornada larga.
- Varios ítems preguntan por requisitos que existen en la constitución pero no en la especificación. Si la delegación es deliberada, conviene declararla una sola vez en Assumptions en lugar de dejarla implícita por requisito.
