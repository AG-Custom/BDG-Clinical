using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AG.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManualStockMovementUnitValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "valor_unitario",
                table: "movimentacao_estoque",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "valor_unitario",
                table: "movimentacao_estoque");
        }
    }
}
