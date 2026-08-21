/* ============================================================================
   003_login_aplicacion.sql
   Crea el login SQL de la aplicacion / herramientas (MCP) y sus permisos
   minimos sobre OpticaDB. Idempotente.

   Requiere autenticacion mixta habilitada en la instancia:
     EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE',
          N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2;
     -- y reiniciar el servicio MSSQLSERVER

   Ejecucion (la clave se pasa por variable, nunca se guarda en el archivo):
     sqlcmd -S localhost -E -d master -i 003_login_aplicacion.sql -v Clave="<clave>"
   ============================================================================ */
:setvar Login "optica_app"

IF SUSER_ID(N'$(Login)') IS NULL
    EXEC(N'CREATE LOGIN [$(Login)] WITH PASSWORD = ''$(Clave)'',
             DEFAULT_DATABASE = [OpticaDB],
             CHECK_POLICY = ON, CHECK_EXPIRATION = OFF');
ELSE
    EXEC(N'ALTER LOGIN [$(Login)] WITH PASSWORD = ''$(Clave)''');
GO

USE [OpticaDB];
GO

IF DATABASE_PRINCIPAL_ID(N'$(Login)') IS NULL
    EXEC(N'CREATE USER [$(Login)] FOR LOGIN [$(Login)]');
GO

/* Permisos: lectura/escritura de datos y ejecucion de SP.
   Deliberadamente SIN derechos DDL: los cambios de esquema se aplican con los
   scripts numerados usando autenticacion Windows de un administrador. */
ALTER ROLE db_datareader ADD MEMBER [$(Login)];
ALTER ROLE db_datawriter ADD MEMBER [$(Login)];
GRANT EXECUTE TO [$(Login)];
GRANT VIEW DEFINITION TO [$(Login)];
GO

SELECT Principal = dp.name, Rol = ISNULL(r.name, N'(sin rol)')
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members rm ON rm.member_principal_id = dp.principal_id
LEFT JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
WHERE dp.name = N'$(Login)';
GO
