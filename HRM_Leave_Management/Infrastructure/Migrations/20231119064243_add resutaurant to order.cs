using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addresutauranttoorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_promotion_to_restaurant_restaurant_restaurant_temp_id1",
                table: "promotion-to-restaurant");

            migrationBuilder.AddColumn<Guid>(
                name: "restaurant_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_restaurant_id",
                table: "orders",
                column: "restaurant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_restaurant_restaurant_temp_id1",
                table: "orders",
                column: "restaurant_id",
                principalTable: "restaurants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_to_restaurant_restaurant_restaurant_temp_id2",
                table: "promotion-to-restaurant",
                column: "restaurant_id",
                principalTable: "restaurants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_restaurant_restaurant_temp_id1",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_to_restaurant_restaurant_restaurant_temp_id2",
                table: "promotion-to-restaurant");

            migrationBuilder.DropIndex(
                name: "ix_orders_restaurant_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "restaurant_id",
                table: "orders");

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_to_restaurant_restaurant_restaurant_temp_id1",
                table: "promotion-to-restaurant",
                column: "restaurant_id",
                principalTable: "restaurants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
