using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovePermissionsToCargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "perfil_permissao_item");

            migrationBuilder.DropTable(
                name: "usuario_perfil");

            migrationBuilder.DropTable(
                name: "perfil_permissao");

            migrationBuilder.CreateTable(
                name: "cargo_permissao_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cargo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    permission_key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargo_permissao_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_cargo_permissao_item_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cargo_permissao_item_cargo_id_permission_key",
                table: "cargo_permissao_item",
                columns: new[] { "cargo_id", "permission_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargo_permissao_item");

            migrationBuilder.CreateTable(
                name: "perfil_permissao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true),
                    nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfil_permissao", x => x.id);
                    table.ForeignKey(
                        name: "fk_perfil_permissao_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "perfil_permissao_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    perfil_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    permission_key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfil_permissao_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_perfil_permissao_item_perfil_permissao_perfil_id",
                        column: x => x.perfil_id,
                        principalTable: "perfil_permissao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_perfil",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    perfil_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_perfil", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_perfil_perfil_permissao_perfil_id",
                        column: x => x.perfil_id,
                        principalTable: "perfil_permissao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_perfil_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_perfil_permissao_empresa_id_nome",
                table: "perfil_permissao",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_perfil_permissao_item_perfil_id_permission_key",
                table: "perfil_permissao_item",
                columns: new[] { "perfil_id", "permission_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_perfil_perfil_id",
                table: "usuario_perfil",
                column: "perfil_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_perfil_usuario_id",
                table: "usuario_perfil",
                column: "usuario_id",
                unique: true);
        }
    }
}
