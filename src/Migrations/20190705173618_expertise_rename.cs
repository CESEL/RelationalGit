using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class expertise_rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LossOfExpertise",
                table: "PullRequestRecommendationResults",
                newName: "Expertise");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Expertise",
                table: "PullRequestRecommendationResults",
                newName: "LossOfExpertise");
        }
    }
}
