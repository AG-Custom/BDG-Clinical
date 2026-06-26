using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeCompraPacienteOptionalAndAddCanceladaToAplicacaoPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "compra_paciente_id",
                table: "aplicacao_paciente",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "cancelada",
                table: "aplicacao_paciente",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_aplicacao_paciente_empresa_id_cancelada",
                table: "aplicacao_paciente",
                columns: new[] { "empresa_id", "cancelada" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_aplicacao_paciente_empresa_id_cancelada",
                table: "aplicacao_paciente");

            migrationBuilder.DropColumn(
                name: "cancelada",
                table: "aplicacao_paciente");

            migrationBuilder.AlterColumn<Guid>(
                name: "compra_paciente_id",
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
