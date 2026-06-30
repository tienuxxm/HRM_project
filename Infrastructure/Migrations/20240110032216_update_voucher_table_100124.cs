using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_voucher_table_100124 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "index",
                table: "voucher",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_delete",
                table: "voucher",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "index",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "is_delete",
                table: "voucher");
        }
    }
}
