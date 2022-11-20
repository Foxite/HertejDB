using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HertejDB.Server.Migrations
{
    /// <inheritdoc />
    public partial class PendingCrawlMaxAtOnce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAtOnce",
                table: "PendingCrawls",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAtOnce",
                table: "PendingCrawls");
        }
    }
}
