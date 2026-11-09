using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignProjectPlatformArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserEmailVerificationTokens_ApplicationUsers_Use~",
                table: "ApplicationUserEmailVerificationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserPasswordResetTokens_ApplicationUsers_UserId",
                table: "ApplicationUserPasswordResetTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUsers_Accounts_AccountId",
                table: "ApplicationUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUsers_Applications_ApplicationId",
                table: "ApplicationUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Collections_CollectionId1",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "ApplicationPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CollectionId1",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId",
                table: "DeveloperRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserEmailVerificationTokens_UserId",
                table: "ApplicationUserEmailVerificationTokens");

            migrationBuilder.DropColumn(
                name: "CollectionId1",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ApplicationUserPasswordResetTokens",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationUserPasswordResetTokens_UserId",
                table: "ApplicationUserPasswordResetTokens",
                newName: "IX_ApplicationUserPasswordResetTokens_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ApplicationUserEmailVerificationTokens",
                newName: "ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Developers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "DeveloperRefreshTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "ApplicationUserEmailVerificationTokens",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectPlatformId",
                table: "Applications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProjectPlatforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PackageName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BundleId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ProjectPlatforms", x => x.Id);
                    table.UniqueConstraint("AK_ProjectPlatforms_ProjectId_Id", x => new { x.ProjectId, x.Id });
                    table.ForeignKey(
                        name: "FK_ProjectPlatforms_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId_ExpiresAt",
                table: "DeveloperRefreshTokens",
                columns: new[] { "DeveloperId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserEmailVerificationTokens_ApplicationUserId_Ex~",
                table: "ApplicationUserEmailVerificationTokens",
                columns: new[] { "ApplicationUserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications",
                columns: new[] { "ProjectId", "ProjectPlatformId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_ProjectPlatforms_ProjectId_ProjectPlatformId",
                table: "Applications",
                columns: new[] { "ProjectId", "ProjectPlatformId" },
                principalTable: "ProjectPlatforms",
                principalColumns: new[] { "ProjectId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserEmailVerificationTokens_ApplicationUsers_App~",
                table: "ApplicationUserEmailVerificationTokens",
                column: "ApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserPasswordResetTokens_ApplicationUsers_Applica~",
                table: "ApplicationUserPasswordResetTokens",
                column: "ApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Accounts_AccountId",
                table: "ApplicationUsers",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Applications_ApplicationId",
                table: "ApplicationUsers",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_ProjectPlatforms_ProjectId_ProjectPlatformId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserEmailVerificationTokens_ApplicationUsers_App~",
                table: "ApplicationUserEmailVerificationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserPasswordResetTokens_ApplicationUsers_Applica~",
                table: "ApplicationUserPasswordResetTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUsers_Accounts_AccountId",
                table: "ApplicationUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUsers_Applications_ApplicationId",
                table: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "ProjectPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId_ExpiresAt",
                table: "DeveloperRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserEmailVerificationTokens_ApplicationUserId_Ex~",
                table: "ApplicationUserEmailVerificationTokens");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ProjectPlatformId",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "ApplicationUserPasswordResetTokens",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationUserPasswordResetTokens_ApplicationUserId",
                table: "ApplicationUserPasswordResetTokens",
                newName: "IX_ApplicationUserPasswordResetTokens_UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "ApplicationUserEmailVerificationTokens",
                newName: "UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "CollectionId1",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Developers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "DeveloperRefreshTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "ApplicationUserEmailVerificationTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateTable(
                name: "ApplicationPlatforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BundleId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PackageName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationPlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationPlatforms_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CollectionId1",
                table: "Documents",
                column: "CollectionId1");

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId",
                table: "DeveloperRefreshTokens",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserEmailVerificationTokens_UserId",
                table: "ApplicationUserEmailVerificationTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPlatforms_ApplicationId_Platform",
                table: "ApplicationPlatforms",
                columns: new[] { "ApplicationId", "Platform" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserEmailVerificationTokens_ApplicationUsers_Use~",
                table: "ApplicationUserEmailVerificationTokens",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserPasswordResetTokens_ApplicationUsers_UserId",
                table: "ApplicationUserPasswordResetTokens",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Accounts_AccountId",
                table: "ApplicationUsers",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Applications_ApplicationId",
                table: "ApplicationUsers",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Collections_CollectionId1",
                table: "Documents",
                column: "CollectionId1",
                principalTable: "Collections",
                principalColumn: "Id");
        }
    }
}
