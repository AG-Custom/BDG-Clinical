using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationLotsAndConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "concentracao_por_conteudo",
                table: "produto",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "conteudo_por_embalagem",
                table: "produto",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "unidade_conteudo_id",
                table: "produto",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "unidade_embalagem_id",
                table: "produto",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "lote_produto_id",
                table: "movimentacao_estoque",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantidade_embalagem",
                table: "movimentacao_estoque",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "lote_produto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    data_validade = table.Column<DateOnly>(type: "date", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lote_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_lote_produto_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lote_produto_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lote_produto_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produto_unidade_conteudo_id",
                table: "produto",
                column: "unidade_conteudo_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_unidade_embalagem_id",
                table: "produto",
                column: "unidade_embalagem_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_empresa_id_lote_produto_id",
                table: "movimentacao_estoque",
                columns: new[] { "empresa_id", "lote_produto_id" });

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_lote_produto_id",
                table: "movimentacao_estoque",
                column: "lote_produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_lote_produto_empresa_id_unidade_id_produto_id_codigo",
                table: "lote_produto",
                columns: new[] { "empresa_id", "unidade_id", "produto_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lote_produto_produto_id",
                table: "lote_produto",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_lote_produto_unidade_id",
                table: "lote_produto",
                column: "unidade_id");

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacao_estoque_lote_produto_lote_produto_id",
                table: "movimentacao_estoque",
                column: "lote_produto_id",
                principalTable: "lote_produto",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_produto_unidade_medida_unidade_conteudo_id",
                table: "produto",
                column: "unidade_conteudo_id",
                principalTable: "unidade_medida",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_produto_unidade_medida_unidade_embalagem_id",
                table: "produto",
                column: "unidade_embalagem_id",
                principalTable: "unidade_medida",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_movimentacao_estoque_lote_produto_lote_produto_id",
                table: "movimentacao_estoque");

            migrationBuilder.DropForeignKey(
                name: "fk_produto_unidade_medida_unidade_conteudo_id",
                table: "produto");

            migrationBuilder.DropForeignKey(
                name: "fk_produto_unidade_medida_unidade_embalagem_id",
                table: "produto");

            migrationBuilder.DropTable(
                name: "lote_produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_unidade_conteudo_id",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_unidade_embalagem_id",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_movimentacao_estoque_empresa_id_lote_produto_id",
                table: "movimentacao_estoque");

            migrationBuilder.DropIndex(
                name: "ix_movimentacao_estoque_lote_produto_id",
                table: "movimentacao_estoque");

            migrationBuilder.DropColumn(
                name: "concentracao_por_conteudo",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "conteudo_por_embalagem",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "unidade_conteudo_id",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "unidade_embalagem_id",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "lote_produto_id",
                table: "movimentacao_estoque");

            migrationBuilder.DropColumn(
                name: "quantidade_embalagem",
                table: "movimentacao_estoque");
        }
    }
}
