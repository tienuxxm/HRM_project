using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_invoice_orders_order_id",
                table: "invoice");

            migrationBuilder.DropForeignKey(
                name: "fk_line_item_product_product_temp_id1",
                table: "line_item");

            migrationBuilder.AlterColumn<Guid>(
                name: "product_id",
                table: "line_item",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "order_id",
                table: "invoice",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_invoice_orders_order_id",
                table: "invoice",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_line_item_product_product_temp_id1",
                table: "line_item",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_invoice_orders_order_id",
                table: "invoice");

            migrationBuilder.DropForeignKey(
                name: "fk_line_item_product_product_temp_id1",
                table: "line_item");

            migrationBuilder.AlterColumn<Guid>(
                name: "product_id",
                table: "line_item",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "order_id",
                table: "invoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_invoice_orders_order_id",
                table: "invoice",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_line_item_product_product_temp_id1",
                table: "line_item",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
