using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveFlagAplicadorToCargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "flag_aplicador",
                table: "cargo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE cargo
                SET flag_aplicador = 1
                WHERE id IN (
                    SELECT DISTINCT cargo_id
                    FROM funcionario_vinculo
                    WHERE flag_aplicador = 1
                      AND cargo_id IS NOT NULL
                )
                """);

            migrationBuilder.DropColumn(
                name: "flag_aplicador",
                table: "funcionario_vinculo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "flag_aplicador",
                table: "funcionario_vinculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE fv
                SET fv.flag_aplicador = 1
                FROM funcionario_vinculo fv
                INNER JOIN cargo c ON c.id = fv.cargo_id
                WHERE c.flag_aplicador = 1
                """);

            migrationBuilder.DropColumn(
                name: "flag_aplicador",
                table: "cargo");
        }
    }
}
