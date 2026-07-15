using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePackageApplicationCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quantidade_aplicacoes",
                table: "pacote");

            migrationBuilder.DropColumn(
                name: "quantidade_aplicacoes",
                table: "compra_paciente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "quantidade_aplicacoes",
                table: "pacote",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "quantidade_aplicacoes",
                table: "compra_paciente",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
