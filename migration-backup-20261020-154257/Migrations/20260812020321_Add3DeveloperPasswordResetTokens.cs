using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add3DeveloperPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperPasswordResetToken_Developers_DeveloperId",
                table: "DeveloperPasswordResetToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeveloperPasswordResetToken",
                table: "DeveloperPasswordResetToken");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperPasswordResetToken_DeveloperId",
                table: "DeveloperPasswordResetToken");

            migrationBuilder.RenameTable(
                name: "DeveloperPasswordResetToken",
                newName: "DeveloperPasswordResetTokens");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "DeveloperPasswordResetTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeveloperPasswordResetTokens",
                table: "DeveloperPasswordResetTokens",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperPasswordResetTokens_DeveloperId_ExpiresAt",
                table: "DeveloperPasswordResetTokens",
                columns: new[] { "DeveloperId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperPasswordResetTokens_TokenHash",
                table: "DeveloperPasswordResetTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperPasswordResetTokens_Developers_DeveloperId",
                table: "DeveloperPasswordResetTokens",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperPasswordResetTokens_Developers_DeveloperId",
                table: "DeveloperPasswordResetTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeveloperPasswordResetTokens",
                table: "DeveloperPasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperPasswordResetTokens_DeveloperId_ExpiresAt",
                table: "DeveloperPasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperPasswordResetTokens_TokenHash",
                table: "DeveloperPasswordResetTokens");

            migrationBuilder.RenameTable(
                name: "DeveloperPasswordResetTokens",
                newName: "DeveloperPasswordResetToken");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "DeveloperPasswordResetToken",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeveloperPasswordResetToken",
                table: "DeveloperPasswordResetToken",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperPasswordResetToken_DeveloperId",
                table: "DeveloperPasswordResetToken",
                column: "DeveloperId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperPasswordResetToken_Developers_DeveloperId",
                table: "DeveloperPasswordResetToken",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
