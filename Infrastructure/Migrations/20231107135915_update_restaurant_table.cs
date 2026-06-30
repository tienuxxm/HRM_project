using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_restaurant_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_day");

            migrationBuilder.DropColumn(
                name: "amenities",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "capacity",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "description",
                table: "restaurants");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "closing_at",
                table: "restaurants",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "image_key",
                table: "restaurants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "opening_at",
                table: "restaurants",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "restaurant_area_id",
                table: "restaurants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "restaurant_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_area", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_restaurants_restaurant_area_id",
                table: "restaurants",
                column: "restaurant_area_id");

            migrationBuilder.AddForeignKey(
                name: "fk_restaurants_restaurant_area_restaurant_area_id",
                table: "restaurants",
                column: "restaurant_area_id",
                principalTable: "restaurant_area",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_restaurants_restaurant_area_restaurant_area_id",
                table: "restaurants");

            migrationBuilder.DropTable(
                name: "restaurant_area");

            migrationBuilder.DropIndex(
                name: "ix_restaurants_restaurant_area_id",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "closing_at",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "image_key",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "opening_at",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "restaurant_area_id",
                table: "restaurants");

            migrationBuilder.AddColumn<int[]>(
                name: "amenities",
                table: "restaurants",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "restaurants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "restaurants",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "work_day",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    begin_shift = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    end_shift = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_day", x => x.id);
                    table.ForeignKey(
                        name: "fk_work_day_restaurant_restaurant_temp_id2",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_work_day_restaurant_id",
                table: "work_day",
                column: "restaurant_id");
        }
    }
}
