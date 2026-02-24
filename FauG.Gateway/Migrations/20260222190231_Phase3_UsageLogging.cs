using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FauG.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_UsageLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                table: "RequestLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalTokens",
                table: "RequestLogs");
        }
    }
}
