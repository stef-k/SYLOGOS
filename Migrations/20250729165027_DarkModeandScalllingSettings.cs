using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYLOGOS.Migrations
{
    /// <inheritdoc />
    public partial class DarkModeandScalllingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScaleMode",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UseDarkMode",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScaleMode",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "UseDarkMode",
                table: "Settings");
        }
    }
}
