# Scripts SQL — OpticaDB (SQL Server 2022)

Scripts numerados, idempotentes, pensados para ejecutarse en orden y tantas veces
como se quiera. Se aplican con `sqlcmd` usando autenticación Windows.

> **Importante:** ejecutar siempre con `-f 65001`. Los archivos están en UTF-8 y
> sin ese switch `sqlcmd` los interpreta como ANSI y corrompe las tildes
> (ej. `Optómetra` se guardó como `OptÃ³metra` en la primera carga).

## Orden de ejecución

| Script | Contenido | Comando |
|---|---|---|
| `000_crear_base_datos.sql` | Crea `OpticaDB` y activa RCSI | `sqlcmd -S localhost -E -d master -f 65001 -i 000_crear_base_datos.sql` |
| `001_modelo_datos_optica.sql` | DDL: 9 esquemas, 40 tablas, FK e índices | `sqlcmd -S localhost -E -d OpticaDB -f 65001 -i 001_modelo_datos_optica.sql` |
| `002_seed_catalogos_merge.sql` | Catálogos maestros vía `MERGE` | `sqlcmd -S localhost -E -d OpticaDB -f 65001 -i 002_seed_catalogos_merge.sql` |
| `003_login_aplicacion.sql` | Login `optica_app` + permisos | `sqlcmd -S localhost -E -d master -f 65001 -i 003_login_aplicacion.sql -v Clave="<clave>"` |

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

La instancia local estaba en modo *sólo autenticación Windows*. Para que el
servidor MCP (y la app) puedan conectarse con usuario y clave:

```sql
-- ya aplicado
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE',
     N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2;
```

Falta reiniciar el servicio **con permisos de administrador** para que el cambio
tome efecto:

```powershell
Restart-Service -Name MSSQLSERVER -Force
```

El login `optica_app` tiene `db_datareader`, `db_datawriter`, `EXECUTE` y
`VIEW DEFINITION` sobre `OpticaDB`. **No tiene permisos DDL**: los cambios de
esquema se aplican con los scripts numerados y autenticación Windows.

La configuración del MCP está en `.mcp.json` en la raíz del repositorio, que está
en `.gitignore` porque contiene la clave de la BD local.
