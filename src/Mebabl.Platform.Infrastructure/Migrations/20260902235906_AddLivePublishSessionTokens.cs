using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLivePublishSessionTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KeyHash",
                table: "StreamCredentials",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "LiveStreamSessions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LiveStreamSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishTokenExpiresAt",
                table: "LiveStreamSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PublishTokenHash",
                table: "LiveStreamSessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PublisherUserId",
                table: "LiveStreamSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSessions_LiveStreamId",
                table: "LiveStreamSessions",
                column: "LiveStreamId",
                unique: true,
                filter: "\"Status\" <> 2");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSessions_LiveStreamId_PublisherUserId",
                table: "LiveStreamSessions",
                columns: new[] { "LiveStreamId", "PublisherUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSessions_PublishTokenHash",
                table: "LiveStreamSessions",
                column: "PublishTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LiveStreamSessions_LiveStreamId",
                table: "LiveStreamSessions");

            migrationBuilder.DropIndex(
                name: "IX_LiveStreamSessions_LiveStreamId_PublisherUserId",
                table: "LiveStreamSessions");

            migrationBuilder.DropIndex(
                name: "IX_LiveStreamSessions_PublishTokenHash",
                table: "LiveStreamSessions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LiveStreamSessions");

            migrationBuilder.DropColumn(
                name: "PublishTokenExpiresAt",
                table: "LiveStreamSessions");

            migrationBuilder.DropColumn(
                name: "PublishTokenHash",
                table: "LiveStreamSessions");

            migrationBuilder.DropColumn(
                name: "PublisherUserId",
                table: "LiveStreamSessions");

            migrationBuilder.AlterColumn<string>(
                name: "KeyHash",
                table: "StreamCredentials",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "LiveStreamSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
