using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierObservacaoAndOrderAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "observacao",
                table: "fornecedor",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "anexo_pedido_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pedido_fornecedor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome_arquivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    object_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anexo_pedido_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_anexo_pedido_fornecedor_pedido_fornecedor_pedido_fornecedor_id",
                        column: x => x.pedido_fornecedor_id,
                        principalTable: "pedido_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_anexo_pedido_fornecedor_pedido_fornecedor_id_empresa_id",
                table: "anexo_pedido_fornecedor",
                columns: new[] { "pedido_fornecedor_id", "empresa_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anexo_pedido_fornecedor");

            migrationBuilder.DropColumn(
                name: "observacao",
                table: "fornecedor");
        }
    }
}
