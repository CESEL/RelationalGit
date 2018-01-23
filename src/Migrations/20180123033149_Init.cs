using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommitBlobBlames",
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
                    table.PrimaryKey("PK_CommitBlobBlames", x => new { x.DeveloperIdentity, x.CommitSha, x.CanonicalPath });
                });

            migrationBuilder.CreateTable(
                name: "CommitPeriods",
                columns: table => new
                {
                    CommitSha = table.Column<string>(maxLength: 40, nullable: false),
                    PeriodId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitPeriods", x => new { x.CommitSha, x.PeriodId });
                });

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
                name: "CommittedChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: true),
                    CommitSha = table.Column<string>(nullable: true),
                    Oid = table.Column<string>(nullable: true),
                    OldOid = table.Column<string>(nullable: true),
                    OldPath = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    Status = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedChanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstCommit = table.Column<string>(nullable: true),
                    FromDateTime = table.Column<DateTime>(nullable: false),
                    LastCommitSha = table.Column<string>(nullable: true),
                    ToDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequestFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Additions = table.Column<int>(nullable: true),
                    Changes = table.Column<int>(nullable: true),
                    Deletions = table.Column<int>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    PullRequestNumber = table.Column<int>(nullable: false),
                    Sha = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequestReviewerComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    CommitId = table.Column<string>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: false),
                    InReplyTo = table.Column<int>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    PullRequestReviewId = table.Column<int>(nullable: true),
                    PullRequestUrl = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    UserLogin = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestReviewerComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequestReviewers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CommitId = table.Column<string>(nullable: true),
                    PullRequestNumber = table.Column<long>(nullable: false),
                    State = table.Column<string>(nullable: true),
                    UserLogin = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestReviewers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequests",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    BaseSha = table.Column<string>(nullable: true),
                    ClosedAtDateTime = table.Column<DateTime>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: true),
                    HtmlUrl = table.Column<string>(nullable: true),
                    IssueId = table.Column<long>(nullable: false),
                    IssueUrl = table.Column<string>(nullable: true),
                    MergeCommitSha = table.Column<string>(nullable: true),
                    Merged = table.Column<bool>(nullable: false),
                    MergedAtDateTime = table.Column<DateTime>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    UserLogin = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlames_CanonicalPath",
                table: "CommitBlobBlames",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlames_CommitSha",
                table: "CommitBlobBlames",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlames_DeveloperIdentity",
                table: "CommitBlobBlames",
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
                name: "IX_CommittedBlob_CanonicalPath",
                table: "CommittedBlob",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedBlob_CommitSha",
                table: "CommittedBlob",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChanges_CanonicalPath",
                table: "CommittedChanges",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChanges_CommitSha",
                table: "CommittedChanges",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChanges_Oid",
                table: "CommittedChanges",
                column: "Oid");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChanges_OldOid",
                table: "CommittedChanges",
                column: "OldOid");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChanges_Path",
                table: "CommittedChanges",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_CommittedChanges_Status",
                table: "CommittedChanges",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestFiles_FileName",
                table: "PullRequestFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestFiles_PullRequestNumber",
                table: "PullRequestFiles",
                column: "PullRequestNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_CommitId",
                table: "PullRequestReviewerComments",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_Path",
                table: "PullRequestReviewerComments",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_PullRequestReviewId",
                table: "PullRequestReviewerComments",
                column: "PullRequestReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_UserLogin",
                table: "PullRequestReviewerComments",
                column: "UserLogin");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_BaseSha",
                table: "PullRequests",
                column: "BaseSha");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_MergeCommitSha",
                table: "PullRequests",
                column: "MergeCommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_Number",
                table: "PullRequests",
                column: "Number");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitBlobBlames");

            migrationBuilder.DropTable(
                name: "CommitPeriods");

            migrationBuilder.DropTable(
                name: "CommitRelationships");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "CommittedBlob");

            migrationBuilder.DropTable(
                name: "CommittedChanges");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "PullRequestFiles");

            migrationBuilder.DropTable(
                name: "PullRequestReviewerComments");

            migrationBuilder.DropTable(
                name: "PullRequestReviewers");

            migrationBuilder.DropTable(
                name: "PullRequests");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
