using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignDeveloperProjectApplicationArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Developers_DeveloperId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Developers_DeveloperId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Code",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_DeveloperId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Applications_Code",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_DeveloperId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "DeveloperId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "PublicKey",
                table: "ProjectSdkConfigurations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectNumber",
                table: "ProjectSdkConfigurations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSdkConfigurations_PublicKey",
                table: "ProjectSdkConfigurations",
                column: "PublicKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DeveloperId_Code",
                table: "Projects",
                columns: new[] { "DeveloperId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Developers_DeveloperId",
                table: "Projects",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Developers_DeveloperId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectSdkConfigurations_PublicKey",
                table: "ProjectSdkConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Projects_DeveloperId_Code",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "PublicKey",
                table: "ProjectSdkConfigurations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectNumber",
                table: "ProjectSdkConfigurations",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Projects",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperId",
                table: "Applications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Applications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Code",
                table: "Projects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DeveloperId",
                table: "Projects",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Code",
                table: "Applications",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_DeveloperId",
                table: "Applications",
                column: "DeveloperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Developers_DeveloperId",
                table: "Applications",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Projects_ProjectId",
                table: "Applications",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Developers_DeveloperId",
                table: "Projects",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
