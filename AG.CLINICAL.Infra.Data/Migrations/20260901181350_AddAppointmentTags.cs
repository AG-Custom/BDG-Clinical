using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AG.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tag_agendamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    cor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_agendamento", x => x.id);
                    table.ForeignKey(
                        name: "fk_tag_agendamento_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agendamento_tag",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    agendamento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tag_agendamento_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agendamento_tag", x => x.id);
                    table.ForeignKey(
                        name: "fk_agendamento_tag_agendamento_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_agendamento_tag_tag_agendamento_tag_agendamento_id",
                        column: x => x.tag_agendamento_id,
                        principalTable: "tag_agendamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_tag_agendamento_id_ordem",
                table: "agendamento_tag",
                columns: new[] { "agendamento_id", "ordem" });

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_tag_agendamento_id_tag_agendamento_id",
                table: "agendamento_tag",
                columns: new[] { "agendamento_id", "tag_agendamento_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agendamento_tag_tag_agendamento_id",
                table: "agendamento_tag",
                column: "tag_agendamento_id");

            migrationBuilder.CreateIndex(
                name: "ix_tag_agendamento_empresa_id_nome",
                table: "tag_agendamento",
                columns: new[] { "empresa_id", "nome" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agendamento_tag");

            migrationBuilder.DropTable(
                name: "tag_agendamento");
        }
    }
}
