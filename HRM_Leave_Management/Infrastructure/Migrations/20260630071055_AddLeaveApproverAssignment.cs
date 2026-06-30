using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveApproverAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_department_department_parent_department_temp_id",
                table: "department");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_department_department_id1",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_employee_manager_temp_id",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_position_position_temp_id",
                table: "employee");

            migrationBuilder.CreateTable(
                name: "leave_approver_assignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    approver_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_position_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: true),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leave_approver_assignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_leave_approver_assignment_department_target_department_id1",
                        column: x => x.target_department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_leave_approver_assignment_employee_approver_employee_id1",
                        column: x => x.approver_employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_leave_approver_assignment_positions_target_position_temp_id",
                        column: x => x.target_position_id,
                        principalTable: "position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_leave_approver_assignment_approver_employee_id_target_depar",
                table: "leave_approver_assignment",
                columns: new[] { "approver_employee_id", "target_department_id", "target_position_id" });

            migrationBuilder.CreateIndex(
                name: "ix_leave_approver_assignment_target_department_id",
                table: "leave_approver_assignment",
                column: "target_department_id");

            migrationBuilder.CreateIndex(
                name: "ix_leave_approver_assignment_target_position_id",
                table: "leave_approver_assignment",
                column: "target_position_id");

            migrationBuilder.AddForeignKey(
                name: "fk_department_department_parent_department_temp_id2",
                table: "department",
                column: "parent_department_id",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_department_department_temp_id1",
                table: "employee",
                column: "department_id",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_employee_manager_temp_id1",
                table: "employee",
                column: "manager_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_positions_position_temp_id1",
                table: "employee",
                column: "position_id",
                principalTable: "position",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_department_department_parent_department_temp_id2",
                table: "department");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_department_department_temp_id1",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_employee_manager_temp_id1",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_positions_position_temp_id1",
                table: "employee");

            migrationBuilder.DropTable(
                name: "leave_approver_assignment");

            migrationBuilder.AddForeignKey(
                name: "fk_department_department_parent_department_temp_id",
                table: "department",
                column: "parent_department_id",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_department_department_id1",
                table: "employee",
                column: "department_id",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_employee_manager_temp_id",
                table: "employee",
                column: "manager_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_position_position_temp_id",
                table: "employee",
                column: "position_id",
                principalTable: "position",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
