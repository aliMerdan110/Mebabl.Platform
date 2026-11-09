using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SocialFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SocialComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Text = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
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
                    table.PrimaryKey("PK_SocialComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialComments_SocialComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "SocialComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SocialReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_SocialReactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialReposts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SocialReposts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SocialShares", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialComments_ApplicationId_OwnerId",
                table: "SocialComments",
                columns: new[] { "ApplicationId", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialComments_ApplicationId_PostId_CreatedAt",
                table: "SocialComments",
                columns: new[] { "ApplicationId", "PostId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialComments_ParentCommentId",
                table: "SocialComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialReactions_ApplicationId_PostId",
                table: "SocialReactions",
                columns: new[] { "ApplicationId", "PostId" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialReactions_ApplicationId_PostId_UserId",
                table: "SocialReactions",
                columns: new[] { "ApplicationId", "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialReposts_ApplicationId_PostId",
                table: "SocialReposts",
                columns: new[] { "ApplicationId", "PostId" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialReposts_ApplicationId_PostId_UserId",
                table: "SocialReposts",
                columns: new[] { "ApplicationId", "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialShares_ApplicationId_PostId",
                table: "SocialShares",
                columns: new[] { "ApplicationId", "PostId" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialShares_ApplicationId_PostId_UserId",
                table: "SocialShares",
                columns: new[] { "ApplicationId", "PostId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialComments");

            migrationBuilder.DropTable(
                name: "SocialReactions");

            migrationBuilder.DropTable(
                name: "SocialReposts");

            migrationBuilder.DropTable(
                name: "SocialShares");
        }
    }
}
