using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAutoApproveAndCompanyUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_approval_route_policy_department_id",
                table: "approval_route_policy",
                newName: "ix_approval_route_policy_department_id_active_dept");

            migrationBuilder.AddCheckConstraint(
                name: "ck_approval_route_rule_auto_approve_no_specific_approver",
                table: "approval_route_rule",
                sql: "is_auto_approve = false OR specific_approver_employee_id IS NULL");

            // PostgreSQL-correct expression index for active company-level policy (department_id IS NULL)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ix_approval_route_policy_active_company_level
                ON approval_route_policy ((1))
                WHERE is_active = true AND department_id IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_approval_route_policy_active_company_level;
            ");

            migrationBuilder.DropCheckConstraint(
                name: "ck_approval_route_rule_auto_approve_no_specific_approver",
                table: "approval_route_rule");

            migrationBuilder.RenameIndex(
                name: "ix_approval_route_policy_department_id_active_dept",
                table: "approval_route_policy",
                newName: "ix_approval_route_policy_department_id");
        }
    }
}
