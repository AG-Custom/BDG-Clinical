using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    cnpj = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    logo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    cor_principal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empresa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "modulo_sistema",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modulo_sistema", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cargo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargo", x => x.id);
                    table.ForeignKey(
                        name: "fk_cargo_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forma_pagamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forma_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "fk_forma_pagamento_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    cnpj = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_fornecedor_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pacote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    quantidade_aplicacoes = table.Column<int>(type: "int", nullable: false),
                    valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pacote", x => x.id);
                    table.ForeignKey(
                        name: "fk_pacote_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    unidade_medida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    estoque_minimo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_produto_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sintoma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sintoma", x => x.id);
                    table.ForeignKey(
                        name: "fk_sintoma_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    endereco = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unidade", x => x.id);
                    table.ForeignKey(
                        name: "fk_unidade_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "licenca_modulo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    modulo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_fim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_licenca_modulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_licenca_modulo_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_licenca_modulo_modulo_sistema_modulo_id",
                        column: x => x.modulo_id,
                        principalTable: "modulo_sistema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_pacote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pacote_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantidade_total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    unidade_medida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_pacote", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_pacote_pacote_pacote_id",
                        column: x => x.pacote_id,
                        principalTable: "pacote",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_item_pacote_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "funcionario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    cargo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    flag_aplicador = table.Column<bool>(type: "bit", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funcionario", x => x.id);
                    table.ForeignKey(
                        name: "fk_funcionario_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionario_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionario_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    cpf = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_paciente_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_paciente_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedido_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tipo_pedido = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    data_pedido = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    valor_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedido_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedido_fornecedor_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_fornecedor_fornecedor_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_fornecedor_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "disponibilidade_funcionario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dia_semana = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    hora_fim = table.Column<TimeOnly>(type: "time", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disponibilidade_funcionario", x => x.id);
                    table.ForeignKey(
                        name: "fk_disponibilidade_funcionario_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_disponibilidade_funcionario_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_disponibilidade_funcionario_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    nome = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    email_login = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    senha_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    auth_provider = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    google_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    tipo_usuario = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compra_paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pacote_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    data_compra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quantidade_aplicacoes = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compra_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_compra_paciente_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compra_paciente_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compra_paciente_pacote_pacote_id",
                        column: x => x.pacote_id,
                        principalTable: "pacote",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compra_paciente_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_pedido_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pedido_fornecedor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_pedido_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id",
                        column: x => x.pedido_fornecedor_id,
                        principalTable: "pedido_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_item_pedido_fornecedor_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bloqueio_agenda",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bloqueio_agenda", x => x.id);
                    table.ForeignKey(
                        name: "fk_bloqueio_agenda_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bloqueio_agenda_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bloqueio_agenda_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bloqueio_agenda_usuario_criado_por_id",
                        column: x => x.criado_por_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conta_google_conectada",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    google_email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    google_account_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    access_token = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    refresh_token = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    escopos_autorizados = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    conectado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conta_google_conectada", x => x.id);
                    table.ForeignKey(
                        name: "fk_conta_google_conectada_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_conta_google_conectada_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_conta_google_conectada_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "log_auditoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entidade = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    registro_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    acao = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    dados_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    dados_novos = table.Column<string>(type: "jsonb", nullable: true),
                    data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_log_auditoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_log_auditoria_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_log_auditoria_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "permissao_usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    modulo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pode_visualizar = table.Column<bool>(type: "bit", nullable: false),
                    pode_criar = table.Column<bool>(type: "bit", nullable: false),
                    pode_editar = table.Column<bool>(type: "bit", nullable: false),
                    pode_excluir = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissao_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_permissao_usuario_modulo_sistema_modulo_id",
                        column: x => x.modulo_id,
                        principalTable: "modulo_sistema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_permissao_usuario_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agendamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    compra_paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cancelado_por_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agendamento", x => x.id);
                    table.ForeignKey(
                        name: "fk_agendamento_compra_paciente_compra_paciente_id",
                        column: x => x.compra_paciente_id,
                        principalTable: "compra_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_usuario_cancelado_por_id",
                        column: x => x.cancelado_por_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_usuario_criado_por_id",
                        column: x => x.criado_por_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conta_receber",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    compra_paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    valor_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conta_receber", x => x.id);
                    table.ForeignKey(
                        name: "fk_conta_receber_compra_paciente_compra_paciente_id",
                        column: x => x.compra_paciente_id,
                        principalTable: "compra_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_conta_receber_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_conta_receber_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agenda_google",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    conta_google_conectada_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    google_calendar_id = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    principal = table.Column<bool>(type: "bit", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agenda_google", x => x.id);
                    table.ForeignKey(
                        name: "fk_agenda_google_conta_google_conectada_conta_google_conectada_id",
                        column: x => x.conta_google_conectada_id,
                        principalTable: "conta_google_conectada",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agenda_google_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aplicacao_paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    compra_paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    agendamento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    data_aplicacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quantidade_utilizada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    peso = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    realizado = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aplicacao_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_aplicacao_paciente_agendamento_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacao_paciente_compra_paciente_compra_paciente_id",
                        column: x => x.compra_paciente_id,
                        principalTable: "compra_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacao_paciente_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacao_paciente_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacao_paciente_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacao_paciente_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacao_paciente_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagamento_paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    conta_receber_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    forma_pagamento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    valor_pago = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamento_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_pagamento_paciente_conta_receber_conta_receber_id",
                        column: x => x.conta_receber_id,
                        principalTable: "conta_receber",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamento_paciente_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamento_paciente_forma_pagamento_forma_pagamento_id",
                        column: x => x.forma_pagamento_id,
                        principalTable: "forma_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamento_paciente_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agendamento_google_sync",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    agendamento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    agenda_google_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    google_event_id = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    status_sync = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ultima_sincronizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    erro_sync = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agendamento_google_sync", x => x.id);
                    table.ForeignKey(
                        name: "fk_agendamento_google_sync_agenda_google_agenda_google_id",
                        column: x => x.agenda_google_id,
                        principalTable: "agenda_google",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_google_sync_agendamento_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_agendamento_google_sync_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aplicacao_sintoma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    aplicacao_paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sintoma_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aplicacao_sintoma", x => x.id);
                    table.ForeignKey(
                        name: "fk_aplicacao_sintoma_aplicacao_paciente_aplicacao_paciente_id",
                        column: x => x.aplicacao_paciente_id,
                        principalTable: "aplicacao_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacao_sintoma_sintoma_sintoma_id",
                        column: x => x.sintoma_id,
                        principalTable: "sintoma",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacao_estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    quantidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    origem = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    aplicacao_paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    pedido_fornecedor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movimentacao_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_aplicacao_paciente_aplicacao_paciente_id",
                        column: x => x.aplicacao_paciente_id,
                        principalTable: "aplicacao_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_pedido_fornecedor_pedido_fornecedor_id",
                        column: x => x.pedido_fornecedor_id,
                        principalTable: "pedido_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agenda_google_conta_google_conectada_id",
                table: "agenda_google",
                column: "conta_google_conectada_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenda_google_empresa_id_conta_google_conectada_id_google_calendar_id",
                table: "agenda_google",
                columns: new[] { "empresa_id", "conta_google_conectada_id", "google_calendar_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_cancelado_por_id",
                table: "agendamento",
                column: "cancelado_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_compra_paciente_id",
                table: "agendamento",
                column: "compra_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_criado_por_id",
                table: "agendamento",
                column: "criado_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_empresa_id_funcionario_id_data_inicio_data_fim",
                table: "agendamento",
                columns: new[] { "empresa_id", "funcionario_id", "data_inicio", "data_fim" });

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_empresa_id_paciente_id_status",
                table: "agendamento",
                columns: new[] { "empresa_id", "paciente_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_empresa_id_unidade_id_data_inicio",
                table: "agendamento",
                columns: new[] { "empresa_id", "unidade_id", "data_inicio" });

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_funcionario_id",
                table: "agendamento",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_paciente_id",
                table: "agendamento",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_unidade_id",
                table: "agendamento",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_google_sync_agenda_google_id",
                table: "agendamento_google_sync",
                column: "agenda_google_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_google_sync_agendamento_id",
                table: "agendamento_google_sync",
                column: "agendamento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_google_sync_empresa_id_status_sync",
                table: "agendamento_google_sync",
                columns: new[] { "empresa_id", "status_sync" });

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_agendamento_id",
                table: "aplicacao_paciente",
                column: "agendamento_id",
                unique: true,
                filter: "[agendamento_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_compra_paciente_id",
                table: "aplicacao_paciente",
                column: "compra_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_empresa_id_paciente_id_data_aplicacao",
                table: "aplicacao_paciente",
                columns: new[] { "empresa_id", "paciente_id", "data_aplicacao" });

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_funcionario_id",
                table: "aplicacao_paciente",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_paciente_id",
                table: "aplicacao_paciente",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_produto_id",
                table: "aplicacao_paciente",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_unidade_id",
                table: "aplicacao_paciente",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_sintoma_aplicacao_paciente_id_sintoma_id",
                table: "aplicacao_sintoma",
                columns: new[] { "aplicacao_paciente_id", "sintoma_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_sintoma_sintoma_id",
                table: "aplicacao_sintoma",
                column: "sintoma_id");

            migrationBuilder.CreateIndex(
                name: "ix_bloqueio_agenda_criado_por_id",
                table: "bloqueio_agenda",
                column: "criado_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_bloqueio_agenda_empresa_id_funcionario_id_data_inicio_data_fim",
                table: "bloqueio_agenda",
                columns: new[] { "empresa_id", "funcionario_id", "data_inicio", "data_fim" });

            migrationBuilder.CreateIndex(
                name: "ix_bloqueio_agenda_funcionario_id",
                table: "bloqueio_agenda",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_bloqueio_agenda_unidade_id",
                table: "bloqueio_agenda",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_cargo_empresa_id_nome",
                table: "cargo",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compra_paciente_empresa_id_paciente_id_status",
                table: "compra_paciente",
                columns: new[] { "empresa_id", "paciente_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_compra_paciente_paciente_id",
                table: "compra_paciente",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_compra_paciente_pacote_id",
                table: "compra_paciente",
                column: "pacote_id");

            migrationBuilder.CreateIndex(
                name: "ix_compra_paciente_unidade_id",
                table: "compra_paciente",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_google_conectada_empresa_id_funcionario_id_ativo",
                table: "conta_google_conectada",
                columns: new[] { "empresa_id", "funcionario_id", "ativo" });

            migrationBuilder.CreateIndex(
                name: "ix_conta_google_conectada_empresa_id_google_account_id",
                table: "conta_google_conectada",
                columns: new[] { "empresa_id", "google_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_conta_google_conectada_funcionario_id",
                table: "conta_google_conectada",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_google_conectada_usuario_id",
                table: "conta_google_conectada",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_receber_compra_paciente_id",
                table: "conta_receber",
                column: "compra_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_conta_receber_empresa_id_paciente_id_status",
                table: "conta_receber",
                columns: new[] { "empresa_id", "paciente_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_conta_receber_paciente_id",
                table: "conta_receber",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_disponibilidade_funcionario_empresa_id_funcionario_id_unidade_id_dia_semana",
                table: "disponibilidade_funcionario",
                columns: new[] { "empresa_id", "funcionario_id", "unidade_id", "dia_semana" });

            migrationBuilder.CreateIndex(
                name: "ix_disponibilidade_funcionario_funcionario_id",
                table: "disponibilidade_funcionario",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_disponibilidade_funcionario_unidade_id",
                table: "disponibilidade_funcionario",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_cnpj",
                table: "empresa",
                column: "cnpj");

            migrationBuilder.CreateIndex(
                name: "ix_forma_pagamento_empresa_id_nome",
                table: "forma_pagamento",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_empresa_id_cnpj",
                table: "fornecedor",
                columns: new[] { "empresa_id", "cnpj" });

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_cargo_id",
                table: "funcionario",
                column: "cargo_id");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_empresa_id_nome",
                table: "funcionario",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_unidade_id",
                table: "funcionario",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pacote_pacote_id_produto_id",
                table: "item_pacote",
                columns: new[] { "pacote_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_pacote_produto_id",
                table: "item_pacote",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_fornecedor_pedido_fornecedor_id",
                table: "item_pedido_fornecedor",
                column: "pedido_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_fornecedor_produto_id",
                table: "item_pedido_fornecedor",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_licenca_modulo_empresa_id_modulo_id",
                table: "licenca_modulo",
                columns: new[] { "empresa_id", "modulo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_licenca_modulo_modulo_id",
                table: "licenca_modulo",
                column: "modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_log_auditoria_empresa_id_entidade_registro_id",
                table: "log_auditoria",
                columns: new[] { "empresa_id", "entidade", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "ix_log_auditoria_empresa_id_usuario_id_data",
                table: "log_auditoria",
                columns: new[] { "empresa_id", "usuario_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix_log_auditoria_usuario_id",
                table: "log_auditoria",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_modulo_sistema_codigo",
                table: "modulo_sistema",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_aplicacao_paciente_id",
                table: "movimentacao_estoque",
                column: "aplicacao_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_empresa_id_unidade_id_produto_id_data",
                table: "movimentacao_estoque",
                columns: new[] { "empresa_id", "unidade_id", "produto_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_funcionario_id",
                table: "movimentacao_estoque",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_pedido_fornecedor_id",
                table: "movimentacao_estoque",
                column: "pedido_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_produto_id",
                table: "movimentacao_estoque",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_unidade_id",
                table: "movimentacao_estoque",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_paciente_empresa_id_cpf",
                table: "paciente",
                columns: new[] { "empresa_id", "cpf" });

            migrationBuilder.CreateIndex(
                name: "ix_paciente_unidade_id",
                table: "paciente",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_pacote_empresa_id_nome",
                table: "pacote",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_paciente_conta_receber_id",
                table: "pagamento_paciente",
                column: "conta_receber_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_paciente_empresa_id_paciente_id_data_pagamento",
                table: "pagamento_paciente",
                columns: new[] { "empresa_id", "paciente_id", "data_pagamento" });

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_paciente_forma_pagamento_id",
                table: "pagamento_paciente",
                column: "forma_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_paciente_paciente_id",
                table: "pagamento_paciente",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_fornecedor_empresa_id_status",
                table: "pedido_fornecedor",
                columns: new[] { "empresa_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_pedido_fornecedor_fornecedor_id",
                table: "pedido_fornecedor",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_fornecedor_unidade_id",
                table: "pedido_fornecedor",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissao_usuario_modulo_id",
                table: "permissao_usuario",
                column: "modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissao_usuario_usuario_id_modulo_id",
                table: "permissao_usuario",
                columns: new[] { "usuario_id", "modulo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_nome",
                table: "produto",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_sintoma_empresa_id_nome",
                table: "sintoma",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unidade_empresa_id_nome",
                table: "unidade",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_empresa_id_email_login",
                table: "usuario",
                columns: new[] { "empresa_id", "email_login" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_funcionario_id",
                table: "usuario",
                column: "funcionario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agendamento_google_sync");

            migrationBuilder.DropTable(
                name: "aplicacao_sintoma");

            migrationBuilder.DropTable(
                name: "bloqueio_agenda");

            migrationBuilder.DropTable(
                name: "disponibilidade_funcionario");

            migrationBuilder.DropTable(
                name: "item_pacote");

            migrationBuilder.DropTable(
                name: "item_pedido_fornecedor");

            migrationBuilder.DropTable(
                name: "licenca_modulo");

            migrationBuilder.DropTable(
                name: "log_auditoria");

            migrationBuilder.DropTable(
                name: "movimentacao_estoque");

            migrationBuilder.DropTable(
                name: "pagamento_paciente");

            migrationBuilder.DropTable(
                name: "permissao_usuario");

            migrationBuilder.DropTable(
                name: "agenda_google");

            migrationBuilder.DropTable(
                name: "sintoma");

            migrationBuilder.DropTable(
                name: "aplicacao_paciente");

            migrationBuilder.DropTable(
                name: "pedido_fornecedor");

            migrationBuilder.DropTable(
                name: "conta_receber");

            migrationBuilder.DropTable(
                name: "forma_pagamento");

            migrationBuilder.DropTable(
                name: "modulo_sistema");

            migrationBuilder.DropTable(
                name: "conta_google_conectada");

            migrationBuilder.DropTable(
                name: "agendamento");

            migrationBuilder.DropTable(
                name: "produto");

            migrationBuilder.DropTable(
                name: "fornecedor");

            migrationBuilder.DropTable(
                name: "compra_paciente");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "paciente");

            migrationBuilder.DropTable(
                name: "pacote");

            migrationBuilder.DropTable(
                name: "funcionario");

            migrationBuilder.DropTable(
                name: "cargo");

            migrationBuilder.DropTable(
                name: "unidade");

            migrationBuilder.DropTable(
                name: "empresa");
        }
    }
}
