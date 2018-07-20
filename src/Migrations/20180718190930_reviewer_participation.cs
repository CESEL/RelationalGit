using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class reviewer_participation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstCommit",
                table: "Periods",
                newName: "FirstCommitSha");

            migrationBuilder.RenameColumn(
                name: "FileAbondonedThreshold",
                table: "LossSimulations",
                newName: "FileAbandoningThreshold");

            migrationBuilder.RenameColumn(
                name: "LastPeriodId",
                table: "Developers",
                newName: "LastReviewPeriodId");

            migrationBuilder.RenameColumn(
                name: "FirstPeriodId",
                table: "Developers",
                newName: "LastCommitPeriodId");

            migrationBuilder.RenameColumn(
                name: "AllPeriods",
                table: "Developers",
                newName: "AllReviewPeriods");

            migrationBuilder.AddColumn<int>(
                name: "FileHoardingThreshold",
                table: "LossSimulations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AllCommitPeriods",
                table: "Developers",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FirstCommitPeriodId",
                table: "Developers",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FirstReviewPeriodId",
                table: "Developers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Developers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "DeveloperContributions",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHoardingThreshold",
                table: "LossSimulations");

            migrationBuilder.DropColumn(
                name: "AllCommitPeriods",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "FirstCommitPeriodId",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "FirstReviewPeriodId",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "DeveloperContributions");

            migrationBuilder.RenameColumn(
                name: "FirstCommitSha",
                table: "Periods",
                newName: "FirstCommit");

            migrationBuilder.RenameColumn(
                name: "FileAbandoningThreshold",
                table: "LossSimulations",
                newName: "FileAbondonedThreshold");

            migrationBuilder.RenameColumn(
                name: "LastReviewPeriodId",
                table: "Developers",
                newName: "LastPeriodId");

            migrationBuilder.RenameColumn(
                name: "LastCommitPeriodId",
                table: "Developers",
                newName: "FirstPeriodId");

            migrationBuilder.RenameColumn(
                name: "AllReviewPeriods",
                table: "Developers",
                newName: "AllPeriods");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "PullRequestReviewers",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PullRequestReviewerComments",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
        }
    }
}
