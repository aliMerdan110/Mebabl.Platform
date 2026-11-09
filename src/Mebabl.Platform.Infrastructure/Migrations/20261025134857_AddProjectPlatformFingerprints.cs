using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectPlatformFingerprints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectPlatformFingerprints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectPlatformId = table.Column<Guid>(type: "uuid", nullable: false),
                    FingerprintType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Value = table.Column<string>(type: "character varying(95)", maxLength: 95, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPlatformFingerprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPlatformFingerprints_ProjectPlatforms_ProjectPlatfor~",
                        column: x => x.ProjectPlatformId,
                        principalTable: "ProjectPlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPlatformFingerprints_ProjectPlatformId_FingerprintTy~",
                table: "ProjectPlatformFingerprints",
                columns: new[] { "ProjectPlatformId", "FingerprintType", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectPlatformFingerprints");
        }
    }
}
