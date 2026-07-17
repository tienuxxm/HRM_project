using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkCalendarPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO permission (id, resource_name, is_default, display_name, created_date)
                SELECT 'b8b50e2d-dc99-43ef-b387-052637738f61', 'VIEW_WORK_CALENDAR', false, 'View Work Calendar', NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM permission WHERE resource_name = 'VIEW_WORK_CALENDAR'
                );
            ");

            migrationBuilder.Sql(@"
                INSERT INTO permission (id, resource_name, is_default, display_name, created_date)
                SELECT 'b8b50e2d-dc99-43ef-b387-052637738f62', 'UPDATE_WORK_CALENDAR', false, 'Update Work Calendar', NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM permission WHERE resource_name = 'UPDATE_WORK_CALENDAR'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM permission WHERE resource_name IN ('VIEW_WORK_CALENDAR', 'UPDATE_WORK_CALENDAR');
            ");
        }
    }
}
