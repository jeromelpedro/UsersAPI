IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Usuarios] (
    [Id] nvarchar(450) NOT NULL,
    [Nome] nvarchar(150) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [Senha] nvarchar(max) NOT NULL,
    [Role] nvarchar(5) NOT NULL,
    [DataCriacaoUsuario] datetime2 NOT NULL,
    [DataAlteracaoSenha] datetime2 NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251127042846_InitialCreate', N'9.0.11');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'Id');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Usuarios] ALTER COLUMN [Id] uniqueidentifier NOT NULL;

CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier NOT NULL,
    [TableName] nvarchar(max) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [KeyValues] nvarchar(max) NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [User] nvarchar(max) NULL,
    [CorrelationId] nvarchar(max) NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260223030417_AddAuditLog', N'9.0.11');

COMMIT;
GO

