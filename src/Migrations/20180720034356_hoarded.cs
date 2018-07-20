using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class hoarded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileHoardingThreshold",
                table: "LossSimulations",
                newName: "LeaversOfPeriodExtendedAbsence");

            migrationBuilder.AddColumn<string>(
                name: "LeavingType",
                table: "SimulatedLeavers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskType",
                table: "SimulatedAbondonedFiles",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FilesAtRiksOwnersThreshold",
                table: "LossSimulations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FilesAtRiksOwnershipThreshold",
                table: "LossSimulations",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeavingType",
                table: "SimulatedLeavers");

            migrationBuilder.DropColumn(
                name: "RiskType",
                table: "SimulatedAbondonedFiles");

            migrationBuilder.DropColumn(
                name: "FilesAtRiksOwnersThreshold",
                table: "LossSimulations");

            migrationBuilder.DropColumn(
                name: "FilesAtRiksOwnershipThreshold",
                table: "LossSimulations");

            migrationBuilder.RenameColumn(
                name: "LeaversOfPeriodExtendedAbsence",
                table: "LossSimulations",
                newName: "FileHoardingThreshold");
        }
    }
}
