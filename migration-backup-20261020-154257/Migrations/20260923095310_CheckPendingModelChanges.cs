using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CheckPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationPlatforms_ApplicationId_Platform",
                table: "ApplicationPlatforms");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPlatforms_ApplicationId_Platform",
                table: "ApplicationPlatforms",
                columns: new[] { "ApplicationId", "Platform" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationPlatforms_ApplicationId_Platform",
                table: "ApplicationPlatforms");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPlatforms_ApplicationId_Platform",
                table: "ApplicationPlatforms",
                columns: new[] { "ApplicationId", "Platform" });
        }
    }
}
