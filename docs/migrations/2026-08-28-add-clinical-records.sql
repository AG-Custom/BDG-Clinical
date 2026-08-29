-- Schema do módulo Prontuário (AddClinicalRecords).
-- Idempotente. Não aplica automaticamente — execute manualmente no SQL Server.
-- Depois rode: 2026-08-28-seed-prontuario-modulo-permissoes.sql

IF COL_LENGTH('dbo.paciente', 'sexo') IS NULL
BEGIN
    ALTER TABLE dbo.paciente
        ADD sexo nvarchar(20) NULL;
END
GO

IF OBJECT_ID(N'dbo.modelo_anamnese', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.modelo_anamnese
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        nome nvarchar(180) NOT NULL,
        especialidade nvarchar(120) NULL,
        schema_json nvarchar(max) NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_modelo_anamnese PRIMARY KEY (id),
        CONSTRAINT fk_modelo_anamnese_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id)
    );

    CREATE UNIQUE INDEX ix_modelo_anamnese_empresa_id_nome
        ON dbo.modelo_anamnese (empresa_id, nome);
END
GO

IF OBJECT_ID(N'dbo.prontuario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.prontuario
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        alergias nvarchar(4000) NULL,
        alertas nvarchar(4000) NULL,
        observacao nvarchar(4000) NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_prontuario PRIMARY KEY (id),
        CONSTRAINT fk_prontuario_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_prontuario_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id)
    );

    CREATE UNIQUE INDEX ix_prontuario_empresa_id_paciente_id
        ON dbo.prontuario (empresa_id, paciente_id);

    CREATE INDEX ix_prontuario_paciente_id
        ON dbo.prontuario (paciente_id);
END
GO

