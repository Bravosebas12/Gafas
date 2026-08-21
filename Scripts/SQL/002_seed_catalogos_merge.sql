/* ============================================================================
   002_seed_catalogos_merge.sql
   Carga de catalogos maestros mediante MERGE (upsert idempotente).

   Diseno:
   - La fuente de verdad son los VALUES declarados en cada bloque.
   - MERGE por clave natural (CODIGO o NOMBRE), nunca por el ID IDENTITY, para
     que los IDs existentes no cambien y las FK no se rompan.
   - WHEN MATCHED actualiza solo si algun valor cambio (evita escrituras y
     FECHA_ACTUALIZACION falsas).
   - NUNCA se hace DELETE: las filas ausentes en la fuente se desactivan
     (ACTIVO = 0) para preservar la integridad referencial historica.
   - Reejecutable N veces con el mismo resultado final.

   Ejecucion:
     sqlcmd -S localhost -E -d OpticaDB -i 002_seed_catalogos_merge.sql
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ---------------------------------------------------------------------------
   Pre-requisito: CATALOGOS.TiposIDentificacion no traia clave natural unica
   en 001. MERGE necesita una clave estable para no duplicar filas.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
               WHERE name = N'UQ_TiposIDentificacion_NOMBRE'
                 AND parent_object_id = OBJECT_ID(N'[CATALOGOS].TiposIDentificacion'))
    ALTER TABLE [CATALOGOS].TiposIDentificacion
        ADD CONSTRAINT UQ_TiposIDentificacion_NOMBRE UNIQUE (NOMBRE);
GO

/* Usuario del sistema para la auditoria de los seeds (NULL = proceso automatico). */
DECLARE @UsuarioSistema bigint = NULL;
DECLARE @Resumen TABLE (Catalogo sysname, Accion nvarchar(10), Filas int);

BEGIN TRANSACTION;

/* ===========================================================================
   1. CATALOGOS.EstadosOrden   (clave natural: CODIGO)
   =========================================================================== */
MERGE [CATALOGOS].EstadosOrden AS destino
USING (VALUES
        (N'Abierta',   N'Abierta',   1),
        (N'Abonada',   N'Abonada',   1),
        (N'Pagada',    N'Pagada',    1),
        (N'Entregada', N'Entregada', 1),
        (N'Cancelada', N'Cancelada', 1),
        (N'Anulada',   N'Anulada',   1)
      ) AS fuente (CODIGO, NOMBRE, ACTIVO)
    ON destino.CODIGO = fuente.CODIGO
WHEN MATCHED AND (destino.NOMBRE <> fuente.NOMBRE OR destino.ACTIVO <> fuente.ACTIVO)
    THEN UPDATE SET destino.NOMBRE                = fuente.NOMBRE,
                    destino.ACTIVO                = fuente.ACTIVO,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CODIGO, NOMBRE, ACTIVO, USUARIO_CREACION, USUARIO_ACTUALIZACION)
         VALUES (fuente.CODIGO, fuente.NOMBRE, fuente.ACTIVO, @UsuarioSistema, @UsuarioSistema)
WHEN NOT MATCHED BY SOURCE AND destino.ACTIVO = 1
    THEN UPDATE SET destino.ACTIVO                = 0,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
OUTPUT N'CATALOGOS.EstadosOrden', $action, 1 INTO @Resumen (Catalogo, Accion, Filas);

/* ===========================================================================
   2. CATALOGOS.MetodosPago   (clave natural: CODIGO)
   =========================================================================== */
MERGE [CATALOGOS].MetodosPago AS destino
USING (VALUES
        (N'Efectivo',      N'Efectivo',      1),
        (N'Transferencia', N'Transferencia', 1),
        (N'Nequi',         N'Nequi',         1),
        (N'Tarjeta',       N'Tarjeta',       1)
      ) AS fuente (CODIGO, NOMBRE, ACTIVO)
    ON destino.CODIGO = fuente.CODIGO
WHEN MATCHED AND (destino.NOMBRE <> fuente.NOMBRE OR destino.ACTIVO <> fuente.ACTIVO)
    THEN UPDATE SET destino.NOMBRE                = fuente.NOMBRE,
                    destino.ACTIVO                = fuente.ACTIVO,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CODIGO, NOMBRE, ACTIVO, USUARIO_CREACION, USUARIO_ACTUALIZACION)
         VALUES (fuente.CODIGO, fuente.NOMBRE, fuente.ACTIVO, @UsuarioSistema, @UsuarioSistema)
