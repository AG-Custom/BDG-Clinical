using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitPermissionCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE permissao_sistema SET categoria = N'Sintomas', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.visualizar';
                UPDATE permissao_sistema SET categoria = N'Sintomas', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.criar';
                UPDATE permissao_sistema SET categoria = N'Sintomas', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.editar';
                UPDATE permissao_sistema SET categoria = N'Sintomas', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.excluir';

                UPDATE permissao_sistema SET categoria = N'Produto', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.visualizar';
                UPDATE permissao_sistema SET categoria = N'Produto', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.criar';
                UPDATE permissao_sistema SET categoria = N'Produto', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.editar';
                UPDATE permissao_sistema SET categoria = N'Produto', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.excluir';

                UPDATE permissao_sistema SET categoria = N'Tipo de produto', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.visualizar';
                UPDATE permissao_sistema SET categoria = N'Tipo de produto', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.criar';
                UPDATE permissao_sistema SET categoria = N'Tipo de produto', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.editar';
                UPDATE permissao_sistema SET categoria = N'Tipo de produto', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.excluir';

                UPDATE permissao_sistema SET categoria = N'Unidade de medida', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.visualizar';
                UPDATE permissao_sistema SET categoria = N'Unidade de medida', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.criar';
                UPDATE permissao_sistema SET categoria = N'Unidade de medida', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.editar';
                UPDATE permissao_sistema SET categoria = N'Unidade de medida', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.excluir';

                UPDATE permissao_sistema SET categoria = N'Fornecedor', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.visualizar';
                UPDATE permissao_sistema SET categoria = N'Fornecedor', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.criar';
                UPDATE permissao_sistema SET categoria = N'Fornecedor', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.editar';
                UPDATE permissao_sistema SET categoria = N'Fornecedor', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.excluir';

                UPDATE permissao_sistema SET categoria = N'Pedido', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.visualizar';
                UPDATE permissao_sistema SET categoria = N'Pedido', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.criar';
                UPDATE permissao_sistema SET categoria = N'Pedido', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.editar';
                UPDATE permissao_sistema SET categoria = N'Pedido', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.cancelar';
                UPDATE permissao_sistema SET categoria = N'Pedido', ordem = 14, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.aprovar';

                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.visualizar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.movimentar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.ajustar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.inventario';

                UPDATE permissao_sistema SET categoria = N'Procedimentos', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'procedimento.visualizar';
                UPDATE permissao_sistema SET categoria = N'Procedimentos', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'procedimento.criar';
                UPDATE permissao_sistema SET categoria = N'Procedimentos', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'procedimento.editar';

                UPDATE permissao_sistema SET categoria = N'Funcionários', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'funcionario.visualizar';
                UPDATE permissao_sistema SET categoria = N'Funcionários', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'funcionario.criar';
                UPDATE permissao_sistema SET categoria = N'Funcionários', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'funcionario.editar';

                UPDATE permissao_sistema SET categoria = N'Unidades', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade.visualizar';
                UPDATE permissao_sistema SET categoria = N'Unidades', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade.criar';
                UPDATE permissao_sistema SET categoria = N'Unidades', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade.editar';

                UPDATE permissao_sistema SET categoria = N'Empresa', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'empresa.editar';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE permissao_sistema SET categoria = N'Pacientes', ordem = 20, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.visualizar';
                UPDATE permissao_sistema SET categoria = N'Pacientes', ordem = 21, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.criar';
                UPDATE permissao_sistema SET categoria = N'Pacientes', ordem = 22, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.editar';
                UPDATE permissao_sistema SET categoria = N'Pacientes', ordem = 23, atualizado_em = SYSUTCDATETIME() WHERE chave = N'sintoma.excluir';

                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.visualizar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.criar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.editar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 13, atualizado_em = SYSUTCDATETIME() WHERE chave = N'produto.excluir';

                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 14, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.visualizar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 15, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.criar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 16, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.editar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 17, atualizado_em = SYSUTCDATETIME() WHERE chave = N'tipo_produto.excluir';

                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 18, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.visualizar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 19, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.criar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 20, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.editar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 21, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade_medida.excluir';

                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 20, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.visualizar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 21, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.criar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 22, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.editar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 23, atualizado_em = SYSUTCDATETIME() WHERE chave = N'fornecedor.excluir';

                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 30, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.visualizar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 31, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.criar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 32, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.editar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 33, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.cancelar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 34, atualizado_em = SYSUTCDATETIME() WHERE chave = N'pedido.aprovar';

                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 40, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.visualizar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 41, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.movimentar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 42, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.ajustar';
                UPDATE permissao_sistema SET categoria = N'Estoque', ordem = 43, atualizado_em = SYSUTCDATETIME() WHERE chave = N'estoque.inventario';

                UPDATE permissao_sistema SET categoria = N'Aplicações', ordem = 20, atualizado_em = SYSUTCDATETIME() WHERE chave = N'procedimento.visualizar';
                UPDATE permissao_sistema SET categoria = N'Aplicações', ordem = 21, atualizado_em = SYSUTCDATETIME() WHERE chave = N'procedimento.criar';
                UPDATE permissao_sistema SET categoria = N'Aplicações', ordem = 22, atualizado_em = SYSUTCDATETIME() WHERE chave = N'procedimento.editar';

                UPDATE permissao_sistema SET categoria = N'Core', ordem = 10, atualizado_em = SYSUTCDATETIME() WHERE chave = N'funcionario.visualizar';
                UPDATE permissao_sistema SET categoria = N'Core', ordem = 11, atualizado_em = SYSUTCDATETIME() WHERE chave = N'funcionario.criar';
                UPDATE permissao_sistema SET categoria = N'Core', ordem = 12, atualizado_em = SYSUTCDATETIME() WHERE chave = N'funcionario.editar';

                UPDATE permissao_sistema SET categoria = N'Core', ordem = 20, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade.visualizar';
                UPDATE permissao_sistema SET categoria = N'Core', ordem = 21, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade.criar';
                UPDATE permissao_sistema SET categoria = N'Core', ordem = 22, atualizado_em = SYSUTCDATETIME() WHERE chave = N'unidade.editar';

                UPDATE permissao_sistema SET categoria = N'Core', ordem = 30, atualizado_em = SYSUTCDATETIME() WHERE chave = N'empresa.editar';
                """);
        }
    }
}
