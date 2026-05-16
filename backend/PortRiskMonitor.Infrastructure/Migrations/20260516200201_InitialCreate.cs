using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortRiskMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MethodName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UserIdentifier = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Permissions = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMs = table.Column<long>(type: "INTEGER", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    HttpMethod = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RequestPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kris",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GreenMax = table.Column<double>(type: "REAL", nullable: false),
                    YellowMax = table.Column<double>(type: "REAL", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    HigherIsWorse = table.Column<bool>(type: "INTEGER", nullable: false),
                    MockBaseline = table.Column<double>(type: "REAL", nullable: false),
                    MockVariance = table.Column<double>(type: "REAL", nullable: false),
                    MockPattern = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false, defaultValueSql: "randomblob(8)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kris", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KriId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KriName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TriggerValue = table.Column<double>(type: "REAL", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Kris_KriId",
                        column: x => x.KriId,
                        principalTable: "Kris",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KriReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KriId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    RiskLevel = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsSimulated = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KriReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KriReadings_Kris_KriId",
                        column: x => x.KriId,
                        principalTable: "Kris",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_KriId",
                table: "Alerts",
                column: "KriId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ResolvedAt",
                table: "Alerts",
                column: "ResolvedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ExecutedAt",
                table: "AuditLogs",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserIdentifier",
                table: "AuditLogs",
                column: "UserIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_KriReadings_KriId_Timestamp",
                table: "KriReadings",
                columns: new[] { "KriId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Kris_Name",
                table: "Kris",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kris_Slug",
                table: "Kris",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "KriReadings");

            migrationBuilder.DropTable(
                name: "Kris");
        }
    }
}
