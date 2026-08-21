/* ============================================================================
   000_crear_base_datos.sql
   Crea la base de datos de la aplicación si no existe. Idempotente.
   Ejecutar contra [master]:
     sqlcmd -S localhost -E -d master -i 000_crear_base_datos.sql
   ============================================================================ */
IF DB_ID(N'OpticaDB') IS NULL
BEGIN
    PRINT N'Creando base de datos OpticaDB...';
    EXEC(N'CREATE DATABASE [OpticaDB]');
END
ELSE
    PRINT N'OpticaDB ya existe. Sin cambios.';
GO

ALTER DATABASE [OpticaDB] SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO
