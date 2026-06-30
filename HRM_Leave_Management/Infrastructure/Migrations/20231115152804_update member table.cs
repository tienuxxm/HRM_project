using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatemembertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "member_platform_identity_id",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "register_type",
                table: "members",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "member_platform_identity_id",
                table: "members");

            migrationBuilder.DropColumn(
                name: "register_type",
                table: "members");
        }
    }
}
