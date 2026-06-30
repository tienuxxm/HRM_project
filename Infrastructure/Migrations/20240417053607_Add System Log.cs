using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_members_district_district_id1",
                table: "members");

            migrationBuilder.DropForeignKey(
                name: "fk_ward_district_district_temp_id1",
                table: "ward");

            migrationBuilder.CreateTable(
                name: "system_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_log", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_members_district_district_id",
                table: "members",
                column: "district_id",
                principalTable: "district",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_ward_district_district_temp_id",
                table: "ward",
                column: "district_id",
                principalTable: "district",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_members_district_district_id",
                table: "members");

            migrationBuilder.DropForeignKey(
                name: "fk_ward_district_district_temp_id",
                table: "ward");

            migrationBuilder.DropTable(
                name: "system_log");

            migrationBuilder.AddForeignKey(
                name: "fk_members_district_district_id1",
                table: "members",
                column: "district_id",
                principalTable: "district",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_ward_district_district_temp_id1",
                table: "ward",
                column: "district_id",
                principalTable: "district",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
