using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatemembershipclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "effective_year",
                table: "membership_class",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "membership_assigned_date",
                table: "members",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "effective_year",
                table: "membership_class");

            migrationBuilder.DropColumn(
                name: "membership_assigned_date",
                table: "members");
        }
    }
}
