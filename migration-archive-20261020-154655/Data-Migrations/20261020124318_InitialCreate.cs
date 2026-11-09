using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Developers_DeveloperId",
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
                name: "FK_Documents_Collections_CollectionId1",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "ApplicationPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ApplicationId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ApplicationId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CollectionId1",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId",
                table: "DeveloperRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserEmailVerificationTokens_UserId",
                table: "ApplicationUserEmailVerificationTokens");

            migrationBuilder.DropIndex(
                name: "IX_Applications_Code",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_DeveloperId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "CollectionId1",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Applications");

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

            migrationBuilder.RenameColumn(
                name: "DeveloperId",
                table: "Applications",
                newName: "ProjectPlatformId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Roles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Permissions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Applications",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Applications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Applications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeveloperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "ProjectSdkConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConfigurationVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_ProjectSdkConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSdkConfigurations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ApplicationId_Code",
                table: "Roles",
                columns: new[] { "ApplicationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ApplicationId_Code",
                table: "Permissions",
                columns: new[] { "ApplicationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId_ExpiresAt",
                table: "DeveloperRefreshTokens",
                columns: new[] { "DeveloperId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserEmailVerificationTokens_ApplicationUserId_Ex~",
                table: "ApplicationUserEmailVerificationTokens",
                columns: new[] { "ApplicationUserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProjectId_Code",
                table: "Applications",
                columns: new[] { "ProjectId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications",
                columns: new[] { "ProjectId", "ProjectPlatformId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DeveloperId_Code",
                table: "Projects",
                columns: new[] { "DeveloperId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSdkConfigurations_ProjectId",
                table: "ProjectSdkConfigurations",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSdkConfigurations_ProjectNumber",
                table: "ProjectSdkConfigurations",
                column: "ProjectNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSdkConfigurations_PublicKey",
                table: "ProjectSdkConfigurations",
                column: "PublicKey",
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

            migrationBuilder.DropTable(
                name: "ProjectPlatforms");

            migrationBuilder.DropTable(
                name: "ProjectSdkConfigurations");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ApplicationId_Code",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ApplicationId_Code",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperRefreshTokens_DeveloperId_ExpiresAt",
                table: "DeveloperRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserEmailVerificationTokens_ApplicationUserId_Ex~",
                table: "ApplicationUserEmailVerificationTokens");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ProjectId_Code",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ProjectId_ProjectPlatformId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ProjectId",
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

            migrationBuilder.RenameColumn(
                name: "ProjectPlatformId",
                table: "Applications",
                newName: "DeveloperId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Roles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Permissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Permissions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Applications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Applications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Applications",
                type: "text",
                nullable: true);

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
                name: "IX_Roles_ApplicationId",
                table: "Roles",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ApplicationId",
                table: "Permissions",
                column: "ApplicationId");

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
                name: "IX_Applications_Code",
                table: "Applications",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_DeveloperId",
                table: "Applications",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPlatforms_ApplicationId_Platform",
                table: "ApplicationPlatforms",
                columns: new[] { "ApplicationId", "Platform" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Developers_DeveloperId",
                table: "Applications",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Documents_Collections_CollectionId1",
                table: "Documents",
                column: "CollectionId1",
                principalTable: "Collections",
                principalColumn: "Id");
        }
    }
}
