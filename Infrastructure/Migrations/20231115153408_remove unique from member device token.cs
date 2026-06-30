using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeuniquefrommemberdevicetoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_member_device_token_device_token",
                table: "member_device_token");

            migrationBuilder.DropIndex(
                name: "ix_member_device_token_identity_id",
                table: "member_device_token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_member_device_token_device_token",
                table: "member_device_token",
                column: "device_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_member_device_token_identity_id",
                table: "member_device_token",
                column: "identity_id",
                unique: true);
        }
    }
}
