using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AG.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemCompraPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "item_compra_paciente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    compra_paciente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantidade_contratada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    quantidade_utilizada_base = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    unidade_medida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_compra_paciente", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_compra_paciente_compra_paciente_compra_paciente_id",
                        column: x => x.compra_paciente_id,
                        principalTable: "compra_paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_compra_paciente_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_compra_paciente_compra_paciente_id_produto_id",
                table: "item_compra_paciente",
                columns: new[] { "compra_paciente_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_compra_paciente_produto_id",
                table: "item_compra_paciente",
                column: "produto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_compra_paciente");
        }
    }
}
