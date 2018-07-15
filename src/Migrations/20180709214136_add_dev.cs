using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class add_dev : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeveloperContributions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NormalizedName = table.Column<string>(nullable: true),
                    PeriodId = table.Column<long>(nullable: false),
                    TotalCommits = table.Column<int>(nullable: false),
                    IsCore = table.Column<bool>(nullable: false),
                    TotalLines = table.Column<int>(nullable: false),
                    LinesPercentage = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeveloperContributions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Developers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NormalizedName = table.Column<string>(nullable: true),
                    FirstPeriodId = table.Column<long>(nullable: true),
                    LastPeriodId = table.Column<long>(nullable: true),
                    AllPeriods = table.Column<string>(nullable: true),
                    TotalCommits = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeveloperContributions");

            migrationBuilder.DropTable(
                name: "Developers");
        }
    }
}
