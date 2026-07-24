using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAutoApproveToApprovalRouteRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_approval_route_policy_department_id",
                table: "approval_route_policy");

            migrationBuilder.AddColumn<bool>(
                name: "is_auto_approve",
                table: "approval_route_rule",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "department_id",
                table: "approval_route_policy",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_policy_department_id",
                table: "approval_route_policy",
                column: "department_id",
                unique: true,
                filter: "is_active = true AND department_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_approval_route_policy_department_id",
                table: "approval_route_policy");

            migrationBuilder.DropColumn(
                name: "is_auto_approve",
                table: "approval_route_rule");

            migrationBuilder.AlterColumn<Guid>(
                name: "department_id",
                table: "approval_route_policy",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_approval_route_policy_department_id",
                table: "approval_route_policy",
                column: "department_id",
                unique: true,
                filter: "is_active = true");
        }
    }
}
