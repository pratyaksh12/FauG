using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FauG.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_TokenUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_ProviderAccounts_ProviderAccountId",
                table: "RequestLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProviderAccountId",
                table: "RequestLogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_ProviderAccounts_ProviderAccountId",
                table: "RequestLogs",
                column: "ProviderAccountId",
                principalTable: "ProviderAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_ProviderAccounts_ProviderAccountId",
                table: "RequestLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProviderAccountId",
                table: "RequestLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_ProviderAccounts_ProviderAccountId",
                table: "RequestLogs",
                column: "ProviderAccountId",
                principalTable: "ProviderAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
