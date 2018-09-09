using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class file_knowledgeables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileKnowledgeables",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PeriodId = table.Column<long>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: true),
                    TotalKnowledgeables = table.Column<int>(nullable: false),
                    LossSimulationId = table.Column<long>(nullable: false),
                    Knowledgeables = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileKnowledgeables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileKnowledgeables_CanonicalPath",
                table: "FileKnowledgeables",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_FileKnowledgeables_LossSimulationId",
                table: "FileKnowledgeables",
                column: "LossSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_FileKnowledgeables_PeriodId",
                table: "FileKnowledgeables",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FileKnowledgeables_TotalKnowledgeables",
                table: "FileKnowledgeables",
                column: "TotalKnowledgeables");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileKnowledgeables");
        }
    }
}
