using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommitRelationships",
                columns: table => new
                {
                    Parent = table.Column<string>(nullable: false),
                    Child = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitRelationships", x => new { x.Parent, x.Child });
                });

            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    Sha = table.Column<string>(nullable: false),
                    AuthorDateTime = table.Column<DateTime>(nullable: false),
                    AuthorEmail = table.Column<string>(nullable: true),
                    CommitterDateTime = table.Column<DateTime>(nullable: false),
                    CommitterEmail = table.Column<string>(nullable: true),
                    IsMergeCommit = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    MessageShort = table.Column<string>(nullable: true),
                    TreeSha = table.Column<string>(nullable: true)
                },
                
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.Sha);
                });

            migrationBuilder.CreateTable(
                
                name: "CommittedFiles",
                columns: table => new
                {
                    Path = table.Column<string>(nullable: false),
                    CommitSha = table.Column<string>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: true),
                    Oid = table.Column<string>(nullable: true),
                    OldOid = table.Column<string>(nullable: true),
                    OldPath = table.Column<string>(nullable: true),
                    Status = table.Column<short>(nullable: false)
                },
               
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedFiles", x => new { x.Path, x.CommitSha });
                });


            migrationBuilder.CreateIndex(
                name: "IX_Commits_AuthorEmail",
                table: "Commits",
                column: "AuthorEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_CommitterEmail",
                table: "Commits",
                column: "CommitterEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_Sha",
                table: "Commits",
                column: "Sha");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedFiles_CanonicalPath",
                table: "CommittedFiles",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedFiles_CommitSha",
                table: "CommittedFiles",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedFiles_Oid",
                table: "CommittedFiles",
                column: "Oid");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedFiles_OldOid",
                table: "CommittedFiles",
                column: "OldOid");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedFiles_Path",
                table: "CommittedFiles",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedFiles_Status",
                table: "CommittedFiles",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitRelationships");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "CommittedFiles");
        }
    }
}
