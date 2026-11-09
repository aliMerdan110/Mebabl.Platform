
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenHashStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Allow legacy plaintext refresh tokens to become nullable.
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "DeveloperRefreshTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "DeveloperRefreshTokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            // Enable PostgreSQL SHA-256 hashing support.
            migrationBuilder.Sql(
                """
                CREATE EXTENSION IF NOT EXISTS pgcrypto;
                """);

            // Migrate existing user refresh tokens to SHA-256 hashes.
            // UPPER matches .NET Convert.ToHexString output.
            migrationBuilder.Sql(
                """
                UPDATE public."RefreshTokens"
                SET
                    "TokenHash" = UPPER(
                        ENCODE(
                            DIGEST(
                                CONVERT_TO("Token", 'UTF8'),
                                'sha256'
                            ),
                            'hex'
                        )
                    ),
                    "Token" = NULL
                WHERE "Token" IS NOT NULL;
                """);

            // Migrate existing developer refresh tokens to SHA-256 hashes.
            migrationBuilder.Sql(
                """
                UPDATE public."DeveloperRefreshTokens"
                SET
                    "TokenHash" = UPPER(
                        ENCODE(
                            DIGEST(
                                CONVERT_TO("Token", 'UTF8'),
                                'sha256'
                            ),
                            'hex'
                        )
                    ),
                    "Token" = NULL
                WHERE "Token" IS NOT NULL;
                """);

            // Create unique indexes after migrating existing tokens.
            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperRefreshTokens_TokenHash",
                table: "DeveloperRefreshTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Refuse to downgrade if plaintext tokens have been removed.
            // A SHA-256 hash cannot be reversed to recover the original token.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM public."RefreshTokens"
                        WHERE "Token" IS NULL
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM public."DeveloperRefreshTokens"
                        WHERE "Token" IS NULL
                    )
                    THEN
                        RAISE EXCEPTION
                            'Cannot downgrade refresh token storage after plaintext tokens have been removed.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperRefreshTokens_TokenHash",
                table: "DeveloperRefreshTokens");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "DeveloperRefreshTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "DeveloperRefreshTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
