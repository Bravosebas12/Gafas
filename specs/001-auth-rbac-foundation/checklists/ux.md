# UX y Accesibilidad Checklist: Pantalla de Ingreso

**Purpose**: validar la **calidad de los requisitos de interfaz y accesibilidad** de la única pantalla de la feature antes de implementarla. Evalúa si están escritos de forma completa y medible; no verifica la pantalla.
**Created**: 2026-08-20
**Feature**: [spec.md](../spec.md)
**Uso**: compuerta formal previa a la implementación, recorrida por el autor.

Referencia normativa: principio IX (Interfaz Accesible y Mobile-First) de [constitution.md](../../../.specify/memory/constitution.md).

**Alcance**: la especificación declara que la pantalla de ingreso es la única interfaz incluida. El checklist es corto por eso, no por falta de rigor.

## Completitud de los requisitos de la pantalla

- [ ] CHK001 ¿Están declarados en la **especificación** los requisitos de los cuatro estados de la vista, o solo aparecen en el plan? Si solo están en el plan, ¿es una delegación deliberada? [Traceability, Spec §US1]
- [ ] CHK002 ¿Está especificado el contenido y el comportamiento requeridos del estado de carga mientras se valida la credencial? [Gap, Spec §US1]
- [ ] CHK003 ¿Está definido el requisito de impedir un segundo envío del formulario mientras el primero está en curso? [Gap]
- [ ] CHK004 ¿Está especificado el destino tras un ingreso exitoso, y si debe preservarse la ruta que el usuario intentaba alcanzar? [Gap, Spec §SC-001]
- [ ] CHK005 ¿Existe un requisito sobre el control de mostrar u ocultar la contraseña, que aparece en el prototipo pero no en ningún requisito? [Gap, Spec §Assumptions]
- [ ] CHK006 ¿Está definido el requisito de foco inicial al cargar la pantalla? [Gap, Constitución §IX]

## Requisitos del mensaje de error

- [ ] CHK007 ¿Está especificado el texto exacto o la plantilla del mensaje genérico, de modo que dos implementaciones no produzcan mensajes distintos? [Clarity, Spec §FR-004]
- [ ] CHK008 ¿Está declarado el requisito de que el error se comunique además del color, con ícono o texto, según exige el principio IX? [Traceability, Constitución §IX]
- [ ] CHK009 ¿Está definido si el mensaje de error debe anunciarse a un lector de pantalla, y con qué grado de urgencia? [Gap, Accessibility]
- [ ] CHK010 ¿Está especificado el requisito de qué debe hacer el usuario cuando su cuenta está bloqueada, dado que el mensaje no puede revelar el bloqueo? El usuario legítimo queda sin salida visible durante quince minutos. [Coverage, Spec §FR-004, §FR-018]
- [ ] CHK011 ¿Está definido el idioma de los mensajes y si se requiere alguna capacidad de localización? [Gap]

## Cobertura de escenarios de sesión en la interfaz

- [ ] CHK012 ¿Están definidos los requisitos de lo que ve el usuario cuando la renovación de sesión **falla** en medio de su trabajo? US2 describe la renovación silenciosa exitosa, no el fallo. [Gap, Spec §US2]
- [ ] CHK013 ¿Está especificado qué ocurre en la interfaz cuando la sesión se corta por la revocación en cascada de FR-013, sin que el usuario haya hecho nada? [Coverage, Spec §FR-013]
- [ ] CHK014 ¿Está definido el comportamiento requerido al vencer las ocho horas durante una operación en curso? [Gap, Spec §FR-009]
- [ ] CHK015 ¿Existen requisitos para el cierre de sesión iniciado por el usuario en cuanto a confirmación y destino? [Gap, Spec §FR-014]

## Medibilidad y accesibilidad

- [ ] CHK016 ¿Es SC-001, que exige menos de 3 segundos "hasta ver la pantalla principal", medible de forma reproducible, con un punto de inicio y de fin definidos y un entorno de referencia? [Measurability, Spec §SC-001]
- [ ] CHK017 ¿Están declarados en la especificación los requisitos de contraste, o se heredan del principio IX sin restatement? [Traceability, Constitución §IX]
- [ ] CHK018 ¿Está declarado el requisito de usabilidad a 360 píxeles de ancho sin desplazamiento horizontal para esta pantalla? [Gap, Constitución §IX]
- [ ] CHK019 ¿Está especificado el orden de tabulación esperado y el requisito de foco visible en cada control? [Gap, Constitución §IX]
- [ ] CHK020 ¿Puede verificarse objetivamente que la pantalla es alcanzable por completo con teclado, o el requisito depende de una revisión subjetiva? [Measurability, Constitución §IX]

## Consistencia con el resto del proyecto

- [ ] CHK021 ¿Está declarado el requisito de que los colores y las tipografías provengan de los tokens definidos, sin valores literales? [Traceability, Constitución §IX]
- [ ] CHK022 ¿Es consistente el modo de render elegido para esta pantalla con lo que ADR-001 establece, y está registrado el motivo? [Consistency, Spec §Assumptions]
- [ ] CHK023 ¿Está definido si esta pantalla debe funcionar sin JavaScript habilitado, dado que se resuelve con render en servidor? [Gap]

## Notes

- Los ítems CHK010 y CHK012 son los de mayor impacto para el usuario real: el primero deja al empleado legítimo sin instrucción alguna durante un bloqueo, y el segundo es un hueco de comportamiento en el escenario más frecuente de una jornada larga.
- Varios ítems preguntan por requisitos que existen en la constitución pero no en la especificación. Si la delegación es deliberada, conviene declararla una sola vez en Assumptions en lugar de dejarla implícita por requisito.
