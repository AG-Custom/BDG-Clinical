/*
  BGD Clinical - Schema inicial
  Banco: Microsoft SQL Server
  Uso: executar em banco vazio (copiar e colar na interface do SSMS / Azure Data Studio)

  Ordem respeita dependencias de chave estrangeira.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE empresa (
    id              UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(200)    NOT NULL,
    cnpj            NVARCHAR(20)     NULL,
    telefone        NVARCHAR(30)     NULL,
    email           NVARCHAR(200)    NULL,
    logo            NVARCHAR(500)    NULL,
    cor_principal   NVARCHAR(20)     NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_empresa PRIMARY KEY (id)
);
GO

CREATE TABLE modulo_sistema (
    id              UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(120)    NOT NULL,
    codigo          NVARCHAR(80)     NOT NULL,
    descricao       NVARCHAR(500)    NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_modulo_sistema PRIMARY KEY (id)
);
GO

CREATE TABLE cargo (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(120)    NOT NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_cargo PRIMARY KEY (id),
    CONSTRAINT fk_cargo_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE forma_pagamento (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(120)    NOT NULL,
    tipo            NVARCHAR(80)     NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_forma_pagamento PRIMARY KEY (id),
    CONSTRAINT fk_forma_pagamento_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE fornecedor (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(180)    NOT NULL,
    telefone        NVARCHAR(30)     NULL,
    email           NVARCHAR(200)    NULL,
    cnpj            NVARCHAR(20)     NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_fornecedor PRIMARY KEY (id),
    CONSTRAINT fk_fornecedor_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE pacote (
    id                      UNIQUEIDENTIFIER NOT NULL,
    empresa_id              UNIQUEIDENTIFIER NOT NULL,
    nome                    NVARCHAR(160)    NOT NULL,
    descricao               NVARCHAR(1000)   NULL,
    quantidade_aplicacoes   INT              NOT NULL,
    valor                   DECIMAL(18, 2)   NOT NULL,
    ativo                   BIT              NOT NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_pacote PRIMARY KEY (id),
    CONSTRAINT fk_pacote_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE produto (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(160)    NOT NULL,
    tipo            NVARCHAR(40)     NOT NULL,
    unidade_medida  NVARCHAR(30)     NOT NULL,
    estoque_minimo  DECIMAL(18, 4)   NOT NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_produto PRIMARY KEY (id),
    CONSTRAINT fk_produto_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE sintoma (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(120)    NOT NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_sintoma PRIMARY KEY (id),
    CONSTRAINT fk_sintoma_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE unidade (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(160)    NOT NULL,
    endereco        NVARCHAR(500)    NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_unidade PRIMARY KEY (id),
    CONSTRAINT fk_unidade_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE licenca_modulo (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    modulo_id       UNIQUEIDENTIFIER NOT NULL,
    status          NVARCHAR(40)     NOT NULL,
    data_inicio     DATETIME2        NOT NULL,
    data_fim        DATETIME2        NULL,
    valor           DECIMAL(18, 2)   NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_licenca_modulo PRIMARY KEY (id),
    CONSTRAINT fk_licenca_modulo_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_licenca_modulo_modulo_sistema_modulo_id FOREIGN KEY (modulo_id) REFERENCES modulo_sistema (id)
);
GO

CREATE TABLE item_pacote (
    id                  UNIQUEIDENTIFIER NOT NULL,
    pacote_id           UNIQUEIDENTIFIER NOT NULL,
    produto_id          UNIQUEIDENTIFIER NOT NULL,
    quantidade_total    DECIMAL(18, 4)   NOT NULL,
    unidade_medida      NVARCHAR(30)     NOT NULL,
    criado_em           DATETIME2        NOT NULL,
    atualizado_em       DATETIME2        NULL,
    CONSTRAINT pk_item_pacote PRIMARY KEY (id),
    CONSTRAINT fk_item_pacote_pacote_pacote_id FOREIGN KEY (pacote_id) REFERENCES pacote (id),
    CONSTRAINT fk_item_pacote_produto_produto_id FOREIGN KEY (produto_id) REFERENCES produto (id)
);
GO

CREATE TABLE funcionario (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    unidade_id      UNIQUEIDENTIFIER NOT NULL,
    nome            NVARCHAR(160)    NOT NULL,
    telefone        NVARCHAR(30)     NULL,
    email           NVARCHAR(200)    NULL,
    cargo_id        UNIQUEIDENTIFIER NULL,
    flag_aplicador  BIT              NOT NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_funcionario PRIMARY KEY (id),
    CONSTRAINT fk_funcionario_cargo_cargo_id FOREIGN KEY (cargo_id) REFERENCES cargo (id),
    CONSTRAINT fk_funcionario_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_funcionario_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id)
);
GO

CREATE TABLE paciente (
    id                  UNIQUEIDENTIFIER NOT NULL,
    empresa_id          UNIQUEIDENTIFIER NOT NULL,
    unidade_id          UNIQUEIDENTIFIER NOT NULL,
    nome                NVARCHAR(180)    NOT NULL,
    cpf                 NVARCHAR(20)     NULL,
    telefone            NVARCHAR(30)     NULL,
    email               NVARCHAR(200)    NULL,
    data_nascimento     DATE             NULL,
    observacao          NVARCHAR(2000)   NULL,
    ativo               BIT              NOT NULL,
    criado_em           DATETIME2        NOT NULL,
    atualizado_em       DATETIME2        NULL,
    CONSTRAINT pk_paciente PRIMARY KEY (id),
    CONSTRAINT fk_paciente_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_paciente_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id)
);
GO

CREATE TABLE pedido_fornecedor (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    fornecedor_id   UNIQUEIDENTIFIER NOT NULL,
    unidade_id      UNIQUEIDENTIFIER NOT NULL,
    tipo_pedido     NVARCHAR(40)     NOT NULL,
    data_pedido     DATETIME2        NOT NULL,
    status          NVARCHAR(40)     NOT NULL,
    valor_total     DECIMAL(18, 2)   NOT NULL,
    observacao      NVARCHAR(2000)   NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_pedido_fornecedor PRIMARY KEY (id),
    CONSTRAINT fk_pedido_fornecedor_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_pedido_fornecedor_fornecedor_fornecedor_id FOREIGN KEY (fornecedor_id) REFERENCES fornecedor (id),
    CONSTRAINT fk_pedido_fornecedor_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id)
);
GO

CREATE TABLE disponibilidade_funcionario (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    funcionario_id  UNIQUEIDENTIFIER NOT NULL,
    unidade_id      UNIQUEIDENTIFIER NOT NULL,
    dia_semana      NVARCHAR(40)     NOT NULL,
    hora_inicio     TIME             NOT NULL,
    hora_fim        TIME             NOT NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_disponibilidade_funcionario PRIMARY KEY (id),
    CONSTRAINT fk_disponibilidade_funcionario_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_disponibilidade_funcionario_funcionario_funcionario_id FOREIGN KEY (funcionario_id) REFERENCES funcionario (id),
    CONSTRAINT fk_disponibilidade_funcionario_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id)
);
GO

CREATE TABLE usuario (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    funcionario_id  UNIQUEIDENTIFIER NULL,
    nome            NVARCHAR(160)    NOT NULL,
    email_login     NVARCHAR(200)    NOT NULL,
    senha_hash      NVARCHAR(500)    NOT NULL,
    auth_provider   NVARCHAR(60)     NOT NULL,
    google_id       NVARCHAR(200)    NULL,
    tipo_usuario    NVARCHAR(40)     NOT NULL,
    ativo           BIT              NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_usuario PRIMARY KEY (id),
    CONSTRAINT fk_usuario_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_usuario_funcionario_funcionario_id FOREIGN KEY (funcionario_id) REFERENCES funcionario (id)
);
GO

CREATE TABLE compra_paciente (
    id                      UNIQUEIDENTIFIER NOT NULL,
    empresa_id              UNIQUEIDENTIFIER NOT NULL,
    paciente_id             UNIQUEIDENTIFIER NOT NULL,
    pacote_id               UNIQUEIDENTIFIER NOT NULL,
    unidade_id              UNIQUEIDENTIFIER NOT NULL,
    data_compra             DATETIME2        NOT NULL,
    quantidade_aplicacoes   INT              NOT NULL,
    status                  NVARCHAR(40)     NOT NULL,
    observacao              NVARCHAR(2000)   NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_compra_paciente PRIMARY KEY (id),
    CONSTRAINT fk_compra_paciente_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_compra_paciente_paciente_paciente_id FOREIGN KEY (paciente_id) REFERENCES paciente (id),
    CONSTRAINT fk_compra_paciente_pacote_pacote_id FOREIGN KEY (pacote_id) REFERENCES pacote (id),
    CONSTRAINT fk_compra_paciente_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id)
);
GO

CREATE TABLE item_pedido_fornecedor (
    id                      UNIQUEIDENTIFIER NOT NULL,
    pedido_fornecedor_id    UNIQUEIDENTIFIER NOT NULL,
    produto_id              UNIQUEIDENTIFIER NOT NULL,
    quantidade              DECIMAL(18, 4)   NOT NULL,
    valor_unitario          DECIMAL(18, 2)   NOT NULL,
    valor_total             DECIMAL(18, 2)   NOT NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_item_pedido_fornecedor PRIMARY KEY (id),
    CONSTRAINT fk_item_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id FOREIGN KEY (pedido_fornecedor_id) REFERENCES pedido_fornecedor (id),
    CONSTRAINT fk_item_pedido_fornecedor_produto_produto_id FOREIGN KEY (produto_id) REFERENCES produto (id)
);
GO

CREATE TABLE bloqueio_agenda (
    id              UNIQUEIDENTIFIER NOT NULL,
    empresa_id      UNIQUEIDENTIFIER NOT NULL,
    funcionario_id  UNIQUEIDENTIFIER NOT NULL,
    unidade_id      UNIQUEIDENTIFIER NOT NULL,
    data_inicio     DATETIME2        NOT NULL,
    data_fim        DATETIME2        NOT NULL,
    motivo          NVARCHAR(500)    NOT NULL,
    criado_por_id   UNIQUEIDENTIFIER NOT NULL,
    criado_em       DATETIME2        NOT NULL,
    atualizado_em   DATETIME2        NULL,
    CONSTRAINT pk_bloqueio_agenda PRIMARY KEY (id),
    CONSTRAINT fk_bloqueio_agenda_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_bloqueio_agenda_funcionario_funcionario_id FOREIGN KEY (funcionario_id) REFERENCES funcionario (id),
    CONSTRAINT fk_bloqueio_agenda_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id),
    CONSTRAINT fk_bloqueio_agenda_usuario_criado_por_id FOREIGN KEY (criado_por_id) REFERENCES usuario (id)
);
GO

CREATE TABLE conta_google_conectada (
    id                      UNIQUEIDENTIFIER NOT NULL,
    empresa_id              UNIQUEIDENTIFIER NOT NULL,
    usuario_id              UNIQUEIDENTIFIER NOT NULL,
    funcionario_id          UNIQUEIDENTIFIER NOT NULL,
    google_email            NVARCHAR(200)    NOT NULL,
    google_account_id       NVARCHAR(200)    NOT NULL,
    access_token            NVARCHAR(4000)   NOT NULL,
    refresh_token           NVARCHAR(4000)   NOT NULL,
    escopos_autorizados     NVARCHAR(1000)   NOT NULL,
    ativo                   BIT              NOT NULL,
    conectado_em            DATETIME2        NOT NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_conta_google_conectada PRIMARY KEY (id),
    CONSTRAINT fk_conta_google_conectada_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_conta_google_conectada_funcionario_funcionario_id FOREIGN KEY (funcionario_id) REFERENCES funcionario (id),
    CONSTRAINT fk_conta_google_conectada_usuario_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuario (id)
);
GO

CREATE TABLE log_auditoria (
    id                  UNIQUEIDENTIFIER NOT NULL,
    empresa_id          UNIQUEIDENTIFIER NOT NULL,
    usuario_id          UNIQUEIDENTIFIER NOT NULL,
    entidade            NVARCHAR(160)    NOT NULL,
    registro_id         UNIQUEIDENTIFIER NOT NULL,
    acao                NVARCHAR(60)     NOT NULL,
    dados_anteriores    NVARCHAR(MAX)    NULL,
    dados_novos         NVARCHAR(MAX)    NULL,
    data                DATETIME2        NOT NULL,
    ip                  NVARCHAR(80)     NULL,
    criado_em           DATETIME2        NOT NULL,
    atualizado_em       DATETIME2        NULL,
    CONSTRAINT pk_log_auditoria PRIMARY KEY (id),
    CONSTRAINT fk_log_auditoria_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_log_auditoria_usuario_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuario (id)
);
GO

CREATE TABLE permissao_usuario (
    id                  UNIQUEIDENTIFIER NOT NULL,
    usuario_id          UNIQUEIDENTIFIER NOT NULL,
    modulo_id           UNIQUEIDENTIFIER NOT NULL,
    pode_visualizar     BIT              NOT NULL,
    pode_criar          BIT              NOT NULL,
    pode_editar         BIT              NOT NULL,
    pode_excluir        BIT              NOT NULL,
    criado_em           DATETIME2        NOT NULL,
    atualizado_em       DATETIME2        NULL,
    CONSTRAINT pk_permissao_usuario PRIMARY KEY (id),
    CONSTRAINT fk_permissao_usuario_modulo_sistema_modulo_id FOREIGN KEY (modulo_id) REFERENCES modulo_sistema (id),
    CONSTRAINT fk_permissao_usuario_usuario_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuario (id)
);
GO

CREATE TABLE agendamento (
    id                      UNIQUEIDENTIFIER NOT NULL,
    empresa_id              UNIQUEIDENTIFIER NOT NULL,
    unidade_id              UNIQUEIDENTIFIER NOT NULL,
    paciente_id             UNIQUEIDENTIFIER NOT NULL,
    funcionario_id          UNIQUEIDENTIFIER NOT NULL,
    compra_paciente_id      UNIQUEIDENTIFIER NULL,
    tipo                    NVARCHAR(40)     NOT NULL,
    status                  NVARCHAR(40)     NOT NULL,
    data_inicio             DATETIME2        NOT NULL,
    data_fim                DATETIME2        NOT NULL,
    titulo                  NVARCHAR(180)    NOT NULL,
    observacao              NVARCHAR(2000)   NULL,
    criado_por_id           UNIQUEIDENTIFIER NOT NULL,
    cancelado_por_id        UNIQUEIDENTIFIER NULL,
    motivo_cancelamento     NVARCHAR(500)    NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_agendamento PRIMARY KEY (id),
    CONSTRAINT fk_agendamento_compra_paciente_compra_paciente_id FOREIGN KEY (compra_paciente_id) REFERENCES compra_paciente (id),
    CONSTRAINT fk_agendamento_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_agendamento_funcionario_funcionario_id FOREIGN KEY (funcionario_id) REFERENCES funcionario (id),
    CONSTRAINT fk_agendamento_paciente_paciente_id FOREIGN KEY (paciente_id) REFERENCES paciente (id),
    CONSTRAINT fk_agendamento_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id),
    CONSTRAINT fk_agendamento_usuario_cancelado_por_id FOREIGN KEY (cancelado_por_id) REFERENCES usuario (id),
    CONSTRAINT fk_agendamento_usuario_criado_por_id FOREIGN KEY (criado_por_id) REFERENCES usuario (id)
);
GO

CREATE TABLE conta_receber (
    id                  UNIQUEIDENTIFIER NOT NULL,
    empresa_id          UNIQUEIDENTIFIER NOT NULL,
    paciente_id         UNIQUEIDENTIFIER NOT NULL,
    compra_paciente_id  UNIQUEIDENTIFIER NULL,
    data_vencimento     DATE             NOT NULL,
    valor_total         DECIMAL(18, 2)   NOT NULL,
    status              NVARCHAR(40)     NOT NULL,
    observacao          NVARCHAR(2000)   NULL,
    criado_em           DATETIME2        NOT NULL,
    atualizado_em       DATETIME2        NULL,
    CONSTRAINT pk_conta_receber PRIMARY KEY (id),
    CONSTRAINT fk_conta_receber_compra_paciente_compra_paciente_id FOREIGN KEY (compra_paciente_id) REFERENCES compra_paciente (id),
    CONSTRAINT fk_conta_receber_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_conta_receber_paciente_paciente_id FOREIGN KEY (paciente_id) REFERENCES paciente (id)
);
GO

CREATE TABLE agenda_google (
    id                          UNIQUEIDENTIFIER NOT NULL,
    empresa_id                  UNIQUEIDENTIFIER NOT NULL,
    conta_google_conectada_id   UNIQUEIDENTIFIER NOT NULL,
    google_calendar_id          NVARCHAR(300)    NOT NULL,
    nome                        NVARCHAR(200)    NOT NULL,
    principal                   BIT              NOT NULL,
    ativo                       BIT              NOT NULL,
    criado_em                   DATETIME2        NOT NULL,
    atualizado_em               DATETIME2        NULL,
    CONSTRAINT pk_agenda_google PRIMARY KEY (id),
    CONSTRAINT fk_agenda_google_conta_google_conectada_conta_google_conectada_id FOREIGN KEY (conta_google_conectada_id) REFERENCES conta_google_conectada (id),
    CONSTRAINT fk_agenda_google_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE aplicacao_paciente (
    id                      UNIQUEIDENTIFIER NOT NULL,
    empresa_id              UNIQUEIDENTIFIER NOT NULL,
    paciente_id             UNIQUEIDENTIFIER NOT NULL,
    compra_paciente_id      UNIQUEIDENTIFIER NOT NULL,
    produto_id              UNIQUEIDENTIFIER NOT NULL,
    funcionario_id          UNIQUEIDENTIFIER NOT NULL,
    unidade_id              UNIQUEIDENTIFIER NOT NULL,
    agendamento_id          UNIQUEIDENTIFIER NULL,
    data_aplicacao          DATETIME2        NOT NULL,
    quantidade_utilizada    DECIMAL(18, 4)   NOT NULL,
    peso                    DECIMAL(10, 3)   NULL,
    observacao              NVARCHAR(2000)   NULL,
    realizado               BIT              NOT NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_aplicacao_paciente PRIMARY KEY (id),
    CONSTRAINT fk_aplicacao_paciente_agendamento_agendamento_id FOREIGN KEY (agendamento_id) REFERENCES agendamento (id),
    CONSTRAINT fk_aplicacao_paciente_compra_paciente_compra_paciente_id FOREIGN KEY (compra_paciente_id) REFERENCES compra_paciente (id),
    CONSTRAINT fk_aplicacao_paciente_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_aplicacao_paciente_funcionario_funcionario_id FOREIGN KEY (funcionario_id) REFERENCES funcionario (id),
    CONSTRAINT fk_aplicacao_paciente_paciente_paciente_id FOREIGN KEY (paciente_id) REFERENCES paciente (id),
    CONSTRAINT fk_aplicacao_paciente_produto_produto_id FOREIGN KEY (produto_id) REFERENCES produto (id),
    CONSTRAINT fk_aplicacao_paciente_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id)
);
GO

CREATE TABLE pagamento_paciente (
    id                  UNIQUEIDENTIFIER NOT NULL,
    empresa_id          UNIQUEIDENTIFIER NOT NULL,
    paciente_id         UNIQUEIDENTIFIER NOT NULL,
    conta_receber_id    UNIQUEIDENTIFIER NOT NULL,
    forma_pagamento_id  UNIQUEIDENTIFIER NOT NULL,
    data_pagamento      DATETIME2        NOT NULL,
    valor_pago          DECIMAL(18, 2)   NOT NULL,
    observacao          NVARCHAR(1000)   NULL,
    criado_em           DATETIME2        NOT NULL,
    atualizado_em       DATETIME2        NULL,
    CONSTRAINT pk_pagamento_paciente PRIMARY KEY (id),
    CONSTRAINT fk_pagamento_paciente_conta_receber_conta_receber_id FOREIGN KEY (conta_receber_id) REFERENCES conta_receber (id),
    CONSTRAINT fk_pagamento_paciente_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_pagamento_paciente_forma_pagamento_forma_pagamento_id FOREIGN KEY (forma_pagamento_id) REFERENCES forma_pagamento (id),
    CONSTRAINT fk_pagamento_paciente_paciente_paciente_id FOREIGN KEY (paciente_id) REFERENCES paciente (id)
);
GO

CREATE TABLE agendamento_google_sync (
    id                      UNIQUEIDENTIFIER NOT NULL,
    empresa_id              UNIQUEIDENTIFIER NOT NULL,
    agendamento_id          UNIQUEIDENTIFIER NOT NULL,
    agenda_google_id        UNIQUEIDENTIFIER NOT NULL,
    google_event_id         NVARCHAR(300)    NULL,
    status_sync             NVARCHAR(40)     NOT NULL,
    ultima_sincronizacao    DATETIME2        NULL,
    erro_sync               NVARCHAR(2000)   NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_agendamento_google_sync PRIMARY KEY (id),
    CONSTRAINT fk_agendamento_google_sync_agenda_google_agenda_google_id FOREIGN KEY (agenda_google_id) REFERENCES agenda_google (id),
    CONSTRAINT fk_agendamento_google_sync_agendamento_agendamento_id FOREIGN KEY (agendamento_id) REFERENCES agendamento (id),
    CONSTRAINT fk_agendamento_google_sync_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id)
);
GO

CREATE TABLE aplicacao_sintoma (
    id                      UNIQUEIDENTIFIER NOT NULL,
    aplicacao_paciente_id   UNIQUEIDENTIFIER NOT NULL,
    sintoma_id              UNIQUEIDENTIFIER NOT NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_aplicacao_sintoma PRIMARY KEY (id),
    CONSTRAINT fk_aplicacao_sintoma_aplicacao_paciente_aplicacao_paciente_id FOREIGN KEY (aplicacao_paciente_id) REFERENCES aplicacao_paciente (id),
    CONSTRAINT fk_aplicacao_sintoma_sintoma_sintoma_id FOREIGN KEY (sintoma_id) REFERENCES sintoma (id)
);
GO

CREATE TABLE movimentacao_estoque (
    id                      UNIQUEIDENTIFIER NOT NULL,
    empresa_id              UNIQUEIDENTIFIER NOT NULL,
    unidade_id              UNIQUEIDENTIFIER NOT NULL,
    produto_id              UNIQUEIDENTIFIER NOT NULL,
    tipo                    NVARCHAR(40)     NOT NULL,
    quantidade              DECIMAL(18, 4)   NOT NULL,
    data                    DATETIME2        NOT NULL,
    origem                  NVARCHAR(120)    NOT NULL,
    funcionario_id          UNIQUEIDENTIFIER NULL,
    aplicacao_paciente_id   UNIQUEIDENTIFIER NULL,
    pedido_fornecedor_id    UNIQUEIDENTIFIER NULL,
    observacao              NVARCHAR(2000)   NULL,
    criado_em               DATETIME2        NOT NULL,
    atualizado_em           DATETIME2        NULL,
    CONSTRAINT pk_movimentacao_estoque PRIMARY KEY (id),
    CONSTRAINT fk_movimentacao_estoque_aplicacao_paciente_aplicacao_paciente_id FOREIGN KEY (aplicacao_paciente_id) REFERENCES aplicacao_paciente (id),
    CONSTRAINT fk_movimentacao_estoque_empresa_empresa_id FOREIGN KEY (empresa_id) REFERENCES empresa (id),
    CONSTRAINT fk_movimentacao_estoque_funcionario_funcionario_id FOREIGN KEY (funcionario_id) REFERENCES funcionario (id),
    CONSTRAINT fk_movimentacao_estoque_pedido_fornecedor_pedido_fornecedor_id FOREIGN KEY (pedido_fornecedor_id) REFERENCES pedido_fornecedor (id),
    CONSTRAINT fk_movimentacao_estoque_produto_produto_id FOREIGN KEY (produto_id) REFERENCES produto (id),
    CONSTRAINT fk_movimentacao_estoque_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id)
);
GO

CREATE INDEX ix_agenda_google_conta_google_conectada_id ON agenda_google (conta_google_conectada_id);
CREATE UNIQUE INDEX ix_agenda_google_empresa_id_conta_google_conectada_id_google_calendar_id ON agenda_google (empresa_id, conta_google_conectada_id, google_calendar_id);

CREATE INDEX ix_agendamento_cancelado_por_id ON agendamento (cancelado_por_id);
CREATE INDEX ix_agendamento_compra_paciente_id ON agendamento (compra_paciente_id);
CREATE INDEX ix_agendamento_criado_por_id ON agendamento (criado_por_id);
CREATE INDEX ix_agendamento_empresa_id_funcionario_id_data_inicio_data_fim ON agendamento (empresa_id, funcionario_id, data_inicio, data_fim);
CREATE INDEX ix_agendamento_empresa_id_paciente_id_status ON agendamento (empresa_id, paciente_id, status);
CREATE INDEX ix_agendamento_empresa_id_unidade_id_data_inicio ON agendamento (empresa_id, unidade_id, data_inicio);
CREATE INDEX ix_agendamento_funcionario_id ON agendamento (funcionario_id);
CREATE INDEX ix_agendamento_paciente_id ON agendamento (paciente_id);
CREATE INDEX ix_agendamento_unidade_id ON agendamento (unidade_id);

CREATE INDEX ix_agendamento_google_sync_agenda_google_id ON agendamento_google_sync (agenda_google_id);
CREATE UNIQUE INDEX ix_agendamento_google_sync_agendamento_id ON agendamento_google_sync (agendamento_id);
CREATE INDEX ix_agendamento_google_sync_empresa_id_status_sync ON agendamento_google_sync (empresa_id, status_sync);

CREATE UNIQUE INDEX ix_aplicacao_paciente_agendamento_id ON aplicacao_paciente (agendamento_id) WHERE agendamento_id IS NOT NULL;
CREATE INDEX ix_aplicacao_paciente_compra_paciente_id ON aplicacao_paciente (compra_paciente_id);
CREATE INDEX ix_aplicacao_paciente_empresa_id_paciente_id_data_aplicacao ON aplicacao_paciente (empresa_id, paciente_id, data_aplicacao);
CREATE INDEX ix_aplicacao_paciente_funcionario_id ON aplicacao_paciente (funcionario_id);
CREATE INDEX ix_aplicacao_paciente_paciente_id ON aplicacao_paciente (paciente_id);
CREATE INDEX ix_aplicacao_paciente_produto_id ON aplicacao_paciente (produto_id);
CREATE INDEX ix_aplicacao_paciente_unidade_id ON aplicacao_paciente (unidade_id);

CREATE UNIQUE INDEX ix_aplicacao_sintoma_aplicacao_paciente_id_sintoma_id ON aplicacao_sintoma (aplicacao_paciente_id, sintoma_id);
CREATE INDEX ix_aplicacao_sintoma_sintoma_id ON aplicacao_sintoma (sintoma_id);

CREATE INDEX ix_bloqueio_agenda_criado_por_id ON bloqueio_agenda (criado_por_id);
CREATE INDEX ix_bloqueio_agenda_empresa_id_funcionario_id_data_inicio_data_fim ON bloqueio_agenda (empresa_id, funcionario_id, data_inicio, data_fim);
CREATE INDEX ix_bloqueio_agenda_funcionario_id ON bloqueio_agenda (funcionario_id);
CREATE INDEX ix_bloqueio_agenda_unidade_id ON bloqueio_agenda (unidade_id);

CREATE UNIQUE INDEX ix_cargo_empresa_id_nome ON cargo (empresa_id, nome);

CREATE INDEX ix_compra_paciente_empresa_id_paciente_id_status ON compra_paciente (empresa_id, paciente_id, status);
CREATE INDEX ix_compra_paciente_paciente_id ON compra_paciente (paciente_id);
CREATE INDEX ix_compra_paciente_pacote_id ON compra_paciente (pacote_id);
CREATE INDEX ix_compra_paciente_unidade_id ON compra_paciente (unidade_id);

CREATE INDEX ix_conta_google_conectada_empresa_id_funcionario_id_ativo ON conta_google_conectada (empresa_id, funcionario_id, ativo);
CREATE INDEX ix_conta_google_conectada_empresa_id_google_account_id ON conta_google_conectada (empresa_id, google_account_id);
CREATE INDEX ix_conta_google_conectada_funcionario_id ON conta_google_conectada (funcionario_id);
CREATE INDEX ix_conta_google_conectada_usuario_id ON conta_google_conectada (usuario_id);

CREATE INDEX ix_conta_receber_compra_paciente_id ON conta_receber (compra_paciente_id);
CREATE INDEX ix_conta_receber_empresa_id_paciente_id_status ON conta_receber (empresa_id, paciente_id, status);
CREATE INDEX ix_conta_receber_paciente_id ON conta_receber (paciente_id);

CREATE INDEX ix_disponibilidade_funcionario_empresa_id_funcionario_id_unidade_id_dia_semana ON disponibilidade_funcionario (empresa_id, funcionario_id, unidade_id, dia_semana);
CREATE INDEX ix_disponibilidade_funcionario_funcionario_id ON disponibilidade_funcionario (funcionario_id);
CREATE INDEX ix_disponibilidade_funcionario_unidade_id ON disponibilidade_funcionario (unidade_id);

CREATE INDEX ix_empresa_cnpj ON empresa (cnpj);

CREATE UNIQUE INDEX ix_forma_pagamento_empresa_id_nome ON forma_pagamento (empresa_id, nome);

CREATE INDEX ix_fornecedor_empresa_id_cnpj ON fornecedor (empresa_id, cnpj);

CREATE INDEX ix_funcionario_cargo_id ON funcionario (cargo_id);
CREATE INDEX ix_funcionario_empresa_id_nome ON funcionario (empresa_id, nome);
CREATE INDEX ix_funcionario_unidade_id ON funcionario (unidade_id);

CREATE UNIQUE INDEX ix_item_pacote_pacote_id_produto_id ON item_pacote (pacote_id, produto_id);
CREATE INDEX ix_item_pacote_produto_id ON item_pacote (produto_id);

CREATE INDEX ix_item_pedido_fornecedor_pedido_fornecedor_id ON item_pedido_fornecedor (pedido_fornecedor_id);
CREATE INDEX ix_item_pedido_fornecedor_produto_id ON item_pedido_fornecedor (produto_id);

CREATE UNIQUE INDEX ix_licenca_modulo_empresa_id_modulo_id ON licenca_modulo (empresa_id, modulo_id);
CREATE INDEX ix_licenca_modulo_modulo_id ON licenca_modulo (modulo_id);

CREATE INDEX ix_log_auditoria_empresa_id_entidade_registro_id ON log_auditoria (empresa_id, entidade, registro_id);
CREATE INDEX ix_log_auditoria_empresa_id_usuario_id_data ON log_auditoria (empresa_id, usuario_id, data);
CREATE INDEX ix_log_auditoria_usuario_id ON log_auditoria (usuario_id);

CREATE UNIQUE INDEX ix_modulo_sistema_codigo ON modulo_sistema (codigo);

CREATE INDEX ix_movimentacao_estoque_aplicacao_paciente_id ON movimentacao_estoque (aplicacao_paciente_id);
CREATE INDEX ix_movimentacao_estoque_empresa_id_unidade_id_produto_id_data ON movimentacao_estoque (empresa_id, unidade_id, produto_id, data);
CREATE INDEX ix_movimentacao_estoque_funcionario_id ON movimentacao_estoque (funcionario_id);
CREATE INDEX ix_movimentacao_estoque_pedido_fornecedor_id ON movimentacao_estoque (pedido_fornecedor_id);
CREATE INDEX ix_movimentacao_estoque_produto_id ON movimentacao_estoque (produto_id);
CREATE INDEX ix_movimentacao_estoque_unidade_id ON movimentacao_estoque (unidade_id);

CREATE INDEX ix_paciente_empresa_id_cpf ON paciente (empresa_id, cpf);
CREATE INDEX ix_paciente_unidade_id ON paciente (unidade_id);

CREATE INDEX ix_pacote_empresa_id_nome ON pacote (empresa_id, nome);

CREATE INDEX ix_pagamento_paciente_conta_receber_id ON pagamento_paciente (conta_receber_id);
CREATE INDEX ix_pagamento_paciente_empresa_id_paciente_id_data_pagamento ON pagamento_paciente (empresa_id, paciente_id, data_pagamento);
CREATE INDEX ix_pagamento_paciente_forma_pagamento_id ON pagamento_paciente (forma_pagamento_id);
CREATE INDEX ix_pagamento_paciente_paciente_id ON pagamento_paciente (paciente_id);

CREATE INDEX ix_pedido_fornecedor_empresa_id_status ON pedido_fornecedor (empresa_id, status);
CREATE INDEX ix_pedido_fornecedor_fornecedor_id ON pedido_fornecedor (fornecedor_id);
CREATE INDEX ix_pedido_fornecedor_unidade_id ON pedido_fornecedor (unidade_id);

CREATE INDEX ix_permissao_usuario_modulo_id ON permissao_usuario (modulo_id);
CREATE UNIQUE INDEX ix_permissao_usuario_usuario_id_modulo_id ON permissao_usuario (usuario_id, modulo_id);

CREATE INDEX ix_produto_empresa_id_nome ON produto (empresa_id, nome);

CREATE UNIQUE INDEX ix_sintoma_empresa_id_nome ON sintoma (empresa_id, nome);

CREATE INDEX ix_unidade_empresa_id_nome ON unidade (empresa_id, nome);

CREATE UNIQUE INDEX ix_usuario_empresa_id_email_login ON usuario (empresa_id, email_login);
CREATE INDEX ix_usuario_funcionario_id ON usuario (funcionario_id);
GO
