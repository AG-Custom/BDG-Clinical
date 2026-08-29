-- Seed do módulo PRONTUARIO: catalogo, permissões e licença por empresa.
-- Idempotente. Execute depois de 2026-08-28-add-clinical-records.sql.

IF NOT EXISTS (SELECT 1 FROM dbo.modulo_sistema WHERE codigo = N'PRONTUARIO')
BEGIN
    INSERT INTO dbo.modulo_sistema (id, nome, codigo, descricao, ativo, criado_em, atualizado_em)
    VALUES (
        NEWID(),
        N'Prontuário',
        N'PRONTUARIO',
        N'Prontuário clínico, anamnese e avaliações',
        1,
        SYSUTCDATETIME(),
        NULL
    );
END
GO

MERGE dbo.permissao_sistema AS destino
USING (
    VALUES
        (N'prontuario.visualizar', N'Visualizar prontuário', N'Prontuário', N'PRONTUARIO', 10, CAST(NULL AS nvarchar(120))),
        (N'prontuario.atendimento.criar', N'Iniciar atendimento clínico', N'Prontuário', N'PRONTUARIO', 11, NULL),
        (N'prontuario.atendimento.editar', N'Editar atendimento clínico', N'Prontuário', N'PRONTUARIO', 12, NULL),
        (N'prontuario.anamnese.editar', N'Editar anamnese', N'Prontuário', N'PRONTUARIO', 13, NULL),
        (N'prontuario.modelo_anamnese.gerenciar', N'Gerenciar modelos de anamnese', N'Prontuário', N'PRONTUARIO', 14, NULL),
        (N'prontuario.avaliacao.criar', N'Criar avaliação corporal', N'Prontuário', N'PRONTUARIO', 15, NULL),
        (N'prontuario.exame.enviar', N'Enviar exames', N'Prontuário', N'PRONTUARIO', 16, NULL),
        (N'prontuario.foto.enviar', N'Enviar fotos', N'Prontuário', N'PRONTUARIO', 17, NULL),
        (N'prontuario.documento.editar', N'Editar documentos clínicos', N'Prontuário', N'PRONTUARIO', 18, NULL),
        (N'prontuario.anotacao.criar', N'Criar anotação clínica', N'Prontuário', N'PRONTUARIO', 19, NULL)
) AS origem (chave, descricao, categoria, modulo_codigo, ordem, chave_pai)
    ON destino.chave = origem.chave
WHEN NOT MATCHED THEN
    INSERT (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em, atualizado_em)
    VALUES (NEWID(), origem.chave, origem.descricao, origem.categoria, origem.modulo_codigo, origem.ordem, origem.chave_pai, SYSUTCDATETIME(), NULL)
WHEN MATCHED AND (
        destino.descricao <> origem.descricao
        OR destino.categoria <> origem.categoria
        OR destino.modulo_codigo <> origem.modulo_codigo
        OR destino.ordem <> origem.ordem
        OR ISNULL(destino.chave_pai, N'') <> ISNULL(origem.chave_pai, N'')
    ) THEN
    UPDATE SET
        descricao = origem.descricao,
        categoria = origem.categoria,
        modulo_codigo = origem.modulo_codigo,
        ordem = origem.ordem,
        chave_pai = origem.chave_pai,
        atualizado_em = SYSUTCDATETIME();
GO

INSERT INTO dbo.licenca_modulo (id, empresa_id, modulo_id, status, data_inicio, data_fim, valor, criado_em, atualizado_em)
SELECT
    NEWID(),
    empresa.id,
    modulo.id,
    N'Ativo',
    SYSUTCDATETIME(),
    NULL,
    0,
    SYSUTCDATETIME(),
    NULL
FROM dbo.empresa AS empresa
CROSS JOIN dbo.modulo_sistema AS modulo
WHERE modulo.codigo = N'PRONTUARIO'
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.licenca_modulo AS licenca
      WHERE licenca.empresa_id = empresa.id
        AND licenca.modulo_id = modulo.id
  );
GO

INSERT INTO dbo.cargo_permissao_item (id, cargo_id, permission_key, criado_em, atualizado_em)
SELECT
    NEWID(),
    cargo.id,
    N'prontuario.*',
    SYSUTCDATETIME(),
    NULL
FROM dbo.cargo AS cargo
WHERE EXISTS (
        SELECT 1
        FROM dbo.cargo_permissao_item AS item
        WHERE item.cargo_id = cargo.id
          AND item.permission_key IN (N'paciente.visualizar', N'paciente.*')
    )
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.cargo_permissao_item AS item
        WHERE item.cargo_id = cargo.id
          AND item.permission_key = N'prontuario.*'
    );
GO
