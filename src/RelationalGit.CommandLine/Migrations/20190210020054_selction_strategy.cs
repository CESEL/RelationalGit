using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class selction_strategy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddOneReviewerToUnsafePullRequests",
                table: "LossSimulations",
                newName: "AddOnlyToUnsafePullrequests");

            migrationBuilder.AddColumn<string>(
                name: "PullRequestReviewerSelectionStrategy",
                table: "LossSimulations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PullRequestReviewerSelectionStrategy",
                table: "LossSimulations");

            migrationBuilder.RenameColumn(
                name: "AddOnlyToUnsafePullrequests",
                table: "LossSimulations",
                newName: "AddOneReviewerToUnsafePullRequests");
        }
    }
}
