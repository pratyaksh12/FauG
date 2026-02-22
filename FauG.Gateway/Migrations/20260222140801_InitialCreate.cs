using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FauG.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelCost",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    InputCostPer1k = table.Column<decimal>(type: "numeric", nullable: false),
                    OutputCostPer1k = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelCost", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orgatisations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalMontlyBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCurrentSpend = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orgatisations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "text", nullable: false),
                    EncryptedApiKey = table.Column<string>(type: "text", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderAccounts_Orgatisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Orgatisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentSpend = table.Column<decimal>(type: "numeric", nullable: false),
                    Access = table.Column<bool>(type: "boolean", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Orgatisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Orgatisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VirtualKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyHash = table.Column<string>(type: "text", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirtualKeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxTokenSpend = table.Column<decimal>(type: "numeric", nullable: false),
                    RequestsPerMinute = table.Column<int>(type: "integer", nullable: false),
                    AllowedModels = table.Column<string[]>(type: "text[]", nullable: false),
                    VirtualKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Policies_VirtualKeys_VirtualKeyId",
                        column: x => x.VirtualKeyId,
                        principalTable: "VirtualKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    PromptTokens = table.Column<int>(type: "integer", nullable: false),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    VirtualKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestLogs_ProviderAccounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "ProviderAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestLogs_VirtualKeys_VirtualKeyId",
                        column: x => x.VirtualKeyId,
                        principalTable: "VirtualKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ModelCost",
                columns: new[] { "Id", "CreatedAt", "InputCostPer1k", "ModelName", "OutputCostPer1k", "Provider" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.002m, "gpt-4o", 0.00125m, "OpenAI" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.0005m, "gpt-3.5-turbo", 0.0015m, "OpenAI" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.00059m, "llama3-70b-8192", 0.00079m, "Groq" }
                });

            migrationBuilder.InsertData(
                table: "Orgatisations",
                columns: new[] { "Id", "CreatedAt", "Name", "TotalCurrentSpend", "TotalMontlyBudget" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Test Org", 0m, 1000m });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Access", "AllocatedBudget", "CreatedAt", "CurrentSpend", "OrganisationId" },
                values: new object[] { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), false, 1000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") });

            migrationBuilder.InsertData(
                table: "VirtualKeys",
                columns: new[] { "Id", "CreatedAt", "IsRevoked", "KeyHash", "LastUsedAt", "UserId" },
                values: new object[] { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "4Eewl8OQ7obBcMby4ZfuZyaYglxZ0VdkDYo6SFs/6g8=", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "AllowedModels", "CreatedAt", "MaxTokenSpend", "RequestsPerMinute", "VirtualKeyId" },
                values: new object[] { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new[] { "gpt-4o", "llama3-70b-8192" }, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1000m, 60, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_VirtualKeyId",
                table: "Policies",
                column: "VirtualKeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAccounts_OrganisationId",
                table: "ProviderAccounts",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_ProviderAccountId",
                table: "RequestLogs",
                column: "ProviderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_VirtualKeyId",
                table: "RequestLogs",
                column: "VirtualKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganisationId",
                table: "Users",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualKeys_UserId",
                table: "VirtualKeys",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelCost");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "RequestLogs");

            migrationBuilder.DropTable(
                name: "ProviderAccounts");

            migrationBuilder.DropTable(
                name: "VirtualKeys");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Orgatisations");
        }
    }
}
