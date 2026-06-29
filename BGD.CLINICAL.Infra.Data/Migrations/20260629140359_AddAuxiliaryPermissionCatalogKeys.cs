using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuxiliaryPermissionCatalogKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'sintoma.visualizar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'sintoma.visualizar', N'Visualizar sintomas', N'Pacientes', N'PACIENTES', 20, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'sintoma.criar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'sintoma.criar', N'Criar sintoma', N'Pacientes', N'PACIENTES', 21, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'sintoma.editar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'sintoma.editar', N'Editar sintoma', N'Pacientes', N'PACIENTES', 22, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'sintoma.excluir')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'sintoma.excluir', N'Excluir sintoma', N'Pacientes', N'PACIENTES', 23, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'tipo_produto.visualizar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'tipo_produto.visualizar', N'Visualizar tipos de produto', N'Estoque', N'ESTOQUE', 14, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'tipo_produto.criar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'tipo_produto.criar', N'Criar tipo de produto', N'Estoque', N'ESTOQUE', 15, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'tipo_produto.editar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'tipo_produto.editar', N'Editar tipo de produto', N'Estoque', N'ESTOQUE', 16, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'tipo_produto.excluir')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'tipo_produto.excluir', N'Excluir tipo de produto', N'Estoque', N'ESTOQUE', 17, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'unidade_medida.visualizar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'unidade_medida.visualizar', N'Visualizar unidades de medida', N'Estoque', N'ESTOQUE', 18, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'unidade_medida.criar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'unidade_medida.criar', N'Criar unidade de medida', N'Estoque', N'ESTOQUE', 19, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'unidade_medida.editar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'unidade_medida.editar', N'Editar unidade de medida', N'Estoque', N'ESTOQUE', 20, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'unidade_medida.excluir')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'unidade_medida.excluir', N'Excluir unidade de medida', N'Estoque', N'ESTOQUE', 21, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'procedimento.visualizar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'procedimento.visualizar', N'Visualizar procedimentos', N'Aplicações', N'APLICACOES', 20, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'procedimento.criar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'procedimento.criar', N'Criar procedimento', N'Aplicações', N'APLICACOES', 21, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM permissao_sistema WHERE chave = N'procedimento.editar')
                    INSERT INTO permissao_sistema (id, chave, descricao, categoria, modulo_codigo, ordem, chave_pai, criado_em)
                    VALUES (NEWID(), N'procedimento.editar', N'Editar procedimento', N'Aplicações', N'APLICACOES', 22, NULL, SYSUTCDATETIME());
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM permissao_sistema
                WHERE chave IN (
                    N'sintoma.visualizar',
                    N'sintoma.criar',
                    N'sintoma.editar',
                    N'sintoma.excluir',
                    N'tipo_produto.visualizar',
                    N'tipo_produto.criar',
                    N'tipo_produto.editar',
                    N'tipo_produto.excluir',
                    N'unidade_medida.visualizar',
                    N'unidade_medida.criar',
                    N'unidade_medida.editar',
                    N'unidade_medida.excluir',
                    N'procedimento.visualizar',
                    N'procedimento.criar',
                    N'procedimento.editar'
                );
                """);
        }
    }
}
