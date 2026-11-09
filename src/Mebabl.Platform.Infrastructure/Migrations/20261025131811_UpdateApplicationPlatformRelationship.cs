using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationPlatformRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications",
                columns: new[] { "ProjectId", "ProjectPlatformId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications",
                columns: new[] { "ProjectId", "ProjectPlatformId" },
                unique: true);
        }
    }
}
