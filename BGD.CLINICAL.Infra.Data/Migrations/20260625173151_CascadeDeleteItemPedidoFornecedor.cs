using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteItemPedidoFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id",
                table: "item_pedido_fornecedor");

            migrationBuilder.AddForeignKey(
                name: "fk_item_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id",
                table: "item_pedido_fornecedor",
                column: "pedido_fornecedor_id",
                principalTable: "pedido_fornecedor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id",
                table: "item_pedido_fornecedor");

            migrationBuilder.AddForeignKey(
                name: "fk_item_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id",
                table: "item_pedido_fornecedor",
                column: "pedido_fornecedor_id",
                principalTable: "pedido_fornecedor",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
