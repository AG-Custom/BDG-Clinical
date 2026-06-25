using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnidadeMedidaAndRefactorProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(@"
INSERT INTO unidade_medida (id, empresa_id, nome, sigla, tipo, ativo, criado_em)
SELECT NEWID(), grouped.empresa_id, grouped.unidade_medida, grouped.unidade_medida, 'Outro', 1, GETUTCDATE()
FROM (
    SELECT DISTINCT empresa_id, unidade_medida
    FROM produto
) grouped
WHERE NOT EXISTS (
    SELECT 1
    FROM unidade_medida um
    WHERE um.empresa_id = grouped.empresa_id
      AND UPPER(um.sigla) = UPPER(grouped.unidade_medida)
);
");

            migrationBuilder.AddColumn<Guid>(
                name: "unidade_medida_id",
                table: "produto",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE p
SET unidade_medida_id = um.id
FROM produto p
INNER JOIN unidade_medida um
    ON um.empresa_id = p.empresa_id
   AND UPPER(um.sigla) = UPPER(p.unidade_medida);
");

            migrationBuilder.DropColumn(
                name: "unidade_medida",
                table: "produto");

            migrationBuilder.AlterColumn<Guid>(
                name: "unidade_medida_id",
                table: "produto",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_produto_unidade_medida_id",
                table: "produto",
                column: "unidade_medida_id");

            migrationBuilder.AddForeignKey(
                name: "fk_produto_unidade_medida_unidade_medida_id",
                table: "produto",
                column: "unidade_medida_id",
                principalTable: "unidade_medida",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_produto_unidade_medida_unidade_medida_id",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_unidade_medida_id",
                table: "produto");

            migrationBuilder.AddColumn<string>(
                name: "unidade_medida",
                table: "produto",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE p
SET unidade_medida = um.sigla
FROM produto p
INNER JOIN unidade_medida um ON um.id = p.unidade_medida_id;
");

            migrationBuilder.DropColumn(
                name: "unidade_medida_id",
                table: "produto");

            migrationBuilder.DropTable(
                name: "unidade_medida");
        }
    }
}
