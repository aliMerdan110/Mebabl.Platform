using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveStreamingV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveStreams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreamSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveStreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreamSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreamSessions_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StreamCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveStreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyHash = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamCredentials_LiveStreams_LiveStreamId",
                        column: x => x.LiveStreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreams_ApplicationId_Name",
                table: "LiveStreams",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreamSessions_LiveStreamId_Status",
                table: "LiveStreamSessions",
                columns: new[] { "LiveStreamId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StreamCredentials_KeyHash",
                table: "StreamCredentials",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamCredentials_LiveStreamId",
                table: "StreamCredentials",
                column: "LiveStreamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveStreamSessions");

            migrationBuilder.DropTable(
                name: "StreamCredentials");

            migrationBuilder.DropTable(
                name: "LiveStreams");
        }
    }
}
