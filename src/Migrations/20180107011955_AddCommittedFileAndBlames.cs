using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class AddCommittedFileAndBlames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommitFileBlame",
                columns: table => new
                {
                    DeveloperIdentity = table.Column<string>(nullable: false),
                    CommitSha = table.Column<string>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: false),
                    AuditedLines = table.Column<int>(nullable: false),
                    AuditedPercentage = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitFileBlame", x => new { x.DeveloperIdentity, x.CommitSha, x.CanonicalPath });
                });

            migrationBuilder.CreateTable(
                name: "CommitFiles",
                columns: table => new
                {
                    CommitSha = table.Column<string>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: false),
                    NumberOfLines = table.Column<int>(nullable: false),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitFiles", x => new { x.CommitSha, x.CanonicalPath });
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitFileBlame_CanonicalPath",
                table: "CommitFileBlame",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommitFileBlame_CommitSha",
                table: "CommitFileBlame",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommitFileBlame_DeveloperIdentity",
                table: "CommitFileBlame",
                column: "DeveloperIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_CommitFiles_CanonicalPath",
                table: "CommitFiles",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommitFiles_CommitSha",
                table: "CommitFiles",
                column: "CommitSha");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitFileBlame");

            migrationBuilder.DropTable(
                name: "CommitFiles");
        }
    }
}
