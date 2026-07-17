using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarImportAndRecalculationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_employee_employee_manager_temp_id1",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_leave_balance_leave_type_leave_type_temp_id",
                table: "leave_balance");

            migrationBuilder.DropForeignKey(
                name: "fk_leave_request_employee_employee_id1",
                table: "leave_request");

            migrationBuilder.DropForeignKey(
                name: "fk_leave_request_leave_type_leave_type_temp_id",
                table: "leave_request");

            migrationBuilder.CreateTable(
                name: "calendar_import_batch",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_import_batch", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_import_batch_row",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: true),
                    day_type = table.Column<int>(type: "integer", nullable: true),
                    work_shift = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    raw_date = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    raw_day_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    raw_work_shift = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_import_batch_row", x => x.id);
                    table.ForeignKey(
                        name: "fk_calendar_import_batch_row_calendar_import_batch_batch_id1",
                        column: x => x.batch_id,
                        principalTable: "calendar_import_batch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leave_request_recalculation_audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    leave_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leave_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_status = table.Column<int>(type: "integer", nullable: false),
                    new_status = table.Column<int>(type: "integer", nullable: false),
                    old_duration = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    new_duration = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    old_processed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    old_processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    old_comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recalculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leave_request_recalculation_audit", x => x.id);
                    table.ForeignKey(
                        name: "fk_leave_request_recalculation_audit_calendar_import_batch_bat",
                        column: x => x.batch_id,
                        principalTable: "calendar_import_batch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_leave_request_recalculation_audit_employee_employee_id1",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_leave_request_recalculation_audit_leave_request_leave_reque",
                        column: x => x.leave_request_id,
                        principalTable: "leave_request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_leave_request_recalculation_audit_leave_type_leave_type_tem",
                        column: x => x.leave_type_id,
                        principalTable: "leave_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_import_batch_row_batch_id",
                table: "calendar_import_batch_row",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_leave_request_recalculation_audit_batch_id",
                table: "leave_request_recalculation_audit",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_leave_request_recalculation_audit_employee_id",
                table: "leave_request_recalculation_audit",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_leave_request_recalculation_audit_leave_request_id",
                table: "leave_request_recalculation_audit",
                column: "leave_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_leave_request_recalculation_audit_leave_type_id",
                table: "leave_request_recalculation_audit",
                column: "leave_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_employee_employee_manager_temp_id2",
                table: "employee",
                column: "manager_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_leave_balance_leave_type_leave_type_temp_id2",
                table: "leave_balance",
                column: "leave_type_id",
                principalTable: "leave_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_leave_request_employee_employee_temp_id3",
                table: "leave_request",
                column: "employee_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_leave_request_leave_type_leave_type_temp_id1",
                table: "leave_request",
                column: "leave_type_id",
                principalTable: "leave_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_employee_employee_manager_temp_id2",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_leave_balance_leave_type_leave_type_temp_id2",
                table: "leave_balance");

            migrationBuilder.DropForeignKey(
                name: "fk_leave_request_employee_employee_temp_id3",
                table: "leave_request");

            migrationBuilder.DropForeignKey(
                name: "fk_leave_request_leave_type_leave_type_temp_id1",
                table: "leave_request");

            migrationBuilder.DropTable(
                name: "calendar_import_batch_row");

            migrationBuilder.DropTable(
                name: "leave_request_recalculation_audit");

            migrationBuilder.DropTable(
                name: "calendar_import_batch");

            migrationBuilder.AddForeignKey(
                name: "fk_employee_employee_manager_temp_id1",
                table: "employee",
                column: "manager_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_leave_balance_leave_type_leave_type_temp_id",
                table: "leave_balance",
                column: "leave_type_id",
                principalTable: "leave_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_leave_request_employee_employee_id1",
                table: "leave_request",
                column: "employee_id",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_leave_request_leave_type_leave_type_temp_id",
                table: "leave_request",
                column: "leave_type_id",
                principalTable: "leave_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
