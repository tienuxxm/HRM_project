using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexEmployeeUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_leave_approver_assignment_employee_approver_employee_id1",
                table: "leave_approver_assignment");

            migrationBuilder.DropIndex(
                name: "ix_employee_user_id",
                table: "employee");

            migrationBuilder.CreateIndex(
                name: "IX_employee_user_id",
                table: "employee",
                column: "user_id",
                unique: true,
                filter: "user_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_leave_approver_assignment_employee_approver_id",
                table: "leave_approver_assignment",
                column: "approver_employee_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_leave_approver_assignment_employee_approver_id",
                table: "leave_approver_assignment");

            migrationBuilder.DropIndex(
                name: "IX_employee_user_id",
                table: "employee");

            migrationBuilder.CreateIndex(
                name: "ix_employee_user_id",
                table: "employee",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_leave_approver_assignment_employee_approver_employee_id1",
                table: "leave_approver_assignment",
                column: "approver_employee_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
