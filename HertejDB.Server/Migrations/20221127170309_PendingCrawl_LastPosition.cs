using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HertejDB.Server.Migrations
{
    /// <inheritdoc />
    public partial class PendingCrawlLastPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastPosition",
                table: "PendingCrawls",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPosition",
                table: "PendingCrawls");
        }
    }
}
