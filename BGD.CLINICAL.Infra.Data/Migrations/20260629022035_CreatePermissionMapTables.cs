using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGD.CLINICAL.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatePermissionMapTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permissao_usuario");

            migrationBuilder.AddColumn<int>(
                name: "permission_version",
                table: "usuario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "perfil_permissao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "permissao_sistema",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    chave = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    categoria = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    modulo_codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    chave_pai = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissao_sistema", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario_permissao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    permission_key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    effect = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_permissao", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_permissao_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "perfil_permissao_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    perfil_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    permission_key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    perfil_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "ix_permissao_sistema_chave",
                table: "permissao_sistema",
                column: "chave",
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

            migrationBuilder.CreateIndex(
                name: "ix_usuario_permissao_usuario_id_permission_key",
                table: "usuario_permissao",
                columns: new[] { "usuario_id", "permission_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "perfil_permissao_item");

            migrationBuilder.DropTable(
                name: "permissao_sistema");

            migrationBuilder.DropTable(
                name: "usuario_perfil");

            migrationBuilder.DropTable(
                name: "usuario_permissao");

            migrationBuilder.DropTable(
                name: "perfil_permissao");

            migrationBuilder.DropColumn(
                name: "permission_version",
                table: "usuario");

            migrationBuilder.CreateTable(
                name: "permissao_usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    modulo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    pode_criar = table.Column<bool>(type: "bit", nullable: false),
                    pode_editar = table.Column<bool>(type: "bit", nullable: false),
                    pode_excluir = table.Column<bool>(type: "bit", nullable: false),
                    pode_visualizar = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissao_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_permissao_usuario_modulo_sistema_modulo_id",
                        column: x => x.modulo_id,
                        principalTable: "modulo_sistema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_permissao_usuario_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_permissao_usuario_modulo_id",
                table: "permissao_usuario",
                column: "modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissao_usuario_usuario_id_modulo_id",
                table: "permissao_usuario",
                columns: new[] { "usuario_id", "modulo_id" },
                unique: true);
        }
    }
}
