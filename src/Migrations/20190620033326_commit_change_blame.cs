using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class commit_change_blame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommittedChangeBlames",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CommitSha = table.Column<string>(nullable: true),
                    CanonicalPath = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    NormalizedDeveloperIdentity = table.Column<string>(nullable: true),
                    AuthorDateTime = table.Column<DateTime>(nullable: false),
                    AuditedLines = table.Column<int>(nullable: false),
                    AuditedPercentage = table.Column<double>(nullable: false),
                    Ignore = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedChangeBlames", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChangeBlames_CanonicalPath",
                table: "CommittedChangeBlames",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChangeBlames_CommitSha",
                table: "CommittedChangeBlames",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChangeBlames_NormalizedDeveloperIdentity",
                table: "CommittedChangeBlames",
                column: "NormalizedDeveloperIdentity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommittedChangeBlames");
        }
    }
}
