using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "approval_route_policy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_route_policy", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_route_policy_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "leave_request_approval_assignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    leave_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_approver_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assignment_status = table.Column<int>(type: "integer", nullable: false),
                    assignment_reason = table.Column<int>(type: "integer", nullable: false),
                    snapshot_policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    snapshot_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    snapshot_candidate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    snapshot_level_assignment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leave_request_approval_assignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_leave_request_approval_assignment_employee_assigned_approver_id",
                        column: x => x.assigned_approver_employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_leave_request_approval_assignment_leave_request_id",
                        column: x => x.leave_request_id,
                        principalTable: "leave_request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "approval_route_level",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    level_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    level_rank = table.Column<int>(type: "integer", nullable: false),
                    can_approve_leave = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_route_level", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_route_level_approval_route_policy_policy_id",
                        column: x => x.policy_id,
                        principalTable: "approval_route_policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "approval_route_rule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    specific_approver_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_route_rule", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_route_rule_approval_route_policy_policy_id",
                        column: x => x.policy_id,
                        principalTable: "approval_route_policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_route_rule_employee_specific_approver_id",
                        column: x => x.specific_approver_employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_route_rule_position_requester_position_id",
                        column: x => x.requester_position_id,
                        principalTable: "position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "approval_route_audit_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    leave_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leave_request_approval_assignment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    previous_approver_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    new_approver_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action_type = table.Column<int>(type: "integer", nullable: false),
                    old_assignment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    new_assignment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_route_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_route_audit_log_assignment_id",
                        column: x => x.leave_request_approval_assignment_id,
                        principalTable: "leave_request_approval_assignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_route_audit_log_employee_new_approver_id",
                        column: x => x.new_approver_employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_route_audit_log_employee_previous_approver_id",
                        column: x => x.previous_approver_employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_route_audit_log_leave_request_id",
                        column: x => x.leave_request_id,
                        principalTable: "leave_request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "approval_route_level_assignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    approval_route_level_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_route_level_assignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_route_level_assignment_employee_assigned_employee_id",
                        column: x => x.assigned_employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_route_level_assignment_level_level_id",
                        column: x => x.approval_route_level_id,
                        principalTable: "approval_route_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "approval_route_rule_candidate",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    approval_route_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approval_route_level_id = table.Column<Guid>(type: "uuid", nullable: false),
                    priority_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_route_rule_candidate", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_route_rule_candidate_level_level_id",
                        column: x => x.approval_route_level_id,
                        principalTable: "approval_route_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_route_rule_candidate_rule_rule_id",
                        column: x => x.approval_route_rule_id,
                        principalTable: "approval_route_rule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_audit_log_leave_request_approval_assignment_",
                table: "approval_route_audit_log",
                column: "leave_request_approval_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_audit_log_leave_request_id_created_date",
                table: "approval_route_audit_log",
                columns: new[] { "leave_request_id", "created_date" });

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_audit_log_new_approver_employee_id",
                table: "approval_route_audit_log",
                column: "new_approver_employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_audit_log_previous_approver_employee_id",
                table: "approval_route_audit_log",
                column: "previous_approver_employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_level_policy_id_level_rank",
                table: "approval_route_level",
                columns: new[] { "policy_id", "level_rank" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_level_assignment_approval_route_level_id_is_",
                table: "approval_route_level_assignment",
                columns: new[] { "approval_route_level_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_level_assignment_assigned_employee_id_is_act",
                table: "approval_route_level_assignment",
                columns: new[] { "assigned_employee_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_policy_department_id",
                table: "approval_route_policy",
                column: "department_id",
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_rule_policy_id_requester_position_id",
                table: "approval_route_rule",
                columns: new[] { "policy_id", "requester_position_id" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_rule_requester_position_id",
                table: "approval_route_rule",
                column: "requester_position_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_rule_specific_approver_employee_id",
                table: "approval_route_rule",
                column: "specific_approver_employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_rule_candidate_approval_route_level_id",
                table: "approval_route_rule_candidate",
                column: "approval_route_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_rule_candidate_approval_route_rule_id_approv",
                table: "approval_route_rule_candidate",
                columns: new[] { "approval_route_rule_id", "approval_route_level_id" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_rule_candidate_approval_route_rule_id_priori",
                table: "approval_route_rule_candidate",
                columns: new[] { "approval_route_rule_id", "priority_order" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_leave_request_approval_assignment_assigned_approver_employe",
                table: "leave_request_approval_assignment",
                columns: new[] { "assigned_approver_employee_id", "assignment_status" });

            migrationBuilder.CreateIndex(
                name: "ix_leave_request_approval_assignment_leave_request_id",
                table: "leave_request_approval_assignment",
                column: "leave_request_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_route_audit_log");

            migrationBuilder.DropTable(
                name: "approval_route_level_assignment");

            migrationBuilder.DropTable(
                name: "approval_route_rule_candidate");

            migrationBuilder.DropTable(
                name: "leave_request_approval_assignment");

            migrationBuilder.DropTable(
                name: "approval_route_level");

            migrationBuilder.DropTable(
                name: "approval_route_rule");

            migrationBuilder.DropTable(
                name: "approval_route_policy");
        }
    }
}
