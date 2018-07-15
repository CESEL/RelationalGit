using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class simulation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LossSimulations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    KnowledgeSaveStrategyType = table.Column<string>(nullable: true),
                    MegaPullRequestSize = table.Column<int>(nullable: false),
                    FileAbondonedThreshold = table.Column<double>(nullable: false),
                    StartDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LossSimulations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulatedAbondonedFiles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FilePath = table.Column<string>(nullable: true),
                    HasCoreLeaver = table.Column<bool>(nullable: false),
                    LossSimulationId = table.Column<long>(nullable: false),
                    PeriodId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulatedAbondonedFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulatedLeavers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    HasCoreLeaver = table.Column<bool>(nullable: false),
                    LossSimulationId = table.Column<long>(nullable: false),
                    PeriodId = table.Column<long>(nullable: false),
                    NormalizedName = table.Column<string>(nullable: true),
                    IsCore = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulatedLeavers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LossSimulations");

            migrationBuilder.DropTable(
                name: "SimulatedAbondonedFiles");

            migrationBuilder.DropTable(
                name: "SimulatedLeavers");
        }
    }
}
