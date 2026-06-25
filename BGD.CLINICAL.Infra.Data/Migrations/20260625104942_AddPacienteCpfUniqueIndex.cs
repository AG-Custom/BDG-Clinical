using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPacienteCpfUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_paciente_empresa_id_cpf",
                table: "paciente");

            migrationBuilder.CreateIndex(
                name: "ix_paciente_empresa_id_cpf",
                table: "paciente",
                columns: new[] { "empresa_id", "cpf" },
                unique: true,
                filter: "[cpf] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_paciente_empresa_id_cpf",
                table: "paciente");

            migrationBuilder.CreateIndex(
                name: "ix_paciente_empresa_id_cpf",
                table: "paciente",
                columns: new[] { "empresa_id", "cpf" });
        }
    }
}
