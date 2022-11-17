using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HertejDB.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "text", nullable: false),
                    StorageId = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    Added = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RatingStatus = table.Column<int>(type: "integer", nullable: false),
                    SourceAttributionSourceName = table.Column<string>(name: "SourceAttribution_SourceName", type: "text", nullable: true),
                    SourceAttributionRemoteId = table.Column<string>(name: "SourceAttribution_RemoteId", type: "text", nullable: true),
                    SourceAttributionRemoteUrl = table.Column<string>(name: "SourceAttribution_RemoteUrl", type: "text", nullable: true),
                    SourceAttributionAuthor = table.Column<string>(name: "SourceAttribution_Author", type: "text", nullable: true),
                    SourceAttributionLicense = table.Column<string>(name: "SourceAttribution_License", type: "text", nullable: true),
                    SourceAttributionCreation = table.Column<DateTimeOffset>(name: "SourceAttribution_Creation", type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageRatings",
                columns: table => new
                {
                    ImageId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IsSuitable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageRatings", x => new { x.ImageId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ImageRatings_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageRatings_UserId",
                table: "ImageRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_Category",
                table: "Images",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Images_RatingStatus",
                table: "Images",
                column: "RatingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Images_RatingStatus_Category",
                table: "Images",
                columns: new[] { "RatingStatus", "Category" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageRatings");

            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
