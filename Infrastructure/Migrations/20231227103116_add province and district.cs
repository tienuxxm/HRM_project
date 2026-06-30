using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addprovinceanddistrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "district_id",
                table: "members",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "province",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_province", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "district",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    province_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_district", x => x.id);
                    table.ForeignKey(
                        name: "fk_district_province_province_temp_id",
                        column: x => x.province_id,
                        principalTable: "province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ward",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ward", x => x.id);
                    table.ForeignKey(
                        name: "fk_ward_district_district_temp_id1",
                        column: x => x.district_id,
                        principalTable: "district",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_members_district_id",
                table: "members",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "ix_district_province_id",
                table: "district",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_ward_district_id",
                table: "ward",
                column: "district_id");

            migrationBuilder.AddForeignKey(
                name: "fk_members_district_district_id1",
                table: "members",
                column: "district_id",
                principalTable: "district",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_members_district_district_id1",
                table: "members");

            migrationBuilder.DropTable(
                name: "ward");

            migrationBuilder.DropTable(
                name: "district");

            migrationBuilder.DropTable(
                name: "province");

            migrationBuilder.DropIndex(
                name: "ix_members_district_id",
                table: "members");

            migrationBuilder.DropColumn(
                name: "district_id",
                table: "members");
        }
    }
}
