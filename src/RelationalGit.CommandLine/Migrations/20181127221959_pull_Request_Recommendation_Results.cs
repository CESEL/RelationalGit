using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class pull_Request_Recommendation_Results : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KnowledgeSaveReviewerReplacementType",
                table: "LossSimulations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PullRequestRecommendationResults",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PullRequestNumber = table.Column<long>(nullable: false),
                    ActualReviewers = table.Column<string>(nullable: true),
                    ActualReviewersLength = table.Column<int>(nullable: false),
                    SelectedReviewers = table.Column<string>(nullable: true),
                    SelectedReviewersLength = table.Column<int>(nullable: false),
                    SortedCandidates = table.Column<string>(nullable: true),
                    SortedCandidatesLength = table.Column<int>(nullable: false),
                    TopFiveIsAccurate = table.Column<bool>(nullable: false),
                    TopTenIsAccurate = table.Column<bool>(nullable: false),
                    MeanReciprocalRank = table.Column<double>(nullable: false),
                    LossSimulationId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestRecommendationResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestRecommendationResults_LossSimulationId",
                table: "PullRequestRecommendationResults",
                column: "LossSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestRecommendationResults_PullRequestNumber",
                table: "PullRequestRecommendationResults",
                column: "PullRequestNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PullRequestRecommendationResults");

            migrationBuilder.DropColumn(
                name: "KnowledgeSaveReviewerReplacementType",
                table: "LossSimulations");
        }
    }
}
