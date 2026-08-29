-- Schema de item_compra_paciente (AddItemCompraPaciente).
-- Idempotente. Não aplica automaticamente — execute manualmente no SQL Server.
-- Só cria tabela/índices. O backfill dos itens antigos é o POST /api/patient-purchases/reconcile-items.

IF OBJECT_ID(N'dbo.item_compra_paciente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.item_compra_paciente
    (
        id uniqueidentifier NOT NULL,
        compra_paciente_id uniqueidentifier NOT NULL,
        produto_id uniqueidentifier NOT NULL,
        quantidade_contratada decimal(18, 4) NOT NULL,
        quantidade_utilizada_base decimal(18, 4) NOT NULL,
        unidade_medida nvarchar(30) NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_item_compra_paciente PRIMARY KEY (id),
        CONSTRAINT fk_item_compra_paciente_compra_paciente_compra_paciente_id
            FOREIGN KEY (compra_paciente_id) REFERENCES dbo.compra_paciente (id)
            ON DELETE CASCADE,
        CONSTRAINT fk_item_compra_paciente_produto_produto_id
            FOREIGN KEY (produto_id) REFERENCES dbo.produto (id)
    );

    CREATE UNIQUE INDEX ix_item_compra_paciente_compra_paciente_id_produto_id
        ON dbo.item_compra_paciente (compra_paciente_id, produto_id);

    CREATE INDEX ix_item_compra_paciente_produto_id
        ON dbo.item_compra_paciente (produto_id);
END
GO

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM dbo.__EFMigrationsHistory
       WHERE MigrationId = N'20260829173830_AddItemCompraPaciente'
   )
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260829173830_AddItemCompraPaciente', N'10.0.9');
END
GO
