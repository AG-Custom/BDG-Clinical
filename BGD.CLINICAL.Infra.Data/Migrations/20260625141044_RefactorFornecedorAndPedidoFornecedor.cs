using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFornecedorAndPedidoFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_fornecedor_empresa_id_cnpj",
                table: "fornecedor");

            migrationBuilder.AlterColumn<string>(
                name: "cnpj",
                table: "fornecedor",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_fornecedor_empresa_id_cnpj",
                table: "fornecedor");

            migrationBuilder.DropIndex(
                name: "ix_fornecedor_empresa_id_nome",
                table: "fornecedor");

            migrationBuilder.AlterColumn<string>(
                name: "cnpj",
                table: "fornecedor",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_empresa_id_cnpj",
                table: "fornecedor",
                columns: new[] { "empresa_id", "cnpj" });
        }
    }
}
