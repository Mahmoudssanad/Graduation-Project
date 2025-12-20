using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenEye.Migrations
{
    /// <inheritdoc />
    public partial class AddConfidenceProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Confidence",
                table: "CropDiseaseHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "CropDiseaseHistories");
        }
    }
}
