using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KidGuard.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modes", x => x.id);
                    table.UniqueConstraint("AK_Modes_name", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    deviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    computerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    deviceToken = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    currentMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "fun"),
                    isOnline = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    lastSeen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.id);
                    table.ForeignKey(
                        name: "FK_Devices_Modes_currentMode",
                        column: x => x.currentMode,
                        principalTable: "Modes",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceLogs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    processName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLogs", x => x.id);
                    table.ForeignKey(
                        name: "FK_DeviceLogs_Devices_deviceId",
                        column: x => x.deviceId,
                        principalTable: "Devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Heartbeats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    agentVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heartbeats", x => x.id);
                    table.ForeignKey(
                        name: "FK_Heartbeats_Devices_deviceId",
                        column: x => x.deviceId,
                        principalTable: "Devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PairCodes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pairCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    expiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isUsed = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PairCodes", x => x.id);
                    table.ForeignKey(
                        name: "FK_PairCodes_Devices_deviceId",
                        column: x => x.deviceId,
                        principalTable: "Devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Modes",
                columns: new[] { "id", "description", "name" },
                values: new object[,]
                {
                    { 1, "Relaxation mode with no application blocking.", "fun" },
                    { 2, "Study mode that blocks distracting applications.", "study" },
                    { 3, "Strict mode that allows only approved applications.", "punishment" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceLogs_createdAt",
                table: "DeviceLogs",
                column: "createdAt");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceLogs_deviceId",
                table: "DeviceLogs",
                column: "deviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_currentMode",
                table: "Devices",
                column: "currentMode");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_deviceToken",
                table: "Devices",
                column: "deviceToken",
                unique: true,
                filter: "[deviceToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_userId",
                table: "Devices",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Heartbeats_createdAt",
                table: "Heartbeats",
                column: "createdAt");

            migrationBuilder.CreateIndex(
                name: "IX_Heartbeats_deviceId",
                table: "Heartbeats",
                column: "deviceId");

            migrationBuilder.CreateIndex(
                name: "IX_PairCodes_deviceId",
                table: "PairCodes",
                column: "deviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PairCodes_pairCode",
                table: "PairCodes",
                column: "pairCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceLogs");

            migrationBuilder.DropTable(
                name: "Heartbeats");

            migrationBuilder.DropTable(
                name: "PairCodes");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Modes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

