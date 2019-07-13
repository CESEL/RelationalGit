using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class RecommendedPullRequestCandidate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecommendedPullRequestCandidates",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PullRequestNumber = table.Column<long>(nullable: false),
                    NormalizedReviewerName = table.Column<string>(nullable: true),
                    LossSimulationId = table.Column<long>(nullable: false),
                    Rank = table.Column<int>(nullable: false),
                    Score = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedPullRequestCandidates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestCandidates_LossSimulationId",
                table: "RecommendedPullRequestCandidates",
                column: "LossSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestCandidates_NormalizedReviewerName",
                table: "RecommendedPullRequestCandidates",
                column: "NormalizedReviewerName");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestCandidates_PullRequestNumber",
                table: "RecommendedPullRequestCandidates",
                column: "PullRequestNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecommendedPullRequestCandidates");
        }
    }
}
