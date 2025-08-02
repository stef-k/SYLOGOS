using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYLOGOS.Migrations
{
    /// <inheritdoc />
    public partial class ReceiptNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiptStartNumber",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptNumber",
                table: "Memberships",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptYear",
                table: "Memberships",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptStartNumber",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "ReceiptYear",
                table: "Memberships");
        }
    }
}
