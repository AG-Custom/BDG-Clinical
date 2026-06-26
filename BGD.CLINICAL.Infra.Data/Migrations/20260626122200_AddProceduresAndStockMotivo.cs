using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProceduresAndStockMotivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_barras",
                table: "produto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_interno",
                table: "produto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "controla_estoque",
                table: "produto",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "sku",
                table: "produto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo",
                table: "movimentacao_estoque",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE movimentacao_estoque SET motivo = 'Compra' WHERE origem = 'PEDIDO_FORNECEDOR';
                UPDATE movimentacao_estoque SET motivo = 'Aplicacao' WHERE origem = 'APLICACAO_PACIENTE';
                UPDATE movimentacao_estoque SET motivo = 'Devolucao' WHERE origem = 'APLICACAO_PACIENTE_CANCELAMENTO';
                UPDATE movimentacao_estoque SET motivo = 'Ajuste' WHERE origem = 'AJUSTE_MANUAL';
                UPDATE movimentacao_estoque SET motivo = 'Perda' WHERE origem = 'PERDA_MANUAL';
                UPDATE movimentacao_estoque SET motivo = 'Ajuste' WHERE motivo = '';
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "quantidade_utilizada",
                table: "aplicacao_paciente",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<Guid>(
                name: "produto_id",
                table: "aplicacao_paciente",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "procedimento_id",
                table: "aplicacao_paciente",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "procedimento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    produto_aplicado_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    tempo_medio_minutos = table.Column<int>(type: "int", nullable: true),
                    preco_sugerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_procedimento", x => x.id);
                    table.ForeignKey(
                        name: "fk_procedimento_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_procedimento_produto_produto_aplicado_id",
                        column: x => x.produto_aplicado_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_procedimento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    procedimento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    produto_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_procedimento", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_procedimento_procedimento_procedimento_id",
                        column: x => x.procedimento_id,
                        principalTable: "procedimento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_procedimento_produto_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_codigo_interno",
                table: "produto",
                columns: new[] { "empresa_id", "codigo_interno" },
                unique: true,
                filter: "[codigo_interno] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_produto_empresa_id_sku",
                table: "produto",
                columns: new[] { "empresa_id", "sku" },
                unique: true,
                filter: "[sku] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_procedimento_id",
                table: "aplicacao_paciente",
                column: "procedimento_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_procedimento_procedimento_id_produto_id",
                table: "item_procedimento",
                columns: new[] { "procedimento_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_procedimento_produto_id",
                table: "item_procedimento",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_procedimento_empresa_id_nome",
                table: "procedimento",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_procedimento_produto_aplicado_id",
                table: "procedimento",
                column: "produto_aplicado_id");

            migrationBuilder.AddForeignKey(
                name: "fk_aplicacao_paciente_procedimento_procedimento_id",
                table: "aplicacao_paciente",
                column: "procedimento_id",
                principalTable: "procedimento",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_aplicacao_paciente_procedimento_procedimento_id",
                table: "aplicacao_paciente");

            migrationBuilder.DropTable(
                name: "item_procedimento");

            migrationBuilder.DropTable(
                name: "procedimento");

            migrationBuilder.DropIndex(
                name: "ix_produto_empresa_id_codigo_interno",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_empresa_id_sku",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_aplicacao_paciente_procedimento_id",
                table: "aplicacao_paciente");

            migrationBuilder.DropColumn(
                name: "codigo_barras",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "codigo_interno",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "controla_estoque",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "sku",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "motivo",
                table: "movimentacao_estoque");

            migrationBuilder.DropColumn(
                name: "procedimento_id",
                table: "aplicacao_paciente");

            migrationBuilder.AlterColumn<decimal>(
                name: "quantidade_utilizada",
                table: "aplicacao_paciente",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "produto_id",
                table: "aplicacao_paciente",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
