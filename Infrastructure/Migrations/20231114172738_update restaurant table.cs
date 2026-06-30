using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updaterestauranttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_restaurants_restaurant_area_restaurant_area_id",
                table: "restaurants");

            migrationBuilder.AlterColumn<Guid>(
                name: "restaurant_area_id",
                table: "restaurants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_restaurants_restaurant_area_restaurant_area_id",
                table: "restaurants",
                column: "restaurant_area_id",
                principalTable: "restaurant_area",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_restaurants_restaurant_area_restaurant_area_id",
                table: "restaurants");

            migrationBuilder.AlterColumn<Guid>(
                name: "restaurant_area_id",
                table: "restaurants",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_restaurants_restaurant_area_restaurant_area_id",
                table: "restaurants",
                column: "restaurant_area_id",
                principalTable: "restaurant_area",
                principalColumn: "id");
        }
    }
}
