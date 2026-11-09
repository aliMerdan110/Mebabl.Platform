using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredFiles_Buckets_BucketId",
                table: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_BucketId_Key",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "StoredFiles");

            migrationBuilder.RenameColumn(
                name: "StoragePath",
                table: "StoredFiles",
                newName: "Path");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "StoredFiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "StoredFiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "StoredFiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Extension",
                table: "StoredFiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "StoredFiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketId",
                table: "StoredFiles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "StoredFiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "StoredFiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "StoredFiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "StoredFiles",
                type: "character varying(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "StoredFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_ApplicationId_Path",
                table: "StoredFiles",
                columns: new[] { "ApplicationId", "Path" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_ApplicationId_StorageKey",
                table: "StoredFiles",
                columns: new[] { "ApplicationId", "StorageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_BucketId",
                table: "StoredFiles",
                column: "BucketId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_Applications_ApplicationId",
                table: "StoredFiles",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_Buckets_BucketId",
                table: "StoredFiles",
                column: "BucketId",
                principalTable: "Buckets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredFiles_Applications_ApplicationId",
                table: "StoredFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredFiles_Buckets_BucketId",
                table: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_ApplicationId_Path",
                table: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_ApplicationId_StorageKey",
                table: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_BucketId",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StoredFiles");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "StoredFiles",
                newName: "StoragePath");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "StoredFiles",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "StoredFiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "StoredFiles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Extension",
                table: "StoredFiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "StoredFiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketId",
                table: "StoredFiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "StoredFiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Metadata",
                table: "StoredFiles",
                type: "jsonb",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_BucketId_Key",
                table: "StoredFiles",
                columns: new[] { "BucketId", "Key" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_Buckets_BucketId",
                table: "StoredFiles",
                column: "BucketId",
                principalTable: "Buckets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
