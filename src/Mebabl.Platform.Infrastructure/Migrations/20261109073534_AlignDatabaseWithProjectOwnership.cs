using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignDatabaseWithProjectOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Applications_ApplicationId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Collections_CollectionId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityRules_Collections_CollectionId",
                table: "SecurityRules");

            migrationBuilder.DropIndex(
                name: "IX_SecurityRules_CollectionId_Permission",
                table: "SecurityRules");

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
                name: "IX_Collections_ApplicationId_Code",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Collections_ApplicationId_Name",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "CanDelete",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "CanQuery",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "CanRead",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "CanWrite",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "Permission",
                table: "SecurityRules");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Documents",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "Documents",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "Collections",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Applications_ProjectId_Code",
                table: "Applications",
                newName: "UX_Applications_ProjectId_Code");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Definition",
                table: "SecurityRules",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "SecurityRules",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "SecurityRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "SecurityRules",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByApplicationId",
                table: "Documents",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Collections_ProjectId_Id",
                table: "Collections",
                columns: new[] { "ProjectId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Applications_ProjectId_Id",
                table: "Applications",
                columns: new[] { "ProjectId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityRules_Project_Active_PublishedAt",
                table: "SecurityRules",
                columns: new[] { "ProjectId", "IsActive", "PublishedAt" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_SecurityRules_Project_Collection",
                table: "SecurityRules",
                columns: new[] { "ProjectId", "CollectionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SecurityRules_Definition_Object",
                table: "SecurityRules",
                sql: "jsonb_typeof(\"Definition\") = 'object'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SecurityRules_Revision_Positive",
                table: "SecurityRules",
                sql: "\"Revision\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPlatforms_Project_Platform_Active",
                table: "ProjectPlatforms",
                columns: new[] { "ProjectId", "Platform", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UX_ProjectPlatforms_Project_BundleId",
                table: "ProjectPlatforms",
                columns: new[] { "ProjectId", "BundleId" },
                unique: true,
                filter: "\"BundleId\" IS NOT NULL AND \"BundleId\" <> ''");

            migrationBuilder.CreateIndex(
                name: "UX_ProjectPlatforms_Project_Domain",
                table: "ProjectPlatforms",
                columns: new[] { "ProjectId", "Domain" },
                unique: true,
                filter: "\"Domain\" IS NOT NULL AND \"Domain\" <> ''");

            migrationBuilder.CreateIndex(
                name: "UX_ProjectPlatforms_Project_PackageName",
                table: "ProjectPlatforms",
                columns: new[] { "ProjectId", "PackageName" },
                unique: true,
                filter: "\"PackageName\" IS NOT NULL AND \"PackageName\" <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectPlatforms_Platform_NotBlank",
                table: "ProjectPlatforms",
                sql: "length(trim(\"Platform\")) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Project_Collection_CreatedAt_Id_Active",
                table: "Documents",
                columns: new[] { "ProjectId", "CollectionId", "CreatedAt", "Id" },
                descending: new[] { false, false, true, true },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Project_Collection_Creator_CreatedAt_Id_Active",
                table: "Documents",
                columns: new[] { "ProjectId", "CollectionId", "CreatedByUserId", "CreatedAt", "Id" },
                descending: new[] { false, false, false, true, true },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ProjectId_CreatedByApplicationId",
                table: "Documents",
                columns: new[] { "ProjectId", "CreatedByApplicationId" });

            migrationBuilder.CreateIndex(
                name: "UX_Documents_Project_Collection_Key_Active",
                table: "Documents",
                columns: new[] { "ProjectId", "CollectionId", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Documents_Version_Positive",
                table: "Documents",
                sql: "\"Version\" >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ProjectId_Name_Active",
                table: "Collections",
                columns: new[] { "ProjectId", "Name" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_Collections_ProjectId_Code_Active",
                table: "Collections",
                columns: new[] { "ProjectId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProjectId_IsActive",
                table: "Applications",
                columns: new[] { "ProjectId", "IsActive" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Applications_Code_Not_Empty",
                table: "Applications",
                sql: "length(btrim(\"Code\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Applications_Name_Not_Empty",
                table: "Applications",
                sql: "length(btrim(\"Name\")) > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Projects_ProjectId",
                table: "Collections",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Applications_ProjectId_CreatedByApplicationId",
                table: "Documents",
                columns: new[] { "ProjectId", "CreatedByApplicationId" },
                principalTable: "Applications",
                principalColumns: new[] { "ProjectId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Collections_ProjectId_CollectionId",
                table: "Documents",
                columns: new[] { "ProjectId", "CollectionId" },
                principalTable: "Collections",
                principalColumns: new[] { "ProjectId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Projects_ProjectId",
                table: "Documents",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityRules_Collections_ProjectId_CollectionId",
                table: "SecurityRules",
                columns: new[] { "ProjectId", "CollectionId" },
                principalTable: "Collections",
                principalColumns: new[] { "ProjectId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityRules_Projects_ProjectId",
                table: "SecurityRules",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Projects_ProjectId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Applications_ProjectId_CreatedByApplicationId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Collections_ProjectId_CollectionId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Projects_ProjectId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityRules_Collections_ProjectId_CollectionId",
                table: "SecurityRules");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityRules_Projects_ProjectId",
                table: "SecurityRules");

            migrationBuilder.DropIndex(
                name: "IX_SecurityRules_Project_Active_PublishedAt",
                table: "SecurityRules");

            migrationBuilder.DropIndex(
                name: "UX_SecurityRules_Project_Collection",
                table: "SecurityRules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SecurityRules_Definition_Object",
                table: "SecurityRules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SecurityRules_Revision_Positive",
                table: "SecurityRules");

            migrationBuilder.DropIndex(
                name: "IX_ProjectPlatforms_Project_Platform_Active",
                table: "ProjectPlatforms");

            migrationBuilder.DropIndex(
                name: "UX_ProjectPlatforms_Project_BundleId",
                table: "ProjectPlatforms");

            migrationBuilder.DropIndex(
                name: "UX_ProjectPlatforms_Project_Domain",
                table: "ProjectPlatforms");

            migrationBuilder.DropIndex(
                name: "UX_ProjectPlatforms_Project_PackageName",
                table: "ProjectPlatforms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectPlatforms_Platform_NotBlank",
                table: "ProjectPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Project_Collection_CreatedAt_Id_Active",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Project_Collection_Creator_CreatedAt_Id_Active",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ProjectId_CreatedByApplicationId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "UX_Documents_Project_Collection_Key_Active",
                table: "Documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Documents_Version_Positive",
                table: "Documents");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Collections_ProjectId_Id",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Collections_ProjectId_Name_Active",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "UX_Collections_ProjectId_Code_Active",
                table: "Collections");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Applications_ProjectId_Id",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ProjectId_IsActive",
                table: "Applications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Applications_Code_Not_Empty",
                table: "Applications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Applications_Name_Not_Empty",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Definition",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "SecurityRules");

            migrationBuilder.DropColumn(
                name: "CreatedByApplicationId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "Documents",
                newName: "ApplicationId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Documents",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "Collections",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "UX_Applications_ProjectId_Code",
                table: "Applications",
                newName: "IX_Applications_ProjectId_Code");

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
                table: "SecurityRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanQuery",
                table: "SecurityRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanRead",
                table: "SecurityRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanWrite",
                table: "SecurityRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Permission",
                table: "SecurityRules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityRules_CollectionId_Permission",
                table: "SecurityRules",
                columns: new[] { "CollectionId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationId_CollectionId_CreatedAt",
                table: "Documents",
                columns: new[] { "ApplicationId", "CollectionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationId_CollectionId_Key",
                table: "Documents",
                columns: new[] { "ApplicationId", "CollectionId", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApplicationId_CollectionId_UserId",
                table: "Documents",
                columns: new[] { "ApplicationId", "CollectionId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CollectionId",
                table: "Documents",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ApplicationId_Code",
                table: "Collections",
                columns: new[] { "ApplicationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ApplicationId_Name",
                table: "Collections",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Applications_ApplicationId",
                table: "Collections",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Collections_CollectionId",
                table: "Documents",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityRules_Collections_CollectionId",
                table: "SecurityRules",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
