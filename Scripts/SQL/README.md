# Scripts SQL — OpticaDB (SQL Server 2022)

Scripts numerados, idempotentes, pensados para ejecutarse en orden y tantas veces
como se quiera. Se aplican con `sqlcmd` usando autenticación Windows.

## Instancia de trabajo

El entorno tiene dos instancias. **La que usa el proyecto es LocalDB**, porque no
requiere permisos de administrador para nada:

| Instancia | Motor | Auth mixta | Estado |
|---|---|---|---|
| `(localdb)\MSSQLLocalDB` | SQL Server 2025 Express LocalDB | Sí (de fábrica) | **En uso.** La arranca y reinicia el propio usuario. |
| `localhost` (`MSSQLSERVER`) | SQL Server 2022 Developer | No efectiva | Tiene el modelo aplicado, pero el login SQL exige reiniciar el servicio como administrador. |

Para apuntar a la instancia de servicio en lugar de LocalDB, basta cambiar
`-S '(localdb)\MSSQLLocalDB'` por `-S localhost` en los comandos de abajo.

> **Importante:** ejecutar siempre con `-f 65001`. Los archivos están en UTF-8 y
> sin ese switch `sqlcmd` los interpreta como ANSI y corrompe las tildes
> (ej. `Optómetra` se guardó como `OptÃ³metra` en la primera carga).

## Orden de ejecución

Con `$S = '(localdb)\MSSQLLocalDB'`:

| Script | Contenido | Comando |
|---|---|---|
| `000_crear_base_datos.sql` | Crea `OpticaDB` y activa RCSI | `sqlcmd -S $S -E -d master -f 65001 -i 000_crear_base_datos.sql` |
| `001_modelo_datos_optica.sql` | DDL: 9 esquemas, 40 tablas, 36 FK, 5 índices | `sqlcmd -S $S -E -d OpticaDB -f 65001 -i 001_modelo_datos_optica.sql` |
| `002_seed_catalogos_merge.sql` | Catálogos maestros vía `MERGE` | `sqlcmd -S $S -E -d OpticaDB -f 65001 -i 002_seed_catalogos_merge.sql` |
| `003_login_aplicacion.sql` | Login `optica_app` + permisos | `sqlcmd -S $S -E -d master -f 65001 -i 003_login_aplicacion.sql -v Clave="<clave>"` |

## Modelo MERGE de catálogos

`002` reemplaza los bloques `IF NOT EXISTS(SELECT 1 FROM <tabla>) + INSERT` que
vivían al final de `001`. Ese patrón sólo sembraba cuando la tabla estaba
**completamente vacía**: cualquier corrección posterior de un nombre de catálogo
no se propagaba nunca a una base ya inicializada.

Reglas del script `MERGE`:

- **Clave natural, nunca el `ID`.** El match es por `CODIGO` (`EstadosOrden`,
  `MetodosPago`, `Roles`) o por `NOMBRE` (`FiltrosLente`,
  `TiposIDentificacion`). Los `IDENTITY` existentes no se tocan, así que las FK
  que ya apuntan a esas filas siguen válidas.
- **`WHEN MATCHED` con guarda de cambio.** Sólo actualiza si algún valor difiere,
  para no ensuciar `FECHA_ACTUALIZACION` en cada corrida.
- **`WHEN NOT MATCHED BY SOURCE` desactiva, no borra** (`ACTIVO = 0`). Nunca se
  hace `DELETE` sobre un catálogo referenciado por órdenes o fórmulas
  históricas. `Roles` no tiene columna `ACTIVO`, por lo que ahí los sobrantes se
  dejan intactos.
- **Salida auditable.** Cada `MERGE` emite `$action` a una tabla temporal y el
  script termina con un resumen por catálogo y acción. Una segunda corrida
  devuelve el resumen vacío: eso es la prueba de idempotencia.

Cambios de esquema que introdujo: `UQ_TiposIDentificacion_NOMBRE` (el catálogo no
tenía clave natural única, requisito para que el `MERGE` no duplique filas) y la
carga inicial de `CATALOGOS.TiposIDentificacion`, que en `001` quedaba vacía.

## Conexión de la aplicación / MCP

El login `optica_app` tiene `db_datareader`, `db_datawriter`, `EXECUTE` y
`VIEW DEFINITION` sobre `OpticaDB`. **No tiene permisos DDL**: los cambios de
esquema se aplican con los scripts numerados y autenticación Windows.

La configuración del MCP está en `.mcp.json` en la raíz del repositorio, que está
en `.gitignore` porque contiene la clave de la BD local.

### El puente TCP

LocalDB sólo escucha en un *named pipe*; no abre ningún puerto TCP. El servidor
MCP usa `tedious`, que sólo habla TCP. [`Scripts/MCP/localdb-tcp-bridge.mjs`](../MCP/localdb-tcp-bridge.mjs)
resuelve esa incompatibilidad reenviando byte a byte entre `127.0.0.1:14330` y el
pipe de la instancia: los paquetes TDS son idénticos en ambos transportes, así
que el relay es transparente.

El nombre del pipe cambia en cada arranque de LocalDB, por eso se resuelve en
caliente con `sqllocaldb info` en lugar de fijarse en la configuración.

[`Scripts/MCP/start-mssql-mcp.mjs`](../MCP/start-mssql-mcp.mjs) es el comando que
declara `.mcp.json`: levanta el puente en su propio proceso y luego lanza
`mssql-mcp` heredando stdin/stdout. El puente vive y muere con el MCP, así que no
queda nada colgado al cerrar la sesión, y si el puerto ya está ocupado reutiliza
el puente existente.

También sirve para conectar cualquier cliente TCP a LocalDB:

```powershell
node Scripts/MCP/localdb-tcp-bridge.mjs MSSQLLocalDB 14330
sqlcmd -S tcp:127.0.0.1,14330 -U optica_app -P <clave> -d OpticaDB
```

### Si algún día se prefiere la instancia de servicio

`MSSQLSERVER` ya tiene `LoginMode = 2` escrito en el registro (aplicado con
`xp_instance_regwrite`), pero el proceso sigue leyendo la configuración vieja.
Sólo falta reiniciarlo **como administrador**, y entonces `optica_app` autentica
ahí también, con TCP nativo en 1433 y sin necesidad del puente:

```powershell
Restart-Service -Name MSSQLSERVER -Force
```
