
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveAuthFieldsToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Collections_CollectionId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CollectionId_IsDeleted",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CollectionId_Key",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_AccountId_ApplicationId",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_ApplicationId",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_NormalizedEmail",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_NormalizedUsername",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ETag",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Documents",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Documents",
                newName: "CollectionId1");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "Documents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "ApplicationUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ApplicationUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "ApplicationUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "ApplicationUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "ApplicationUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "ApplicationUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUsername",
                table: "ApplicationUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "ApplicationUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "ApplicationUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "ApplicationUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ApplicationUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            /*
             * نقل حقول المصادقة من Accounts إلى ApplicationUsers.
             * يتم ذلك قبل إنشاء الـ unique indexes.
             */
            migrationBuilder.Sql("""
                UPDATE "ApplicationUsers" au
                SET
                    "Email" = COALESCE(a."Email", ''),
                    "NormalizedEmail" = COALESCE(a."NormalizedEmail", ''),
                    "Username" = COALESCE(a."Username", ''),
                    "NormalizedUsername" = COALESCE(a."NormalizedUsername", ''),
                    "PasswordHash" = COALESCE(a."PasswordHash", ''),
                    "SecurityStamp" = COALESCE(a."SecurityStamp", ''),
                    "EmailConfirmed" = COALESCE(a."EmailConfirmed", false),
                    "TwoFactorEnabled" = COALESCE(a."TwoFactorEnabled", false),
                    "LockoutEnabled" = COALESCE(a."LockoutEnabled", false),
                    "LockoutEnd" = a."LockoutEnd",
                    "AccessFailedCount" = COALESCE(a."AccessFailedCount", 0)
                FROM "Accounts" a
                WHERE au."AccountId" = a."Id";
                """);

            /*
             * ضمان عدم وجود قيم فارغة أو مكررة قبل إنشاء الـ unique indexes.
             */
            migrationBuilder.Sql("""
                UPDATE "ApplicationUsers"
                SET
                    "Email" = 'user-' || "Id"::text || '@invalid.local',
                    "NormalizedEmail" =
                        UPPER('user-' || "Id"::text || '@invalid.local')
                WHERE "NormalizedEmail" IS NULL
                   OR "NormalizedEmail" = '';
                """);

            migrationBuilder.Sql("""
                UPDATE "ApplicationUsers"
                SET
                    "Username" = 'user_' || "Id"::text,
                    "NormalizedUsername" =
                        UPPER('user_' || "Id"::text)
                WHERE "NormalizedUsername" IS NULL
                   OR "NormalizedUsername" = '';
                """);

            migrationBuilder.Sql("""
                UPDATE "ApplicationUsers"
                SET
                    "SecurityStamp" = gen_random_uuid()::text
                WHERE "SecurityStamp" IS NULL
                   OR "SecurityStamp" = '';
                """);

            /*
             * حذف حقول المصادقة القديمة من Accounts
             * بعد اكتمال نقل البيانات.
             */
            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NormalizedUsername",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationId_CollectionId_CreatedAt",
                table: "Documents",
                columns: new[]
                {
                    "ApplicationId",
                    "CollectionId",
                    "CreatedAt"
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationId_CollectionId_Key",
                table: "Documents",
                columns: new[]
                {
                    "ApplicationId",
                    "CollectionId",
                    "Key"
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationId_CollectionId_UserId",
                table: "Documents",
                columns: new[]
                {
                    "ApplicationId",
                    "CollectionId",
                    "UserId"
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CollectionId",
                table: "Documents",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CollectionId1",
                table: "Documents",
                column: "CollectionId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_AccountId",
                table: "ApplicationUsers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_ApplicationId_NormalizedEmail",
                table: "ApplicationUsers",
                columns: new[]
                {
                    "ApplicationId",
                    "NormalizedEmail"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_ApplicationId_NormalizedUsername",
                table: "ApplicationUsers",
                columns: new[]
                {
                    "ApplicationId",
                    "NormalizedUsername"
                },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Collections_CollectionId",
                table: "Documents",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Collections_CollectionId1",
                table: "Documents",
                column: "CollectionId1",
                principalTable: "Collections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Collections_CollectionId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Collections_CollectionId1",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ApplicationId_CollectionId_CreatedAt",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ApplicationId_CollectionId_Key",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ApplicationId_CollectionId_UserId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CollectionId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CollectionId1",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_AccountId",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_ApplicationId_NormalizedEmail",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_ApplicationId_NormalizedUsername",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedUsername",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "ApplicationUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Documents",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CollectionId1",
                table: "Documents",
                newName: "DeletedBy");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "Documents",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ETag",
                table: "Documents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUsername",
                table: "Accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CollectionId_IsDeleted",
                table: "Documents",
                columns: new[]
                {
                    "CollectionId",
                    "IsDeleted"
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CollectionId_Key",
                table: "Documents",
                columns: new[]
                {
                    "CollectionId",
                    "Key"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_AccountId_ApplicationId",
                table: "ApplicationUsers",
                columns: new[]
                {
                    "AccountId",
                    "ApplicationId"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_ApplicationId",
                table: "ApplicationUsers",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NormalizedEmail",
                table: "Accounts",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NormalizedUsername",
                table: "Accounts",
                column: "NormalizedUsername",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Collections_CollectionId",
                table: "Documents",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
