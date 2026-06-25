using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoProdutoAndRefactorProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_produto_empresa_id_nome",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "tipo",
                table: "produto");

            migrationBuilder.AddColumn<Guid>(
                name: "tipo_produto_id",
                table: "produto",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_nome",
                table: "produto",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produto_tipo_produto_id",
                table: "produto",
                column: "tipo_produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_produto_empresa_id_nome",
                table: "tipo_produto",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_produto_tipo_produto_tipo_produto_id",
                table: "produto",
                column: "tipo_produto_id",
                principalTable: "tipo_produto",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_produto_tipo_produto_tipo_produto_id",
                table: "produto");

            migrationBuilder.DropTable(
                name: "tipo_produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_empresa_id_nome",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_tipo_produto_id",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "tipo_produto_id",
                table: "produto");

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                table: "produto",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_nome",
                table: "produto",
                columns: new[] { "empresa_id", "nome" });
        }
    }
}
