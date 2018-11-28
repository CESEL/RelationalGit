using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class pull_Request_Recommendation_Results_fixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "TopTenIsAccurate",
                table: "PullRequestRecommendationResults",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<bool>(
                name: "TopFiveIsAccurate",
                table: "PullRequestRecommendationResults",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<int>(
                name: "SortedCandidatesLength",
                table: "PullRequestRecommendationResults",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "SelectedReviewersLength",
                table: "PullRequestRecommendationResults",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "MeanReciprocalRank",
                table: "PullRequestRecommendationResults",
                nullable: true,
                oldClrType: typeof(double));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "TopTenIsAccurate",
                table: "PullRequestRecommendationResults",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "TopFiveIsAccurate",
                table: "PullRequestRecommendationResults",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortedCandidatesLength",
                table: "PullRequestRecommendationResults",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SelectedReviewersLength",
                table: "PullRequestRecommendationResults",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MeanReciprocalRank",
                table: "PullRequestRecommendationResults",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);
        }
    }
}
