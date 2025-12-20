using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenEye.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRevokedToRefreshTokenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "RefreshToken",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");
        }
    }
}
