using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class total_abandaned_lines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AbandonedLinesInPeriod",
                table: "SimulatedAbondonedFiles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SavedLinesInPeriod",
                table: "SimulatedAbondonedFiles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLinesInPeriod",
                table: "SimulatedAbondonedFiles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbandonedLinesInPeriod",
                table: "SimulatedAbondonedFiles");

            migrationBuilder.DropColumn(
                name: "SavedLinesInPeriod",
                table: "SimulatedAbondonedFiles");

            migrationBuilder.DropColumn(
                name: "TotalLinesInPeriod",
                table: "SimulatedAbondonedFiles");
        }
    }
}
