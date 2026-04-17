using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestManagementApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToTestAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TestAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TestAssignments");
        }
    }
}
