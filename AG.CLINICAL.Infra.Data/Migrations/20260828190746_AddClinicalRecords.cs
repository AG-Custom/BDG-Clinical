using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AG.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sexo",
                table: "paciente",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "modelo_anamnese",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    especialidade = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    schema_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modelo_anamnese", x => x.id);
                    table.ForeignKey(
                        name: "fk_modelo_anamnese_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prontuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    alergias = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    alertas = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    observacao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prontuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_prontuario_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_prontuario_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "atendimento_clinico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prontuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    agendamento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_fim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_atendimento_clinico", x => x.id);
                    table.ForeignKey(
                        name: "fk_atendimento_clinico_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_atendimento_clinico_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_atendimento_clinico_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_atendimento_clinico_prontuario_prontuario_id",
                        column: x => x.prontuario_id,
                        principalTable: "prontuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_atendimento_clinico_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "anexo_clinico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    data_documento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    categoria_exame = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    categoria_documento = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    nome_arquivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    object_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anexo_clinico", x => x.id);
                    table.ForeignKey(
                        name: "fk_anexo_clinico_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anexo_clinico_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anexo_clinico_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anexo_clinico_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anexo_clinico_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "anotacao_clinica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    texto = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anotacao_clinica", x => x.id);
                    table.ForeignKey(
                        name: "fk_anotacao_clinica_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anotacao_clinica_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anotacao_clinica_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anotacao_clinica_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_anotacao_clinica_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "avaliacao_corporal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    data_avaliacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    altura_cm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    peso_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    imc = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    classificacao_imc = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    peso_ideal_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    cintura_cm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    quadril_cm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    relacao_cintura_quadril = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    percentual_massa_gorda = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    massa_gorda_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    percentual_massa_magra = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    massa_magra_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    percentual_agua = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    protocolo_prega = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    bioimpedancia_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    circunferencias_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pregas_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    observacao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avaliacao_corporal", x => x.id);
                    table.ForeignKey(
                        name: "fk_avaliacao_corporal_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_avaliacao_corporal_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_avaliacao_corporal_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_avaliacao_corporal_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_avaliacao_corporal_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "calculo_energetico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    perfil = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    protocolo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    nivel_atividade = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    fator_injuria = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    peso_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    altura_cm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    idade = table.Column<int>(type: "int", nullable: false),
                    massa_magra_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    peso_desejado_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    tempo_dias = table.Column<int>(type: "int", nullable: false),
                    atividades_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gasto_energetico_basal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    gasto_energetico_total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ajuste_calorico_diario = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    meta_calorica_diaria = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calculo_energetico", x => x.id);
                    table.ForeignKey(
                        name: "fk_calculo_energetico_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_calculo_energetico_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_calculo_energetico_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_calculo_energetico_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_calculo_energetico_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evento_clinico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    resumo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    entidade = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    registro_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dados_anteriores = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    dados_novos = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evento_clinico", x => x.id);
                    table.ForeignKey(
                        name: "fk_evento_clinico_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evento_clinico_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evento_clinico_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evento_clinico_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evento_clinico_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "registro_anamnese",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    modelo_anamnese_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    schema_snapshot_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    respostas_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    versao_atual = table.Column<int>(type: "int", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registro_anamnese", x => x.id);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_modelo_anamnese_modelo_anamnese_id",
                        column: x => x.modelo_anamnese_id,
                        principalTable: "modelo_anamnese",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "regra_bolso",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    objetivo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    peso_kg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    gasto_energetico_total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    calorias = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    proteinas_g = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    carboidratos_g = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    gorduras_g = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regra_bolso", x => x.id);
                    table.ForeignKey(
                        name: "fk_regra_bolso_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_regra_bolso_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_regra_bolso_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_regra_bolso_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_regra_bolso_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "foto_comparativa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atendimento_clinico_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    avaliacao_corporal_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    categoria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    data_captura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    nome_arquivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    object_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_foto_comparativa", x => x.id);
                    table.ForeignKey(
                        name: "fk_foto_comparativa_atendimento_clinico_atendimento_clinico_id",
                        column: x => x.atendimento_clinico_id,
                        principalTable: "atendimento_clinico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_foto_comparativa_avaliacao_corporal_avaliacao_corporal_id",
                        column: x => x.avaliacao_corporal_id,
                        principalTable: "avaliacao_corporal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_foto_comparativa_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_foto_comparativa_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_foto_comparativa_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_foto_comparativa_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "registro_anamnese_versao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    registro_anamnese_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    versao = table.Column<int>(type: "int", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    respostas_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    resumo_alteracao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    respostas_anteriores_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registro_anamnese_versao", x => x.id);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_versao_funcionario_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_anamnese_versao_registro_anamnese_registro_anamnese_id",
                        column: x => x.registro_anamnese_id,
                        principalTable: "registro_anamnese",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_anexo_clinico_atendimento_clinico_id",
                table: "anexo_clinico",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_anexo_clinico_empresa_id",
                table: "anexo_clinico",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_anexo_clinico_funcionario_id",
                table: "anexo_clinico",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_anexo_clinico_paciente_id",
                table: "anexo_clinico",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_anexo_clinico_unidade_id",
                table: "anexo_clinico",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_anotacao_clinica_atendimento_clinico_id",
                table: "anotacao_clinica",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_anotacao_clinica_empresa_id",
                table: "anotacao_clinica",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_anotacao_clinica_funcionario_id",
                table: "anotacao_clinica",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_anotacao_clinica_paciente_id",
                table: "anotacao_clinica",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_anotacao_clinica_unidade_id",
                table: "anotacao_clinica",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_atendimento_clinico_empresa_id_paciente_id_data_inicio",
                table: "atendimento_clinico",
                columns: new[] { "empresa_id", "paciente_id", "data_inicio" });

            migrationBuilder.CreateIndex(
                name: "ix_atendimento_clinico_funcionario_id",
                table: "atendimento_clinico",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_atendimento_clinico_paciente_id",
                table: "atendimento_clinico",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_atendimento_clinico_prontuario_id",
                table: "atendimento_clinico",
                column: "prontuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_atendimento_clinico_unidade_id",
                table: "atendimento_clinico",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_avaliacao_corporal_atendimento_clinico_id",
                table: "avaliacao_corporal",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_avaliacao_corporal_empresa_id_paciente_id_data_avaliacao",
                table: "avaliacao_corporal",
                columns: new[] { "empresa_id", "paciente_id", "data_avaliacao" });

            migrationBuilder.CreateIndex(
                name: "ix_avaliacao_corporal_funcionario_id",
                table: "avaliacao_corporal",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_avaliacao_corporal_paciente_id",
                table: "avaliacao_corporal",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_avaliacao_corporal_unidade_id",
                table: "avaliacao_corporal",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculo_energetico_atendimento_clinico_id",
                table: "calculo_energetico",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculo_energetico_empresa_id",
                table: "calculo_energetico",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculo_energetico_funcionario_id",
                table: "calculo_energetico",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculo_energetico_paciente_id",
                table: "calculo_energetico",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculo_energetico_unidade_id",
                table: "calculo_energetico",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_clinico_atendimento_clinico_id",
                table: "evento_clinico",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_clinico_empresa_id_paciente_id_data",
                table: "evento_clinico",
                columns: new[] { "empresa_id", "paciente_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix_evento_clinico_funcionario_id",
                table: "evento_clinico",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_clinico_paciente_id",
                table: "evento_clinico",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_clinico_unidade_id",
                table: "evento_clinico",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_foto_comparativa_atendimento_clinico_id",
                table: "foto_comparativa",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_foto_comparativa_avaliacao_corporal_id",
                table: "foto_comparativa",
                column: "avaliacao_corporal_id");

            migrationBuilder.CreateIndex(
                name: "ix_foto_comparativa_empresa_id",
                table: "foto_comparativa",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_foto_comparativa_funcionario_id",
                table: "foto_comparativa",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_foto_comparativa_paciente_id",
                table: "foto_comparativa",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_foto_comparativa_unidade_id",
                table: "foto_comparativa",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_modelo_anamnese_empresa_id_nome",
                table: "modelo_anamnese",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prontuario_empresa_id_paciente_id",
                table: "prontuario",
                columns: new[] { "empresa_id", "paciente_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prontuario_paciente_id",
                table: "prontuario",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_atendimento_clinico_id",
                table: "registro_anamnese",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_empresa_id",
                table: "registro_anamnese",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_funcionario_id",
                table: "registro_anamnese",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_modelo_anamnese_id",
                table: "registro_anamnese",
                column: "modelo_anamnese_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_paciente_id",
                table: "registro_anamnese",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_unidade_id",
                table: "registro_anamnese",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_versao_funcionario_id",
                table: "registro_anamnese_versao",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_anamnese_versao_registro_anamnese_id",
                table: "registro_anamnese_versao",
                column: "registro_anamnese_id");

            migrationBuilder.CreateIndex(
                name: "ix_regra_bolso_atendimento_clinico_id",
                table: "regra_bolso",
                column: "atendimento_clinico_id");

            migrationBuilder.CreateIndex(
                name: "ix_regra_bolso_empresa_id",
                table: "regra_bolso",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_regra_bolso_funcionario_id",
                table: "regra_bolso",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_regra_bolso_paciente_id",
                table: "regra_bolso",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_regra_bolso_unidade_id",
                table: "regra_bolso",
                column: "unidade_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anexo_clinico");

            migrationBuilder.DropTable(
                name: "anotacao_clinica");

            migrationBuilder.DropTable(
                name: "calculo_energetico");

            migrationBuilder.DropTable(
                name: "evento_clinico");

            migrationBuilder.DropTable(
                name: "foto_comparativa");

            migrationBuilder.DropTable(
                name: "registro_anamnese_versao");

            migrationBuilder.DropTable(
                name: "regra_bolso");

            migrationBuilder.DropTable(
                name: "avaliacao_corporal");

            migrationBuilder.DropTable(
                name: "registro_anamnese");

            migrationBuilder.DropTable(
                name: "atendimento_clinico");

            migrationBuilder.DropTable(
                name: "modelo_anamnese");

            migrationBuilder.DropTable(
                name: "prontuario");

            migrationBuilder.DropColumn(
                name: "sexo",
                table: "paciente");
        }
    }
}
