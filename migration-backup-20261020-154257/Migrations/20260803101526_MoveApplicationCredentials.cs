using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveApplicationCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_ApiKey",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationCredentials_ApplicationId",
                table: "ApplicationCredentials");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ApiSecret",
                table: "Applications");

            migrationBuilder.CreateTable(
                name: "DeveloperRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeveloperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_DeveloperRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeveloperRefreshTokens_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationCredentials_ApplicationId",
                table: "ApplicationCredentials",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId",
                table: "DeveloperRefreshTokens",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperRefreshTokens_Token",
                table: "DeveloperRefreshTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeveloperRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationCredentials_ApplicationId",
                table: "ApplicationCredentials");

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Applications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApiSecret",
                table: "Applications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApiKey",
                table: "Applications",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationCredentials_ApplicationId",
                table: "ApplicationCredentials",
                column: "ApplicationId",
                unique: true);
        }
    }
}
