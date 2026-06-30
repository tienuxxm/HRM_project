using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UPDATE_REF_MEMBER_VOUCHER : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_date",
                table: "orders");

            migrationBuilder.AddColumn<Guid>(
                name: "member_id1",
                table: "member_voucher",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_member_voucher_member_id1",
                table: "member_voucher",
                column: "member_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_member_voucher_members_member_id1",
                table: "member_voucher",
                column: "member_id1",
                principalTable: "members",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_member_voucher_members_member_id1",
                table: "member_voucher");

            migrationBuilder.DropIndex(
                name: "ix_member_voucher_member_id1",
                table: "member_voucher");

            migrationBuilder.DropColumn(
                name: "member_id1",
                table: "member_voucher");

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
