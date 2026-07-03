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
                name: "funcionario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funcionario", x => x.id);
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
                name: "output_message_email",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    destinatario_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    destinatario_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    payload_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    tentativas = table.Column<int>(type: "int", nullable: false),
                    erro = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    processado_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_output_message_email", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissao_sistema",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    chave = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    categoria = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    modulo_codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    chave_pai = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissao_sistema", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cargo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    flag_aplicador = table.Column<bool>(type: "bit", nullable: false),
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
                name: "fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    cnpj = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                name: "tipo_produto",
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
                    table.PrimaryKey("pk_tipo_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_tipo_produto_empresa_empresa_id",
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
                name: "unidade_medida",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    sigla = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unidade_medida", x => x.id);
                    table.ForeignKey(
                        name: "fk_unidade_medida_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
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
                    senha_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    auth_provider = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    google_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    tipo_usuario = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    pendente_primeiro_acesso = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    permission_version = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                name: "cargo_permissao_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cargo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    permission_key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargo_permissao_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_cargo_permissao_item_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "funcionario_vinculo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cargo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funcionario_vinculo", x => x.id);
                    table.CheckConstraint("ck_funcionario_vinculo_empresa_xor_unidade", "([empresa_id] IS NOT NULL AND [unidade_id] IS NULL) OR ([empresa_id] IS NULL AND [unidade_id] IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_funcionario_vinculo_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionario_vinculo_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionario_vinculo_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionario_vinculo_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "horario_funcionamento_unidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("pk_horario_funcionamento_unidade", x => x.id);
                    table.ForeignKey(
                        name: "fk_horario_funcionamento_unidade_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_horario_funcionamento_unidade_unidade_unidade_id",
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
                name: "produto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tipo_produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_medida_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    codigo_interno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    codigo_barras = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    estoque_minimo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    controla_estoque = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.ForeignKey(
                        name: "fk_produto_tipo_produto_tipo_produto_id",
                        column: x => x.tipo_produto_id,
                        principalTable: "tipo_produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_produto_unidade_medida_unidade_medida_id",
                        column: x => x.unidade_medida_id,
                        principalTable: "unidade_medida",
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
                name: "convite_primeiro_acesso",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    expira_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    utilizado_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_convite_primeiro_acesso", x => x.id);
                    table.ForeignKey(
                        name: "fk_convite_primeiro_acesso_usuario_usuario_id",
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
                    dados_anteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dados_novos = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                name: "usuario_permissao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    permission_key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    effect = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_permissao", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_permissao_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "anexo_pedido_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pedido_fornecedor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome_arquivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    object_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anexo_pedido_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_anexo_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id",
                        column: x => x.pedido_fornecedor_id,
                        principalTable: "pedido_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_pedido_fornecedor_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "procedimento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    produto_aplicado_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_procedimento", x => x.id);
                    table.ForeignKey(
                        name: "fk_procedimento_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_procedimento_produto_produto_aplicado_id",
                        column: x => x.produto_aplicado_id,
                        principalTable: "produto",
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
                    procedimento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    excecao_horario = table.Column<bool>(type: "bit", nullable: false),
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
                        name: "fk_agendamento_procedimento_procedimento_id",
                        column: x => x.procedimento_id,
                        principalTable: "procedimento",
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
                name: "item_procedimento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    procedimento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_procedimento", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_procedimento_procedimento_procedimento_id",
                        column: x => x.procedimento_id,
                        principalTable: "procedimento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_procedimento_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
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
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    procedimento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    agendamento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    data_aplicacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quantidade_utilizada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    peso = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    realizado = table.Column<bool>(type: "bit", nullable: false),
                    cancelada = table.Column<bool>(type: "bit", nullable: false),
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
                        name: "fk_aplicacao_paciente_procedimento_procedimento_id",
                        column: x => x.procedimento_id,
                        principalTable: "procedimento",
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
                    motivo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
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
                name: "ix_agendamento_cancelado_por_id",
                table: "agendamento",
                column: "cancelado_por_id");

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
                name: "ix_agendamento_procedimento_id",
                table: "agendamento",
                column: "procedimento_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_unidade_id",
                table: "agendamento",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_anexo_pedido_fornecedor_pedido_fornecedor_id_empresa_id",
                table: "anexo_pedido_fornecedor",
                columns: new[] { "pedido_fornecedor_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_agendamento_id",
                table: "aplicacao_paciente",
                column: "agendamento_id",
                unique: true,
                filter: "[agendamento_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_empresa_id_cancelada",
                table: "aplicacao_paciente",
                columns: new[] { "empresa_id", "cancelada" });

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
                name: "ix_aplicacao_paciente_procedimento_id",
                table: "aplicacao_paciente",
                column: "procedimento_id");

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
                name: "ix_cargo_permissao_item_cargo_id_permission_key",
                table: "cargo_permissao_item",
                columns: new[] { "cargo_id", "permission_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_convite_primeiro_acesso_token_hash",
                table: "convite_primeiro_acesso",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_convite_primeiro_acesso_usuario_id_utilizado_em",
                table: "convite_primeiro_acesso",
                columns: new[] { "usuario_id", "utilizado_em" });

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
                name: "ix_fornecedor_empresa_id_cnpj",
                table: "fornecedor",
                columns: new[] { "empresa_id", "cnpj" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_empresa_id_nome",
                table: "fornecedor",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_vinculo_cargo_id",
                table: "funcionario_vinculo",
                column: "cargo_id");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_vinculo_empresa_id",
                table: "funcionario_vinculo",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_vinculo_funcionario_id_empresa_id",
                table: "funcionario_vinculo",
                columns: new[] { "funcionario_id", "empresa_id" },
                unique: true,
                filter: "[empresa_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_vinculo_funcionario_id_unidade_id",
                table: "funcionario_vinculo",
                columns: new[] { "funcionario_id", "unidade_id" },
                unique: true,
                filter: "[unidade_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_vinculo_unidade_id",
                table: "funcionario_vinculo",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_horario_funcionamento_unidade_empresa_id_unidade_id_dia_semana",
                table: "horario_funcionamento_unidade",
                columns: new[] { "empresa_id", "unidade_id", "dia_semana" });

            migrationBuilder.CreateIndex(
                name: "ix_horario_funcionamento_unidade_unidade_id",
                table: "horario_funcionamento_unidade",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_fornecedor_pedido_fornecedor_id",
                table: "item_pedido_fornecedor",
                column: "pedido_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_fornecedor_produto_id",
                table: "item_pedido_fornecedor",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_procedimento_procedimento_id_produto_id",
                table: "item_procedimento",
                columns: new[] { "procedimento_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_procedimento_produto_id",
                table: "item_procedimento",
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
                name: "ix_output_message_email_empresa_id",
                table: "output_message_email",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_output_message_email_status_criado_em",
                table: "output_message_email",
                columns: new[] { "status", "criado_em" });

            migrationBuilder.CreateIndex(
                name: "ix_paciente_empresa_id_cpf",
                table: "paciente",
                columns: new[] { "empresa_id", "cpf" },
                unique: true,
                filter: "[cpf] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_paciente_unidade_id",
                table: "paciente",
                column: "unidade_id");

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
                name: "ix_permissao_sistema_chave",
                table: "permissao_sistema",
                column: "chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_procedimento_empresa_id_nome",
                table: "procedimento",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_procedimento_produto_aplicado_id",
                table: "procedimento",
                column: "produto_aplicado_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_codigo_interno",
                table: "produto",
                columns: new[] { "empresa_id", "codigo_interno" },
                unique: true,
                filter: "[codigo_interno] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_nome",
                table: "produto",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_sku",
                table: "produto",
                columns: new[] { "empresa_id", "sku" },
                unique: true,
                filter: "[sku] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_produto_tipo_produto_id",
                table: "produto",
                column: "tipo_produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_unidade_medida_id",
                table: "produto",
                column: "unidade_medida_id");

            migrationBuilder.CreateIndex(
                name: "ix_sintoma_empresa_id_nome",
                table: "sintoma",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_produto_empresa_id_nome",
                table: "tipo_produto",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unidade_empresa_id_nome",
                table: "unidade",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_unidade_medida_empresa_id_nome",
                table: "unidade_medida",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unidade_medida_empresa_id_sigla",
                table: "unidade_medida",
                columns: new[] { "empresa_id", "sigla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_empresa_id_email_login",
                table: "usuario",
                columns: new[] { "empresa_id", "email_login" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_funcionario_id",
                table: "usuario",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_permissao_usuario_id_permission_key",
                table: "usuario_permissao",
                columns: new[] { "usuario_id", "permission_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anexo_pedido_fornecedor");

            migrationBuilder.DropTable(
                name: "aplicacao_sintoma");

            migrationBuilder.DropTable(
                name: "bloqueio_agenda");

            migrationBuilder.DropTable(
                name: "cargo_permissao_item");

            migrationBuilder.DropTable(
                name: "convite_primeiro_acesso");

            migrationBuilder.DropTable(
                name: "disponibilidade_funcionario");

            migrationBuilder.DropTable(
                name: "funcionario_vinculo");

            migrationBuilder.DropTable(
                name: "horario_funcionamento_unidade");

            migrationBuilder.DropTable(
                name: "item_pedido_fornecedor");

            migrationBuilder.DropTable(
                name: "item_procedimento");

            migrationBuilder.DropTable(
                name: "licenca_modulo");

            migrationBuilder.DropTable(
                name: "log_auditoria");

            migrationBuilder.DropTable(
                name: "movimentacao_estoque");

            migrationBuilder.DropTable(
                name: "output_message_email");

            migrationBuilder.DropTable(
                name: "permissao_sistema");

            migrationBuilder.DropTable(
                name: "usuario_permissao");

            migrationBuilder.DropTable(
                name: "sintoma");

            migrationBuilder.DropTable(
                name: "cargo");

            migrationBuilder.DropTable(
                name: "modulo_sistema");

            migrationBuilder.DropTable(
                name: "aplicacao_paciente");

            migrationBuilder.DropTable(
                name: "pedido_fornecedor");

            migrationBuilder.DropTable(
                name: "agendamento");

            migrationBuilder.DropTable(
                name: "fornecedor");

            migrationBuilder.DropTable(
                name: "paciente");

            migrationBuilder.DropTable(
                name: "procedimento");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "unidade");

            migrationBuilder.DropTable(
                name: "produto");

            migrationBuilder.DropTable(
                name: "funcionario");

            migrationBuilder.DropTable(
                name: "tipo_produto");

            migrationBuilder.DropTable(
                name: "unidade_medida");

            migrationBuilder.DropTable(
                name: "empresa");
        }
    }
}
