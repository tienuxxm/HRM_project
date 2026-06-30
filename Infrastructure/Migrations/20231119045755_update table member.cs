using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatetablemember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_members_membership_class_id",
                table: "members");

            migrationBuilder.CreateIndex(
                name: "ix_members_membership_class_id",
                table: "members",
                column: "membership_class_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_members_membership_class_id",
                table: "members");

            migrationBuilder.CreateIndex(
                name: "ix_members_membership_class_id",
                table: "members",
                column: "membership_class_id",
                unique: true);
        }
    }
}