WHEN NOT MATCHED BY SOURCE AND destino.ACTIVO = 1
    THEN UPDATE SET destino.ACTIVO                = 0,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
OUTPUT N'CATALOGOS.MetodosPago', $action, 1 INTO @Resumen (Catalogo, Accion, Filas);

/* ===========================================================================
   3. CATALOGOS.FiltrosLente   (clave natural: NOMBRE)
   =========================================================================== */
MERGE [CATALOGOS].FiltrosLente AS destino
USING (VALUES
        (N'Blue',        1),
        (N'Antireflejo', 1),
        (N'Normal',      1)
      ) AS fuente (NOMBRE, ACTIVO)
    ON destino.NOMBRE = fuente.NOMBRE
WHEN MATCHED AND destino.ACTIVO <> fuente.ACTIVO
    THEN UPDATE SET destino.ACTIVO                = fuente.ACTIVO,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET
    THEN INSERT (NOMBRE, ACTIVO, USUARIO_CREACION, USUARIO_ACTUALIZACION)
         VALUES (fuente.NOMBRE, fuente.ACTIVO, @UsuarioSistema, @UsuarioSistema)
WHEN NOT MATCHED BY SOURCE AND destino.ACTIVO = 1
    THEN UPDATE SET destino.ACTIVO                = 0,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
OUTPUT N'CATALOGOS.FiltrosLente', $action, 1 INTO @Resumen (Catalogo, Accion, Filas);

/* ===========================================================================
   4. CATALOGOS.TiposIDentificacion   (clave natural: NOMBRE)
   Este catalogo no tenia seed en 001; se cargan los tipos de documento
   estandar usados por la ficha de cliente (RF-CLI-01).
   =========================================================================== */
MERGE [CATALOGOS].TiposIDentificacion AS destino
USING (VALUES
        (N'Cédula de Ciudadanía',            1),
        (N'Cédula de Extranjería',           1),
        (N'Tarjeta de Identidad',            1),
        (N'Pasaporte',                       1),
        (N'NIT',                             1),
        (N'Registro Civil',                  1),
        (N'Permiso por Protección Temporal', 1)
      ) AS fuente (NOMBRE, ACTIVO)
    ON destino.NOMBRE = fuente.NOMBRE
WHEN MATCHED AND destino.ACTIVO <> fuente.ACTIVO
    THEN UPDATE SET destino.ACTIVO                = fuente.ACTIVO,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET
    THEN INSERT (NOMBRE, ACTIVO, USUARIO_CREACION, USUARIO_ACTUALIZACION)
         VALUES (fuente.NOMBRE, fuente.ACTIVO, @UsuarioSistema, @UsuarioSistema)
WHEN NOT MATCHED BY SOURCE AND destino.ACTIVO = 1
    THEN UPDATE SET destino.ACTIVO                = 0,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
OUTPUT N'CATALOGOS.TiposIDentificacion', $action, 1 INTO @Resumen (Catalogo, Accion, Filas);

/* ===========================================================================
   5. ADMINISTRACION_USUARIOS.Roles   (clave natural: CODIGO)
   La tabla no tiene columna ACTIVO, por lo que no se desactivan sobrantes:
   los roles fuera de la fuente se dejan intactos (nunca se borran roles que
   puedan tener usuarios asignados).
   =========================================================================== */
MERGE [ADMINISTRACION_USUARIOS].Roles AS destino
USING (VALUES
        (N'Administrador', N'Administrador'),
        (N'Vendedor',      N'Vendedor'),
        (N'Optometra',     N'Optómetra')
      ) AS fuente (CODIGO, NOMBRE)
    ON destino.CODIGO = fuente.CODIGO
WHEN MATCHED AND destino.NOMBRE <> fuente.NOMBRE
    THEN UPDATE SET destino.NOMBRE                = fuente.NOMBRE,
                    destino.USUARIO_ACTUALIZACION = @UsuarioSistema,
                    destino.FECHA_ACTUALIZACION   = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CODIGO, NOMBRE, USUARIO_CREACION, USUARIO_ACTUALIZACION)
         VALUES (fuente.CODIGO, fuente.NOMBRE, @UsuarioSistema, @UsuarioSistema)
OUTPUT N'ADMINISTRACION_USUARIOS.Roles', $action, 1 INTO @Resumen (Catalogo, Accion, Filas);

COMMIT TRANSACTION;

/* --------------------------------------------------------------------------- */
SELECT Catalogo, Accion, Filas = SUM(Filas)
FROM @Resumen
GROUP BY Catalogo, Accion
ORDER BY Catalogo, Accion;
GO
