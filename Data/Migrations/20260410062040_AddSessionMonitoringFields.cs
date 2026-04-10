using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestManagementApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionMonitoringFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastClientTimeUtc",
                table: "TestSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartbeatAt",
                table: "TestSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LastKnownFullscreen",
                table: "TestSessions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownIp",
                table: "TestSessions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LastKnownTabVisible",
                table: "TestSessions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownUserAgent",
                table: "TestSessions",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastClientTimeUtc",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatAt",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "LastKnownFullscreen",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "LastKnownIp",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "LastKnownTabVisible",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "LastKnownUserAgent",
                table: "TestSessions");
        }
    }
}
