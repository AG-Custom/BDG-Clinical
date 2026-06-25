using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorStatusPedidoFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE pedido_fornecedor SET status = N'Pendente' WHERE status = N'Aberto';
                UPDATE pedido_fornecedor SET status = N'Enviado para Fornecedor' WHERE status = N'Pedido';
                UPDATE pedido_fornecedor SET status = N'Recebido pela Unidade' WHERE status = N'Recebido';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE pedido_fornecedor SET status = N'Aberto' WHERE status = N'Pendente';
                UPDATE pedido_fornecedor SET status = N'Pedido' WHERE status = N'Enviado para Fornecedor';
                UPDATE pedido_fornecedor SET status = N'Recebido' WHERE status = N'Recebido pela Unidade';
                """);
        }
    }
}