IF OBJECT_ID(N'dbo.atendimento_clinico', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.atendimento_clinico
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        prontuario_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        agendamento_id uniqueidentifier NULL,
        data_inicio datetime2 NOT NULL,
        data_fim datetime2 NULL,
        status nvarchar(40) NOT NULL,
        observacao nvarchar(4000) NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_atendimento_clinico PRIMARY KEY (id),
        CONSTRAINT fk_atendimento_clinico_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_atendimento_clinico_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_atendimento_clinico_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_atendimento_clinico_prontuario_prontuario_id
            FOREIGN KEY (prontuario_id) REFERENCES dbo.prontuario (id),
        CONSTRAINT fk_atendimento_clinico_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_atendimento_clinico_empresa_id_paciente_id_data_inicio
        ON dbo.atendimento_clinico (empresa_id, paciente_id, data_inicio);

    CREATE INDEX ix_atendimento_clinico_funcionario_id
        ON dbo.atendimento_clinico (funcionario_id);

    CREATE INDEX ix_atendimento_clinico_paciente_id
        ON dbo.atendimento_clinico (paciente_id);

    CREATE INDEX ix_atendimento_clinico_prontuario_id
        ON dbo.atendimento_clinico (prontuario_id);

    CREATE INDEX ix_atendimento_clinico_unidade_id
        ON dbo.atendimento_clinico (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.anexo_clinico', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.anexo_clinico
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        tipo nvarchar(40) NOT NULL,
        nome nvarchar(180) NOT NULL,
        data_documento datetime2 NOT NULL,
        observacao nvarchar(2000) NULL,
        categoria_exame nvarchar(40) NULL,
        categoria_documento nvarchar(40) NULL,
        nome_arquivo nvarchar(260) NOT NULL,
        content_type nvarchar(120) NOT NULL,
        object_key nvarchar(500) NOT NULL,
        tamanho_bytes bigint NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_anexo_clinico PRIMARY KEY (id),
        CONSTRAINT fk_anexo_clinico_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_anexo_clinico_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_anexo_clinico_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_anexo_clinico_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_anexo_clinico_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_anexo_clinico_atendimento_clinico_id ON dbo.anexo_clinico (atendimento_clinico_id);
    CREATE INDEX ix_anexo_clinico_empresa_id ON dbo.anexo_clinico (empresa_id);
    CREATE INDEX ix_anexo_clinico_funcionario_id ON dbo.anexo_clinico (funcionario_id);
    CREATE INDEX ix_anexo_clinico_paciente_id ON dbo.anexo_clinico (paciente_id);
    CREATE INDEX ix_anexo_clinico_unidade_id ON dbo.anexo_clinico (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.anotacao_clinica', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.anotacao_clinica
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        tipo nvarchar(40) NOT NULL,
        texto nvarchar(max) NOT NULL,
        data datetime2 NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_anotacao_clinica PRIMARY KEY (id),
        CONSTRAINT fk_anotacao_clinica_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_anotacao_clinica_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_anotacao_clinica_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_anotacao_clinica_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_anotacao_clinica_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_anotacao_clinica_atendimento_clinico_id ON dbo.anotacao_clinica (atendimento_clinico_id);
    CREATE INDEX ix_anotacao_clinica_empresa_id ON dbo.anotacao_clinica (empresa_id);
    CREATE INDEX ix_anotacao_clinica_funcionario_id ON dbo.anotacao_clinica (funcionario_id);
    CREATE INDEX ix_anotacao_clinica_paciente_id ON dbo.anotacao_clinica (paciente_id);
    CREATE INDEX ix_anotacao_clinica_unidade_id ON dbo.anotacao_clinica (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.avaliacao_corporal', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.avaliacao_corporal
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        data_avaliacao datetime2 NOT NULL,
        altura_cm decimal(10, 2) NULL,
        peso_kg decimal(10, 3) NULL,
        imc decimal(10, 2) NULL,
        classificacao_imc nvarchar(40) NULL,
        peso_ideal_kg decimal(10, 3) NULL,
        cintura_cm decimal(10, 2) NULL,
        quadril_cm decimal(10, 2) NULL,
        relacao_cintura_quadril decimal(10, 3) NULL,
        percentual_massa_gorda decimal(10, 2) NULL,
        massa_gorda_kg decimal(10, 3) NULL,
        percentual_massa_magra decimal(10, 2) NULL,
        massa_magra_kg decimal(10, 3) NULL,
        percentual_agua decimal(10, 2) NULL,
        protocolo_prega nvarchar(40) NULL,
        bioimpedancia_json nvarchar(max) NULL,
        circunferencias_json nvarchar(max) NULL,
        pregas_json nvarchar(max) NULL,
        observacao nvarchar(4000) NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_avaliacao_corporal PRIMARY KEY (id),
        CONSTRAINT fk_avaliacao_corporal_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_avaliacao_corporal_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_avaliacao_corporal_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_avaliacao_corporal_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_avaliacao_corporal_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_avaliacao_corporal_atendimento_clinico_id ON dbo.avaliacao_corporal (atendimento_clinico_id);
    CREATE INDEX ix_avaliacao_corporal_empresa_id_paciente_id_data_avaliacao
        ON dbo.avaliacao_corporal (empresa_id, paciente_id, data_avaliacao);
    CREATE INDEX ix_avaliacao_corporal_funcionario_id ON dbo.avaliacao_corporal (funcionario_id);
    CREATE INDEX ix_avaliacao_corporal_paciente_id ON dbo.avaliacao_corporal (paciente_id);
    CREATE INDEX ix_avaliacao_corporal_unidade_id ON dbo.avaliacao_corporal (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.calculo_energetico', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.calculo_energetico
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        perfil nvarchar(40) NOT NULL,
        protocolo nvarchar(40) NOT NULL,
        nivel_atividade nvarchar(40) NOT NULL,
        fator_injuria decimal(8, 3) NULL,
        peso_kg decimal(10, 3) NOT NULL,
        altura_cm decimal(10, 2) NOT NULL,
        idade int NOT NULL,
        massa_magra_kg decimal(10, 3) NULL,
        peso_desejado_kg decimal(10, 3) NOT NULL,
        tempo_dias int NOT NULL,
        atividades_json nvarchar(max) NULL,
        gasto_energetico_basal decimal(12, 2) NOT NULL,
        gasto_energetico_total decimal(12, 2) NOT NULL,
        ajuste_calorico_diario decimal(12, 2) NOT NULL,
        meta_calorica_diaria decimal(12, 2) NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_calculo_energetico PRIMARY KEY (id),
        CONSTRAINT fk_calculo_energetico_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_calculo_energetico_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_calculo_energetico_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_calculo_energetico_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_calculo_energetico_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_calculo_energetico_atendimento_clinico_id ON dbo.calculo_energetico (atendimento_clinico_id);
    CREATE INDEX ix_calculo_energetico_empresa_id ON dbo.calculo_energetico (empresa_id);
    CREATE INDEX ix_calculo_energetico_funcionario_id ON dbo.calculo_energetico (funcionario_id);
    CREATE INDEX ix_calculo_energetico_paciente_id ON dbo.calculo_energetico (paciente_id);
    CREATE INDEX ix_calculo_energetico_unidade_id ON dbo.calculo_energetico (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.evento_clinico', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.evento_clinico
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NULL,
        tipo nvarchar(40) NOT NULL,
        titulo nvarchar(180) NOT NULL,
        resumo nvarchar(1000) NULL,
        entidade nvarchar(80) NOT NULL,
        registro_id uniqueidentifier NOT NULL,
        dados_anteriores nvarchar(4000) NULL,
        dados_novos nvarchar(4000) NULL,
        funcionario_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        data datetime2 NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_evento_clinico PRIMARY KEY (id),
        CONSTRAINT fk_evento_clinico_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_evento_clinico_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_evento_clinico_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_evento_clinico_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_evento_clinico_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_evento_clinico_atendimento_clinico_id ON dbo.evento_clinico (atendimento_clinico_id);
    CREATE INDEX ix_evento_clinico_empresa_id_paciente_id_data
        ON dbo.evento_clinico (empresa_id, paciente_id, data);
    CREATE INDEX ix_evento_clinico_funcionario_id ON dbo.evento_clinico (funcionario_id);
    CREATE INDEX ix_evento_clinico_paciente_id ON dbo.evento_clinico (paciente_id);
    CREATE INDEX ix_evento_clinico_unidade_id ON dbo.evento_clinico (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.registro_anamnese', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.registro_anamnese
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        modelo_anamnese_id uniqueidentifier NOT NULL,
        schema_snapshot_json nvarchar(max) NOT NULL,
        respostas_json nvarchar(max) NOT NULL,
        versao_atual int NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_registro_anamnese PRIMARY KEY (id),
        CONSTRAINT fk_registro_anamnese_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_registro_anamnese_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_registro_anamnese_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_registro_anamnese_modelo_anamnese_modelo_anamnese_id
            FOREIGN KEY (modelo_anamnese_id) REFERENCES dbo.modelo_anamnese (id),
        CONSTRAINT fk_registro_anamnese_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_registro_anamnese_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_registro_anamnese_atendimento_clinico_id ON dbo.registro_anamnese (atendimento_clinico_id);
    CREATE INDEX ix_registro_anamnese_empresa_id ON dbo.registro_anamnese (empresa_id);
    CREATE INDEX ix_registro_anamnese_funcionario_id ON dbo.registro_anamnese (funcionario_id);
    CREATE INDEX ix_registro_anamnese_modelo_anamnese_id ON dbo.registro_anamnese (modelo_anamnese_id);
    CREATE INDEX ix_registro_anamnese_paciente_id ON dbo.registro_anamnese (paciente_id);
    CREATE INDEX ix_registro_anamnese_unidade_id ON dbo.registro_anamnese (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.regra_bolso', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.regra_bolso
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        objetivo nvarchar(40) NOT NULL,
        peso_kg decimal(10, 3) NOT NULL,
        gasto_energetico_total decimal(12, 2) NOT NULL,
        calorias decimal(12, 2) NOT NULL,
        proteinas_g decimal(10, 2) NOT NULL,
        carboidratos_g decimal(10, 2) NOT NULL,
        gorduras_g decimal(10, 2) NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_regra_bolso PRIMARY KEY (id),
        CONSTRAINT fk_regra_bolso_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_regra_bolso_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_regra_bolso_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_regra_bolso_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_regra_bolso_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_regra_bolso_atendimento_clinico_id ON dbo.regra_bolso (atendimento_clinico_id);
    CREATE INDEX ix_regra_bolso_empresa_id ON dbo.regra_bolso (empresa_id);
    CREATE INDEX ix_regra_bolso_funcionario_id ON dbo.regra_bolso (funcionario_id);
    CREATE INDEX ix_regra_bolso_paciente_id ON dbo.regra_bolso (paciente_id);
    CREATE INDEX ix_regra_bolso_unidade_id ON dbo.regra_bolso (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.foto_comparativa', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.foto_comparativa
    (
        id uniqueidentifier NOT NULL,
        empresa_id uniqueidentifier NOT NULL,
        atendimento_clinico_id uniqueidentifier NOT NULL,
        paciente_id uniqueidentifier NOT NULL,
        unidade_id uniqueidentifier NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        avaliacao_corporal_id uniqueidentifier NULL,
        categoria nvarchar(40) NOT NULL,
        data_captura datetime2 NOT NULL,
        observacao nvarchar(2000) NULL,
        nome_arquivo nvarchar(260) NOT NULL,
        content_type nvarchar(120) NOT NULL,
        object_key nvarchar(500) NOT NULL,
        tamanho_bytes bigint NOT NULL,
        ativo bit NOT NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_foto_comparativa PRIMARY KEY (id),
        CONSTRAINT fk_foto_comparativa_atendimento_clinico_atendimento_clinico_id
            FOREIGN KEY (atendimento_clinico_id) REFERENCES dbo.atendimento_clinico (id),
        CONSTRAINT fk_foto_comparativa_avaliacao_corporal_avaliacao_corporal_id
            FOREIGN KEY (avaliacao_corporal_id) REFERENCES dbo.avaliacao_corporal (id),
        CONSTRAINT fk_foto_comparativa_empresa_empresa_id
            FOREIGN KEY (empresa_id) REFERENCES dbo.empresa (id),
        CONSTRAINT fk_foto_comparativa_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_foto_comparativa_paciente_paciente_id
            FOREIGN KEY (paciente_id) REFERENCES dbo.paciente (id),
        CONSTRAINT fk_foto_comparativa_unidade_unidade_id
            FOREIGN KEY (unidade_id) REFERENCES dbo.unidade (id)
    );

    CREATE INDEX ix_foto_comparativa_atendimento_clinico_id ON dbo.foto_comparativa (atendimento_clinico_id);
    CREATE INDEX ix_foto_comparativa_avaliacao_corporal_id ON dbo.foto_comparativa (avaliacao_corporal_id);
    CREATE INDEX ix_foto_comparativa_empresa_id ON dbo.foto_comparativa (empresa_id);
    CREATE INDEX ix_foto_comparativa_funcionario_id ON dbo.foto_comparativa (funcionario_id);
    CREATE INDEX ix_foto_comparativa_paciente_id ON dbo.foto_comparativa (paciente_id);
    CREATE INDEX ix_foto_comparativa_unidade_id ON dbo.foto_comparativa (unidade_id);
END
GO

IF OBJECT_ID(N'dbo.registro_anamnese_versao', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.registro_anamnese_versao
    (
        id uniqueidentifier NOT NULL,
        registro_anamnese_id uniqueidentifier NOT NULL,
        versao int NOT NULL,
        funcionario_id uniqueidentifier NOT NULL,
        respostas_json nvarchar(max) NOT NULL,
        resumo_alteracao nvarchar(1000) NULL,
        respostas_anteriores_json nvarchar(max) NULL,
        criado_em datetime2 NOT NULL,
        atualizado_em datetime2 NULL,
        CONSTRAINT pk_registro_anamnese_versao PRIMARY KEY (id),
        CONSTRAINT fk_registro_anamnese_versao_funcionario_funcionario_id
            FOREIGN KEY (funcionario_id) REFERENCES dbo.funcionario (id),
        CONSTRAINT fk_registro_anamnese_versao_registro_anamnese_registro_anamnese_id
            FOREIGN KEY (registro_anamnese_id) REFERENCES dbo.registro_anamnese (id) ON DELETE CASCADE
    );

    CREATE INDEX ix_registro_anamnese_versao_funcionario_id
        ON dbo.registro_anamnese_versao (funcionario_id);

    CREATE INDEX ix_registro_anamnese_versao_registro_anamnese_id
        ON dbo.registro_anamnese_versao (registro_anamnese_id);
END
GO

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM dbo.__EFMigrationsHistory
       WHERE MigrationId = N'20260828190746_AddClinicalRecords'
   )
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260828190746_AddClinicalRecords', N'10.0.9');
END
GO
