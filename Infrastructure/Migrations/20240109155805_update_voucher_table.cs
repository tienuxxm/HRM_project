using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_voucher_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "discount_percent",
                table: "voucher",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "discount_value",
                table: "voucher",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_discount_value",
                table: "voucher",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_order_value",
                table: "voucher",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount_percent",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "discount_value",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "max_discount_value",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "min_order_value",
                table: "voucher");
        }
    }
}
