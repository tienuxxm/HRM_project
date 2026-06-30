using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addproductrestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "restaurant_menu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_menu", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurant_menu_restaurant_restaurant_temp_id2",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_menu_product",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    allow_delivery = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_menu_product", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurant_menu_product_products_product_id1",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_restaurant_menu_product_restaurant_menu_restaurant_menu_temp_id",
                        column: x => x.restaurant_menu_id,
                        principalTable: "restaurant_menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_menu_restaurant_id",
                table: "restaurant_menu",
                column: "restaurant_id");

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_menu_product_product_id",
                table: "restaurant_menu_product",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_menu_product_restaurant_menu_id",
                table: "restaurant_menu_product",
                column: "restaurant_menu_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "restaurant_menu_product");

            migrationBuilder.DropTable(
                name: "restaurant_menu");
        }
    }
}
