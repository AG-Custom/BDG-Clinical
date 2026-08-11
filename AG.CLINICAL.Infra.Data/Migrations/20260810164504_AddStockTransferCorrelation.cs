using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AG.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockTransferCorrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "transferencia_estoque_id",
                table: "movimentacao_estoque",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_empresa_id_transferencia_estoque_id",
                table: "movimentacao_estoque",
                columns: new[] { "empresa_id", "transferencia_estoque_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_movimentacao_estoque_empresa_id_transferencia_estoque_id",
                table: "movimentacao_estoque");

            migrationBuilder.DropColumn(
                name: "transferencia_estoque_id",
                table: "movimentacao_estoque");
        }
    }
}
