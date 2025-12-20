using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenEye.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedOnToRefreshTokenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_AspNetUsers_UserIdId",
                table: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "UserIdId",
                table: "RefreshToken",
                newName: "ApplicationUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "RefreshToken",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_AspNetUsers_ApplicationUserId",
                table: "RefreshToken",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_AspNetUsers_ApplicationUserId",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "RefreshToken",
                newName: "UserIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_AspNetUsers_UserIdId",
                table: "RefreshToken",
                column: "UserIdId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
