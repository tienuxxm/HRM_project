using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createtablepermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_to_role_role_role_temp_id",
                table: "user_to_role");

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_name = table.Column<string>(type: "character varying(155)", maxLength: 155, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    is_update = table.Column<bool>(type: "boolean", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_to_permission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_to_permission", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_to_permission_permission_permission_temp_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_to_permission_role_role_temp_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_role_to_permission_permission_id",
                table: "role_to_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_to_permission_role_id",
                table: "role_to_permission",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_to_role_role_role_temp_id1",
                table: "user_to_role",
                column: "role_id",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_to_role_role_role_temp_id1",
                table: "user_to_role");

            migrationBuilder.DropTable(
                name: "role_to_permission");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.AddForeignKey(
                name: "fk_user_to_role_role_role_temp_id",
                table: "user_to_role",
                column: "role_id",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
