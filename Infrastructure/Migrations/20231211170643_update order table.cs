using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateordertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_members_member_id",
                table: "orders");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_members_member_id1",
                table: "orders",
                column: "member_id",
                principalTable: "members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_members_member_id1",
                table: "orders");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_members_member_id",
                table: "orders",
                column: "member_id",
                principalTable: "members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
