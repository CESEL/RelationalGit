using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class leavers_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCoreLeaver",
                table: "SimulatedLeavers");

            migrationBuilder.DropColumn(
                name: "IsCore",
                table: "SimulatedLeavers");

            migrationBuilder.DropColumn(
                name: "HasCoreLeaver",
                table: "SimulatedAbondonedFiles");

            migrationBuilder.AddColumn<string>(
                name: "LeaversType",
                table: "LossSimulations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaversType",
                table: "LossSimulations");

            migrationBuilder.AddColumn<bool>(
                name: "HasCoreLeaver",
                table: "SimulatedLeavers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCore",
                table: "SimulatedLeavers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCoreLeaver",
                table: "SimulatedAbondonedFiles",
                nullable: false,
                defaultValue: false);
        }
    }
}
