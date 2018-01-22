using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class AddPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommitBlobBlame",
                columns: table => new
                {
                    DeveloperIdentity = table.Column<string>(nullable: false),
                    CommitSha = table.Column<string>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: false),
                    AuditedLines = table.Column<int>(nullable: false),
                    AuditedPercentage = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitBlobBlame", x => new { x.DeveloperIdentity, x.CommitSha, x.CanonicalPath });
                });

            migrationBuilder.CreateTable(
                name: "CommitPeriods",
                columns: table => new
                {
                    CommitSha = table.Column<string>(maxLength: 40, nullable: false),
                    PeriodId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitPeriods", x => new { x.CommitSha, x.PeriodId });
                });

            migrationBuilder.CreateTable(
                name: "CommittedBlob",
                columns: table => new
                {
                    CommitSha = table.Column<string>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: false),
                    NumberOfLines = table.Column<int>(nullable: false),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedBlob", x => new { x.CommitSha, x.CanonicalPath });
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FirstCommit = table.Column<string>(nullable: true),
                    FromDateTime = table.Column<DateTime>(nullable: false),
                    LastCommitSha = table.Column<string>(nullable: true),
                    ToDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlame_CanonicalPath",
                table: "CommitBlobBlame",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlame_CommitSha",
                table: "CommitBlobBlame",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlame_DeveloperIdentity",
                table: "CommitBlobBlame",
                column: "DeveloperIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_CommitPeriods_CommitSha",
                table: "CommitPeriods",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommitPeriods_PeriodId",
                table: "CommitPeriods",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedBlob_CanonicalPath",
                table: "CommittedBlob",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedBlob_CommitSha",
                table: "CommittedBlob",
                column: "CommitSha");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitBlobBlame");

            migrationBuilder.DropTable(
                name: "CommitPeriods");

            migrationBuilder.DropTable(
                name: "CommittedBlob");

            migrationBuilder.DropTable(
                name: "Periods");
        }
    }
}
