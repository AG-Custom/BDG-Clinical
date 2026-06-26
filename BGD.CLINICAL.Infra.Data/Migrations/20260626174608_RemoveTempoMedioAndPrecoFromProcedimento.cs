using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTempoMedioAndPrecoFromProcedimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preco_sugerido",
                table: "procedimento");

            migrationBuilder.DropColumn(
                name: "tempo_medio_minutos",
                table: "procedimento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "preco_sugerido",
                table: "procedimento",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tempo_medio_minutos",
                table: "procedimento",
                type: "int",
                nullable: true);
        }
    }
}
