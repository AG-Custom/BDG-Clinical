using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTypeCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "tipo_produto",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_produto_empresa_id_codigo",
                table: "tipo_produto",
                columns: new[] { "empresa_id", "codigo" },
                unique: true,
                filter: "[codigo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tipo_produto_empresa_id_codigo",
                table: "tipo_produto");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "tipo_produto");
        }
    }
}
