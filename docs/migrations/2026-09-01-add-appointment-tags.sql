-- Schema de tags de agendamento (AddAppointmentTags).
-- Idempotente. Não aplica automaticamente — execute manualmente no SQL Server.
-- Depois reinicie a API para o PermissionSeeder registrar tag_agendamento.*.

IF OBJECT_ID(N'dbo.tag_agendamento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tag_agendamento
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        nome nvarchar(80) NOT NULL,
        cor nvarchar(20) NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_tag_agendamento PRIMARY KEY (id),
        CONSTRAINT fk_tag_agendamento_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id)
    );

    CREATE UNIQUE INDEX ix_tag_agendamento_empresa_id_nome
        ON dbo.tag_agendamento (empresa_id, nome);
END
GO

IF OBJECT_ID(N'dbo.agendamento_tag', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.agendamento_tag
    (
        id uniqueidentifier NOT NULL,
        agendamento_id uniqueidentifier NOT NULL,
        tag_agendamento_id uniqueidentifier NOT NULL,
        ordem int NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_agendamento_tag PRIMARY KEY (id),
        CONSTRAINT fk_agendamento_tag_agendamento_agendamento_id
            FOREIGN KEY (agendamento_id) REFERENCES dbo.agendamento (id)
            ON DELETE CASCADE,
        CONSTRAINT fk_agendamento_tag_tag_agendamento_tag_agendamento_id
            FOREIGN KEY (tag_agendamento_id) REFERENCES dbo.tag_agendamento (id)
    );

    CREATE INDEX ix_agendamento_tag_agendamento_id_ordem
        ON dbo.agendamento_tag (agendamento_id, ordem);

    CREATE UNIQUE INDEX ix_agendamento_tag_agendamento_id_tag_agendamento_id
        ON dbo.agendamento_tag (agendamento_id, tag_agendamento_id);

    CREATE INDEX ix_agendamento_tag_tag_agendamento_id
        ON dbo.agendamento_tag (tag_agendamento_id);
END
GO

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM dbo.__EFMigrationsHistory
       WHERE MigrationId = N'20260901181350_AddAppointmentTags'
   )
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260901181350_AddAppointmentTags', N'10.0.9');
END
GO
