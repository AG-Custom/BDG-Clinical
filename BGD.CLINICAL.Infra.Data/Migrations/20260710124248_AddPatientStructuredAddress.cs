using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientStructuredAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bairro",
                table: "paciente",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cep",
                table: "paciente",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cidade",
                table: "paciente",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complemento",
                table: "paciente",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logradouro",
                table: "paciente",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero",
                table: "paciente",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uf",
                table: "paciente",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bairro",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "cep",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "cidade",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "complemento",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "logradouro",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "numero",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "uf",
                table: "paciente");
        }
    }
}
