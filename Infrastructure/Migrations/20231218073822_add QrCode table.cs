using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addQrCodetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "qr_code_id",
                table: "voucher",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_delete",
                table: "partner",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_code_id",
                table: "partner",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "qr_code",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qr_code", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qr_code");

            migrationBuilder.DropColumn(
                name: "qr_code_id",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "is_delete",
                table: "partner");

            migrationBuilder.DropColumn(
                name: "qr_code_id",
                table: "partner");
        }
    }
}
