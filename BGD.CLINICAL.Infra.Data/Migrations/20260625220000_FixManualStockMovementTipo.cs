using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations;

/// <inheritdoc />
public partial class FixManualStockMovementTipo : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE movimentacao_estoque
            SET tipo = 'Entrada'
            WHERE origem = 'AJUSTE_MANUAL' AND tipo = 'Ajuste';

            UPDATE movimentacao_estoque
            SET tipo = 'Saida'
            WHERE origem = 'PERDA_MANUAL' AND tipo = 'Perda';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE movimentacao_estoque
            SET tipo = 'Ajuste'
            WHERE origem = 'AJUSTE_MANUAL' AND tipo = 'Entrada';

            UPDATE movimentacao_estoque
            SET tipo = 'Perda'
            WHERE origem = 'PERDA_MANUAL' AND tipo = 'Saida';
            """);
    }
}
