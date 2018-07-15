using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class knowledgemap2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LossSimulationId",
                table: "RecommendedPullRequestReviewers",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LossSimulationId",
                table: "FileTouches",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LossSimulationId",
                table: "RecommendedPullRequestReviewers");

            migrationBuilder.DropColumn(
                name: "LossSimulationId",
                table: "FileTouches");
        }
    }
}
