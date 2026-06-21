using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialClinicalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cor_principal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empresas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "modulos_sistema",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modulos_sistema", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cargos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargos", x => x.id);
                    table.ForeignKey(
                        name: "fk_cargos_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "formas_pagamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formas_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "fk_formas_pagamento_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fornecedores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fornecedores", x => x.id);
                    table.ForeignKey(
                        name: "fk_fornecedores_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pacotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    quantidade_aplicacoes = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pacotes", x => x.id);
                    table.ForeignKey(
                        name: "fk_pacotes_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    unidade_medida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estoque_minimo = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos", x => x.id);
                    table.ForeignKey(
                        name: "fk_produtos_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sintomas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sintomas", x => x.id);
                    table.ForeignKey(
                        name: "fk_sintomas_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unidades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    endereco = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unidades", x => x.id);
                    table.ForeignKey(
                        name: "fk_unidades_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "licencas_modulo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_licencas_modulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_licencas_modulo_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_licencas_modulo_modulos_sistema_modulo_id",
                        column: x => x.modulo_id,
                        principalTable: "modulos_sistema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "funcionarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    flag_aplicador = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funcionarios", x => x.id);
                    table.ForeignKey(
                        name: "fk_funcionarios_cargos_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionarios_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "itens_pacote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pacote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    unidade_medida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_itens_pacote", x => x.id);
                    table.ForeignKey(
                        name: "fk_itens_pacote_pacotes_pacote_id",
                        column: x => x.pacote_id,
                        principalTable: "pacotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_itens_pacote_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pacientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    cpf = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pacientes", x => x.id);
                    table.ForeignKey(
                        name: "fk_pacientes_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pacientes_unidades_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedidos_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_pedido = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    data_pedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedidos_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedidos_fornecedor_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedidos_fornecedor_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedidos_fornecedor_unidades_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    senha = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo_usuario = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuarios_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuarios_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compras_paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pacote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_compra = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compras_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_compras_paciente_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compras_paciente_pacientes_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "pacientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compras_paciente_pacotes_pacote_id",
                        column: x => x.pacote_id,
                        principalTable: "pacotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compras_paciente_unidades_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "itens_pedido_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_itens_pedido_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_itens_pedido_fornecedor_pedidos_fornecedor_pedido_fornecedor~",
                        column: x => x.pedido_fornecedor_id,
                        principalTable: "pedidos_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_itens_pedido_fornecedor_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "permissoes_usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visualizar = table.Column<bool>(type: "boolean", nullable: false),
                    criar = table.Column<bool>(type: "boolean", nullable: false),
                    editar = table.Column<bool>(type: "boolean", nullable: false),
                    excluir = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissoes_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_permissoes_usuario_modulos_sistema_modulo_id",
                        column: x => x.modulo_id,
                        principalTable: "modulos_sistema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_permissoes_usuario_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aplicacoes_paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_aplicacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantidade_utilizada = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    peso = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    realizado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aplicacoes_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_aplicacoes_paciente_compras_paciente_compra_paciente_id",
                        column: x => x.compra_paciente_id,
                        principalTable: "compras_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacoes_paciente_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacoes_paciente_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacoes_paciente_pacientes_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "pacientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacoes_paciente_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacoes_paciente_unidades_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contas_receber",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compra_paciente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contas_receber", x => x.id);
                    table.ForeignKey(
                        name: "fk_contas_receber_compras_paciente_compra_paciente_id",
                        column: x => x.compra_paciente_id,
                        principalTable: "compras_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contas_receber_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contas_receber_pacientes_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "pacientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aplicacoes_sintomas",
                columns: table => new
                {
                    aplicacao_paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sintoma_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aplicacoes_sintomas", x => new { x.aplicacao_paciente_id, x.sintoma_id });
                    table.ForeignKey(
                        name: "fk_aplicacoes_sintomas_aplicacoes_paciente_aplicacao_paciente_~",
                        column: x => x.aplicacao_paciente_id,
                        principalTable: "aplicacoes_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacoes_sintomas_sintomas_sintoma_id",
                        column: x => x.sintoma_id,
                        principalTable: "sintomas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacoes_estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origem = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aplicacao_paciente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pedido_fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movimentacoes_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_movimentacoes_estoque_aplicacoes_paciente_aplicacao_pacient~",
                        column: x => x.aplicacao_paciente_id,
                        principalTable: "aplicacoes_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacoes_estoque_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacoes_estoque_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacoes_estoque_pedidos_fornecedor_pedido_fornecedor_id",
                        column: x => x.pedido_fornecedor_id,
                        principalTable: "pedidos_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacoes_estoque_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacoes_estoque_unidades_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos_paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_receber_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forma_pagamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamentos_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_pagamentos_paciente_contas_receber_conta_receber_id",
                        column: x => x.conta_receber_id,
                        principalTable: "contas_receber",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamentos_paciente_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamentos_paciente_formas_pagamento_forma_pagamento_id",
                        column: x => x.forma_pagamento_id,
                        principalTable: "formas_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamentos_paciente_pacientes_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "pacientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoes_paciente_compra_paciente_id",
                table: "aplicacoes_paciente",
                column: "compra_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoes_paciente_empresa_id_paciente_id_data_aplicacao",
                table: "aplicacoes_paciente",
                columns: new[] { "empresa_id", "paciente_id", "data_aplicacao" });

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoes_paciente_funcionario_id",
                table: "aplicacoes_paciente",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoes_paciente_paciente_id",
                table: "aplicacoes_paciente",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoes_paciente_produto_id",
                table: "aplicacoes_paciente",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoes_paciente_unidade_id",
                table: "aplicacoes_paciente",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoes_sintomas_sintoma_id",
                table: "aplicacoes_sintomas",
                column: "sintoma_id");

            migrationBuilder.CreateIndex(
                name: "ix_cargos_empresa_id_nome",
                table: "cargos",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compras_paciente_empresa_id_paciente_id_status",
                table: "compras_paciente",
                columns: new[] { "empresa_id", "paciente_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_compras_paciente_paciente_id",
                table: "compras_paciente",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_compras_paciente_pacote_id",
                table: "compras_paciente",
                column: "pacote_id");

            migrationBuilder.CreateIndex(
                name: "ix_compras_paciente_unidade_id",
                table: "compras_paciente",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_compra_paciente_id",
                table: "contas_receber",
                column: "compra_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_empresa_id_paciente_id_status",
                table: "contas_receber",
                columns: new[] { "empresa_id", "paciente_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_paciente_id",
                table: "contas_receber",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresas_cnpj",
                table: "empresas",
                column: "cnpj");

            migrationBuilder.CreateIndex(
                name: "ix_formas_pagamento_empresa_id_nome",
                table: "formas_pagamento",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedores_empresa_id_cnpj",
                table: "fornecedores",
                columns: new[] { "empresa_id", "cnpj" });

            migrationBuilder.CreateIndex(
                name: "ix_funcionarios_cargo_id",
                table: "funcionarios",
                column: "cargo_id");

            migrationBuilder.CreateIndex(
                name: "ix_funcionarios_empresa_id_nome",
                table: "funcionarios",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_itens_pacote_pacote_id_produto_id",
                table: "itens_pacote",
                columns: new[] { "pacote_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_itens_pacote_produto_id",
                table: "itens_pacote",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_itens_pedido_fornecedor_pedido_fornecedor_id",
                table: "itens_pedido_fornecedor",
                column: "pedido_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_itens_pedido_fornecedor_produto_id",
                table: "itens_pedido_fornecedor",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_licencas_modulo_empresa_id_modulo_id",
                table: "licencas_modulo",
                columns: new[] { "empresa_id", "modulo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_licencas_modulo_modulo_id",
                table: "licencas_modulo",
                column: "modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_modulos_sistema_codigo",
                table: "modulos_sistema",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_aplicacao_paciente_id",
                table: "movimentacoes_estoque",
                column: "aplicacao_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_empresa_id_unidade_id_produto_id_data",
                table: "movimentacoes_estoque",
                columns: new[] { "empresa_id", "unidade_id", "produto_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_funcionario_id",
                table: "movimentacoes_estoque",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_pedido_fornecedor_id",
                table: "movimentacoes_estoque",
                column: "pedido_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_produto_id",
                table: "movimentacoes_estoque",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_unidade_id",
                table: "movimentacoes_estoque",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_pacientes_empresa_id_cpf",
                table: "pacientes",
                columns: new[] { "empresa_id", "cpf" });

            migrationBuilder.CreateIndex(
                name: "ix_pacientes_unidade_id",
                table: "pacientes",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_pacotes_empresa_id_nome",
                table: "pacotes",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_paciente_conta_receber_id",
                table: "pagamentos_paciente",
                column: "conta_receber_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_paciente_empresa_id_paciente_id_data_pagamento",
                table: "pagamentos_paciente",
                columns: new[] { "empresa_id", "paciente_id", "data_pagamento" });

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_paciente_forma_pagamento_id",
                table: "pagamentos_paciente",
                column: "forma_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_paciente_paciente_id",
                table: "pagamentos_paciente",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_fornecedor_empresa_id_status",
                table: "pedidos_fornecedor",
                columns: new[] { "empresa_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_fornecedor_fornecedor_id",
                table: "pedidos_fornecedor",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_fornecedor_unidade_id",
                table: "pedidos_fornecedor",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissoes_usuario_modulo_id",
                table: "permissoes_usuario",
                column: "modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissoes_usuario_usuario_id_modulo_id",
                table: "permissoes_usuario",
                columns: new[] { "usuario_id", "modulo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produtos_empresa_id_nome",
                table: "produtos",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_sintomas_empresa_id_nome",
                table: "sintomas",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unidades_empresa_id_nome",
                table: "unidades",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_empresa_id_email",
                table: "usuarios",
                columns: new[] { "empresa_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_funcionario_id",
                table: "usuarios",
                column: "funcionario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aplicacoes_sintomas");

            migrationBuilder.DropTable(
                name: "itens_pacote");

            migrationBuilder.DropTable(
                name: "itens_pedido_fornecedor");

            migrationBuilder.DropTable(
                name: "licencas_modulo");

            migrationBuilder.DropTable(
                name: "movimentacoes_estoque");

            migrationBuilder.DropTable(
                name: "pagamentos_paciente");

            migrationBuilder.DropTable(
                name: "permissoes_usuario");

            migrationBuilder.DropTable(
                name: "sintomas");

            migrationBuilder.DropTable(
                name: "aplicacoes_paciente");

            migrationBuilder.DropTable(
                name: "pedidos_fornecedor");

            migrationBuilder.DropTable(
                name: "contas_receber");

            migrationBuilder.DropTable(
                name: "formas_pagamento");

            migrationBuilder.DropTable(
                name: "modulos_sistema");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "fornecedores");

            migrationBuilder.DropTable(
                name: "compras_paciente");

            migrationBuilder.DropTable(
                name: "funcionarios");

            migrationBuilder.DropTable(
                name: "pacientes");

            migrationBuilder.DropTable(
                name: "pacotes");

            migrationBuilder.DropTable(
                name: "cargos");

            migrationBuilder.DropTable(
                name: "unidades");

            migrationBuilder.DropTable(
                name: "empresas");
        }
    }
}
