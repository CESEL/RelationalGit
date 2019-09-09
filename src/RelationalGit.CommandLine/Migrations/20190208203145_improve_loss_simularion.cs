using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class improve_loss_simularion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AddOneReviewerToUnsafePullRequests",
                table: "LossSimulations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LgtmTerms",
                table: "LossSimulations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumActualReviewersLength",
                table: "LossSimulations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPeriodsForCalculatingProbabilityOfStay",
                table: "LossSimulations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddOneReviewerToUnsafePullRequests",
                table: "LossSimulations");

            migrationBuilder.DropColumn(
                name: "LgtmTerms",
                table: "LossSimulations");

            migrationBuilder.DropColumn(
                name: "MinimumActualReviewersLength",
                table: "LossSimulations");

            migrationBuilder.DropColumn(
                name: "NumberOfPeriodsForCalculatingProbabilityOfStay",
                table: "LossSimulations");
        }
    }
}
