using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagesAndPatientPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "compra_paciente_id",
                table: "aplicacao_paciente",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "compra_paciente_id",
                table: "agendamento",
                type: "uniqueidentifier",
                nullable: true);

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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_pacote_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_compra_paciente_id",
                table: "aplicacao_paciente",
                column: "compra_paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_compra_paciente_id",
                table: "agendamento",
                column: "compra_paciente_id");

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
                name: "ix_item_pacote_pacote_id_produto_id",
                table: "item_pacote",
                columns: new[] { "pacote_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_pacote_produto_id",
                table: "item_pacote",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_pacote_empresa_id_nome",
                table: "pacote",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_agendamento_compra_paciente_compra_paciente_id",
                table: "agendamento",
                column: "compra_paciente_id",
                principalTable: "compra_paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_aplicacao_paciente_compra_paciente_compra_paciente_id",
                table: "aplicacao_paciente",
                column: "compra_paciente_id",
                principalTable: "compra_paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agendamento_compra_paciente_compra_paciente_id",
                table: "agendamento");

            migrationBuilder.DropForeignKey(
                name: "fk_aplicacao_paciente_compra_paciente_compra_paciente_id",
                table: "aplicacao_paciente");

            migrationBuilder.DropTable(
                name: "compra_paciente");

            migrationBuilder.DropTable(
                name: "item_pacote");

            migrationBuilder.DropTable(
                name: "pacote");

            migrationBuilder.DropIndex(
                name: "ix_aplicacao_paciente_compra_paciente_id",
                table: "aplicacao_paciente");

            migrationBuilder.DropIndex(
                name: "ix_agendamento_compra_paciente_id",
                table: "agendamento");

            migrationBuilder.DropColumn(
                name: "compra_paciente_id",
                table: "aplicacao_paciente");

            migrationBuilder.DropColumn(
                name: "compra_paciente_id",
                table: "agendamento");
        }
    }
}
