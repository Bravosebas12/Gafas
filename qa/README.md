# QA — Escenarios de prueba y análisis

Carpeta de artefactos de calidad del repositorio. Contiene los escenarios Gherkin derivados de
cada especificación y el análisis de técnicas de prueba que los justifica.

No contiene código de automatización. Los escenarios de aquí son la fuente de la que se derivan las
pruebas de `tests/` —xUnit en dominio, aplicación, integración y arquitectura; NUnit con Playwright
en la suite de navegador—, y la trazabilidad de cada carpeta es la que responde qué requisito quedó
sin cubrir.

## Estructura

```text
qa/
└── <feature>/
    ├── features/          Escenarios Gherkin ejecutables por requisito
    ├── casos-de-prueba/   Casos especificados, listos para automatizar
    ├── analisis/          Técnicas aplicadas: equivalencias, límites, exploratorias
    └── trazabilidad.md    Requisito ↔ escenario, con la lista de huecos
```

## Convenciones

- **Idioma Gherkin español.** Todo archivo `.feature` abre con `# language: es` y usa
  `Característica`, `Antecedentes`, `Escenario`, `Esquema del escenario`, `Ejemplos`,
  `Dado`, `Cuando`, `Entonces`, `Y`.
- **Un escenario por regla de negocio.** Tres reglas son tres escenarios con tres veredictos
  independientes, no un escenario con un párrafo de resultados esperados. Cuando una regla falla,
  el nombre del escenario debe bastar para saber qué se rompió.
- **Resultado esperado verificable.** Cada `Entonces` nombra un valor exacto: un código de estado,
  un mensaje literal, un conteo, un campo concreto o una condición acotada en el tiempo.
  Están prohibidas las formulaciones no comprobables del tipo "funciona", "se ve bien",
  "sin problemas" o "esperar un rato": dos personas deben poder acordar el veredicto sin adivinar.
- **Datos de prueba concretos.** Nombres de usuario, contraseñas, conteos y tiempos aparecen
  literales. Ningún escenario dice "unos datos" o "un valor".
- **Etiquetas.** Cada escenario lleva la etiqueta de su requisito (`@FR-017`), la de la técnica
  que lo originó (`@equivalencia`, `@limite`, `@exploratoria`) y la de su uso en ejecución
  (`@smoke`, `@regresion`, `@seguridad`, `@concurrencia`).

## Control del tiempo

Buena parte de los requisitos de autenticación son temporales: 15 minutos de token, 15 minutos de
bloqueo, 8 horas de sesión. Ningún escenario espera tiempo real. Todos manipulan el reloj de
prueba que la abstracción de reloj del dominio permite sustituir, de modo que el paso
`Cuando avanza el reloj 15 minutos` es instantáneo y determinista.

Un escenario que dependa del reloj del sistema es un escenario inestable, y un escenario inestable
mide la infraestructura en lugar del requisito.

## Técnicas aplicadas

| Técnica | Qué busca | Dónde queda |
|---|---|---|
| Particiones de equivalencia | Un representante por clase de entrada, sin repetir clases | `analisis/clases-equivalencia.md` |
| Análisis de valores límite | El borde exacto de cada regla, y el valor inmediatamente anterior y posterior | `analisis/valores-limite.md` |
| Pruebas exploratorias | Lo que la especificación no dice, con cartas de exploración acotadas | `analisis/pruebas-exploratorias.md` |

Las dos primeras producen escenarios Gherkin. La tercera produce cartas de exploración con
duración y oráculo, no guiones paso a paso: su valor está en lo que el guion no anticipa. Los
defectos que encuentre se convierten después en escenarios de regresión.

## Niveles de prueba

Cada caso se especifica en el nivel más bajo donde su requisito sea observable. Subirlo de nivel no
agrega cobertura: agrega tiempo de ejecución y probabilidad de fallo intermitente.

| Nivel | Qué verifica | Dónde se especifica |
|---|---|---|
| Unitaria | Reglas de dominio, handlers, validadores, hasheo | `casos-de-prueba/<tema>.md` |
| Integración | Persistencia, transacciones, concurrencia, contratos HTTP | `casos-de-prueba/<tema>.md` |
| Navegador | Lo que solo se observa en un navegador real: estados de vista, teclado, foco, contraste, cookies `HttpOnly`, tiempo del recorrido completo | `casos-de-prueba/<tema>-e2e-playwright.md` |

Los casos de navegador se automatizan con **Playwright sobre NUnit**
(`Microsoft.Playwright.NUnit`), en un proyecto separado y fuera del cálculo de cobertura por capa,
porque la cobertura que produce un navegador atravesando toda la pila no es atribuible a una capa y
distorsiona los umbrales del principio IV. NUnit es una desviación deliberada del principio IV,
acotada a ese único proyecto y registrada en Complexity Tracking del plan. La decisión y sus
límites están en D-13 de `research.md`.

## Features cubiertas

- [001-auth-rbac-foundation](001-auth-rbac-foundation/) — Autenticación, sesión, bloqueo de cuenta
  y control de acceso por rol.
