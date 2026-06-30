using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_to_permission_role_role_temp_id",
                table: "role_to_permission");

            migrationBuilder.DropForeignKey(
                name: "fk_user_to_role_role_role_temp_id1",
                table: "user_to_role");

            migrationBuilder.DropForeignKey(
                name: "fk_user_to_role_user_user_temp_id",
                table: "user_to_role");

            migrationBuilder.CreateTable(
                name: "employee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    employee_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    join_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee", x => x.id);
                    table.ForeignKey(
                        name: "fk_employee_department_department_id1",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_employee_employee_manager_temp_id",
                        column: x => x.manager_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_employee_user_user_temp_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_employee_department_id",
                table: "employee",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_employee_code",
                table: "employee",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employee_manager_id",
                table: "employee",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_user_id",
                table: "employee",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_role_to_permission_role_role_temp_id1",
                table: "role_to_permission",
                column: "role_id",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_to_role_role_role_temp_id",
                table: "user_to_role",
                column: "role_id",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_to_role_user_user_temp_id1",
                table: "user_to_role",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_to_permission_role_role_temp_id1",
                table: "role_to_permission");

            migrationBuilder.DropForeignKey(
                name: "fk_user_to_role_role_role_temp_id",
                table: "user_to_role");

            migrationBuilder.DropForeignKey(
                name: "fk_user_to_role_user_user_temp_id1",
                table: "user_to_role");

            migrationBuilder.DropTable(
                name: "employee");

            migrationBuilder.AddForeignKey(
                name: "fk_role_to_permission_role_role_temp_id",
                table: "role_to_permission",
                column: "role_id",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_to_role_role_role_temp_id1",
                table: "user_to_role",
                column: "role_id",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_to_role_user_user_temp_id",
                table: "user_to_role",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
