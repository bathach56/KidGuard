using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidGuard.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPairingRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PairingRequests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    connectionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    expiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    approvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    rejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PairingRequests", x => x.id);
                    table.ForeignKey(
                        name: "FK_PairingRequests_Devices_deviceId",
                        column: x => x.deviceId,
                        principalTable: "Devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PairingRequests_Users_parentId",
                        column: x => x.parentId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PairingRequests_connectionCode",
                table: "PairingRequests",
                column: "connectionCode");

            migrationBuilder.CreateIndex(
                name: "IX_PairingRequests_deviceId",
                table: "PairingRequests",
                column: "deviceId");

            migrationBuilder.CreateIndex(
                name: "IX_PairingRequests_expiresAt",
                table: "PairingRequests",
                column: "expiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PairingRequests_parentId",
                table: "PairingRequests",
                column: "parentId");

            migrationBuilder.CreateIndex(
                name: "IX_PairingRequests_status",
                table: "PairingRequests",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PairingRequests");
        }
    }
}
