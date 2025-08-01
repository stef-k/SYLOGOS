using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYLOGOS.Migrations
{
    /// <inheritdoc />
    public partial class MemberCertAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateNumber",
                table: "Members",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificatePublisher",
                table: "Members",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Members",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateNumber",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "CertificatePublisher",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Members");
        }
    }
}
