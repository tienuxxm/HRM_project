using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_position", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_position_code",
                table: "position",
                column: "code",
                unique: true);

            var employeeId = new Guid("f3a611b8-2a21-4f1b-a5d6-0c9f131a4cf1");
            var deptManagerId = new Guid("a9c8b7f6-6c5d-4e3d-2b1a-0f9e8d7c6b5a");
            var ceoId = new Guid("b8a7c6d5-5e4d-3c2b-1a0f-9e8d7c6b5a4f");

            migrationBuilder.InsertData(
                table: "position",
                columns: new[] { "id", "code", "name", "level", "is_active", "created_date" },
                values: new object[,]
                {
                    { employeeId, "EMPLOYEE", "Employee", 1, true, DateTime.UtcNow },
                    { deptManagerId, "DEPT_MANAGER", "Department Manager", 2, true, DateTime.UtcNow },
                    { ceoId, "CEO", "CEO", 3, true, DateTime.UtcNow }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "position_id",
                table: "employee",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_employee_position_id",
                table: "employee",
                column: "position_id");

            migrationBuilder.AddForeignKey(
                name: "fk_employee_position_position_temp_id",
                table: "employee",
                column: "position_id",
                principalTable: "position",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql($@"
                UPDATE employee 
                SET position_id = '{ceoId}' 
                WHERE position IS NOT NULL AND LOWER(position) LIKE '%ceo%';

                UPDATE employee 
                SET position_id = '{deptManagerId}' 
                WHERE position IS NOT NULL AND position_id IS NULL 
                  AND (LOWER(position) LIKE '%manager%' OR LOWER(position) LIKE '%dept%' OR LOWER(position) LIKE '%quản lý%' OR LOWER(position) LIKE '%quan ly%');

                UPDATE employee 
                SET position_id = '{employeeId}' 
                WHERE position IS NOT NULL AND position_id IS NULL;
            ");

            migrationBuilder.DropColumn(
                name: "position",
                table: "employee");

            var viewPositionPermissionId = new Guid("9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f");
            var updatePositionPermissionId = new Guid("8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e");

            migrationBuilder.Sql($@"
                INSERT INTO permission (id, resource_name, display_name, is_default, created_date)
                VALUES 
                ('{viewPositionPermissionId}', 'VIEW_POSITION', 'View Position', true, NOW()),
                ('{updatePositionPermissionId}', 'UPDATE_POSITION', 'Update Position', true, NOW())
                ON CONFLICT (id) DO NOTHING;

                INSERT INTO role_to_permission (id, permission_id, role_id, created_date)
                SELECT gen_random_uuid(), '{viewPositionPermissionId}', id, NOW()
                FROM role 
                WHERE (LOWER(resource_name) = 'admin' OR LOWER(resource_name) = 'administrator')
                  AND id NOT IN (SELECT role_id FROM role_to_permission WHERE permission_id = '{viewPositionPermissionId}');

                INSERT INTO role_to_permission (id, permission_id, role_id, created_date)
                SELECT gen_random_uuid(), '{updatePositionPermissionId}', id, NOW()
                FROM role 
                WHERE (LOWER(resource_name) = 'admin' OR LOWER(resource_name) = 'administrator')
                  AND id NOT IN (SELECT role_id FROM role_to_permission WHERE permission_id = '{updatePositionPermissionId}');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var viewPositionPermissionId = new Guid("9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f");
            var updatePositionPermissionId = new Guid("8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e");

            migrationBuilder.Sql($@"
                DELETE FROM role_to_permission WHERE permission_id IN ('{viewPositionPermissionId}', '{updatePositionPermissionId}');
                DELETE FROM permission WHERE id IN ('{viewPositionPermissionId}', '{updatePositionPermissionId}');
            ");

            migrationBuilder.AddColumn<string>(
                name: "position",
                table: "employee",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE employee e
                SET position = p.name
                FROM position p
                WHERE e.position_id = p.id;
            ");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_position_position_temp_id",
                table: "employee");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropIndex(
                name: "ix_employee_position_id",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "position_id",
                table: "employee");
        }
    }
}
