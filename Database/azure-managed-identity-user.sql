/*
Grant the Azure App Service copy of the API access to VerichatDB.

Run this against VerichatDB signed in as the Microsoft Entra admin of
verichat-server-2026. It cannot be run by the identity it creates.

Order matters. An App Service managed identity does not exist in Entra until
Identity -> System assigned managed identity is switched ON for
verichat-api-aqcbgydhhbgbgha5, and CREATE USER ... FROM EXTERNAL PROVIDER resolves
the name against Entra when the statement runs. Executing this before the switch
fails with "principal name not found" and creates nothing.

The user is named after the App Service because that is how the platform registers
its identity in the directory. Renaming the site breaks the match and the user has
to be recreated under the new name.

Reader and writer are enough for a running API. The startup migration in Program.cs
takes __EFMigrationsHistory as a read and only writes the lock table, so it does not
need DDL rights on an already-migrated database. If a future deployment has to create
or alter tables from the app, grant db_owner instead of adding roles one at a time.
*/

IF NOT EXISTS (
    SELECT 1 FROM sys.database_principals
    WHERE name = N'verichat-api-aqcbgydhhbgbgha5' AND type = 'E')
BEGIN
    CREATE USER [verichat-api-aqcbgydhhbgbgha5] FROM EXTERNAL PROVIDER;
    PRINT 'Created the database user for the App Service identity.';
END
ELSE
    PRINT 'The database user for the App Service identity already exists.';
GO

IF NOT IS_ROLEMEMBER('db_datareader', N'verichat-api-aqcbgydhhbgbgha5')
    ALTER ROLE db_datareader ADD MEMBER [verichat-api-aqcbgydhhbgbgha5];
GO

IF NOT IS_ROLEMEMBER('db_datawriter', N'verichat-api-aqcbgydhhbgbgha5')
    ALTER ROLE db_datawriter ADD MEMBER [verichat-api-aqcbgydhhbgbgha5];
GO

SELECT name, type_desc
FROM sys.database_principals
WHERE type = 'E'
  AND name NOT IN ('MS_XE$', '##MS_AgentSigningCertificate##', '##MS_PolicyEventProcessingLogin##');
GO
