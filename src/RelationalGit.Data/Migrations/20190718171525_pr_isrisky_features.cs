using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Data.Migrations
{
    public partial class pr_isrisky_features : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AliasedDeveloperNames",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AliasedDeveloperNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommitBlobBlames",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CommitSha = table.Column<string>(nullable: true),
                    AuthorCommitSha = table.Column<string>(nullable: true),
                    CanonicalPath = table.Column<string>(nullable: true),
                    DeveloperIdentity = table.Column<string>(nullable: true),
                    AuditedLines = table.Column<int>(nullable: false),
                    AuditedPercentage = table.Column<double>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    Ignore = table.Column<bool>(nullable: false),
                    NormalizedDeveloperIdentity = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitBlobBlames", x => x.Id);
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
                    AuthorEmail = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    CommitterEmail = table.Column<string>(nullable: true),
                    CommitterName = table.Column<string>(nullable: true),
                    AuthorDateTime = table.Column<DateTime>(nullable: false),
                    CommitterDateTime = table.Column<DateTime>(nullable: false),
                    MessageShort = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    TreeSha = table.Column<string>(nullable: true),
                    IsMergeCommit = table.Column<bool>(nullable: false),
                    NormalizedAuthorName = table.Column<string>(nullable: true),
                    Ignore = table.Column<bool>(nullable: false),
                    PeriodId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.Sha);
                });

            migrationBuilder.CreateTable(
                name: "CommittedBlob",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CommitSha = table.Column<string>(nullable: true),
                    CanonicalPath = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    NumberOfLines = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedBlob", x => x.Id);
                });

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
                    Ignore = table.Column<bool>(nullable: false),
                    DeveloperIdentity = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedChangeBlames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommittedChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Oid = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    Status = table.Column<short>(nullable: false),
                    CommitSha = table.Column<string>(nullable: true),
                    CanonicalPath = table.Column<string>(nullable: true),
                    OldPath = table.Column<string>(nullable: true),
                    OldOid = table.Column<string>(nullable: true),
                    Extension = table.Column<string>(nullable: true),
                    FileType = table.Column<string>(nullable: true),
                    IsTest = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedChanges", x => x.Id);
                });

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
                    LinesPercentage = table.Column<double>(nullable: false),
                    TotalReviews = table.Column<int>(nullable: false)
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
                    FirstCommitPeriodId = table.Column<long>(nullable: true),
                    LastCommitPeriodId = table.Column<long>(nullable: true),
                    AllCommitPeriods = table.Column<string>(nullable: true),
                    FirstReviewPeriodId = table.Column<long>(nullable: true),
                    LastReviewPeriodId = table.Column<long>(nullable: true),
                    AllReviewPeriods = table.Column<string>(nullable: true),
                    TotalCommits = table.Column<int>(nullable: false),
                    TotalReviews = table.Column<int>(nullable: false),
                    FirstCommitDateTime = table.Column<DateTime>(nullable: true),
                    LastCommitDateTime = table.Column<DateTime>(nullable: true),
                    FirstReviewDateTime = table.Column<DateTime>(nullable: true),
                    LastReviewDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                });

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
                    Knowledgeables = table.Column<string>(nullable: true),
                    TotalAvailableCommitters = table.Column<int>(nullable: false),
                    TotalAvailableReviewers = table.Column<int>(nullable: false),
                    TotalAvailableReviewOnly = table.Column<int>(nullable: false),
                    TotalAvailableCommitOnly = table.Column<int>(nullable: false),
                    AvailableCommitters = table.Column<string>(nullable: true),
                    AvailableReviewers = table.Column<string>(nullable: true),
                    HasReviewed = table.Column<bool>(nullable: false),
                    TotalCommitters = table.Column<int>(nullable: false),
                    TotalReviewers = table.Column<int>(nullable: false),
                    TotalPullRequests = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileKnowledgeables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileTouches",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NormalizeDeveloperName = table.Column<string>(nullable: true),
                    PeriodId = table.Column<long>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: true),
                    TouchType = table.Column<string>(nullable: true),
                    LossSimulationId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTouches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GitHubGitUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GitUsername = table.Column<string>(nullable: true),
                    GitNormalizedUsername = table.Column<string>(nullable: true),
                    GitHubUsername = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubGitUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Issue",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserLogin = table.Column<string>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: true),
                    ClosedAtDateTime = table.Column<DateTime>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    HtmlUrl = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    PullRequestUrl = table.Column<string>(nullable: true),
                    PullRequestNumber = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    UserLogin = table.Column<string>(nullable: true),
                    IssueNumber = table.Column<long>(nullable: false),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    HtmltUrl = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ActorLogin = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Event = table.Column<string>(nullable: true),
                    CommitId = table.Column<string>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: false),
                    IssueNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LossSimulations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    KnowledgeShareStrategyType = table.Column<string>(nullable: true),
                    MegaPullRequestSize = table.Column<int>(nullable: false),
                    FileAbandoningThreshold = table.Column<double>(nullable: false),
                    StartDateTime = table.Column<DateTime>(nullable: false),
                    LeaversType = table.Column<string>(nullable: true),
                    EndDateTime = table.Column<DateTime>(nullable: false),
                    FilesAtRiksOwnershipThreshold = table.Column<double>(nullable: false),
                    FilesAtRiksOwnersThreshold = table.Column<int>(nullable: false),
                    LeaversOfPeriodExtendedAbsence = table.Column<int>(nullable: false),
                    KnowledgeSaveReviewerReplacementType = table.Column<string>(nullable: true),
                    FirstPeriod = table.Column<int>(nullable: false),
                    SelectedReviewersType = table.Column<string>(nullable: true),
                    MinimumActualReviewersLength = table.Column<int>(nullable: true),
                    PullRequestReviewerSelectionStrategy = table.Column<string>(nullable: true),
                    AddOnlyToUnsafePullrequests = table.Column<bool>(nullable: true),
                    NumberOfPeriodsForCalculatingProbabilityOfStay = table.Column<int>(nullable: true),
                    LgtmTerms = table.Column<string>(nullable: true),
                    MegaDevelopers = table.Column<string>(nullable: true),
                    RecommenderOption = table.Column<string>(nullable: true),
                    ChangePast = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LossSimulations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    FromDateTime = table.Column<DateTime>(nullable: false),
                    ToDateTime = table.Column<DateTime>(nullable: false),
                    FirstCommitSha = table.Column<string>(nullable: true),
                    LastCommitSha = table.Column<string>(nullable: true)
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
                    Sha = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Additions = table.Column<int>(nullable: true),
                    Deletions = table.Column<int>(nullable: true),
                    Changes = table.Column<int>(nullable: true),
                    PullRequestNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequestRecommendationResults",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PullRequestNumber = table.Column<long>(nullable: false),
                    ActualReviewers = table.Column<string>(nullable: true),
                    ActualReviewersLength = table.Column<int>(nullable: false),
                    SelectedReviewers = table.Column<string>(nullable: true),
                    SelectedReviewersLength = table.Column<int>(nullable: true),
                    SortedCandidates = table.Column<string>(nullable: true),
                    SortedCandidatesLength = table.Column<int>(nullable: true),
                    TopFiveIsAccurate = table.Column<bool>(nullable: true),
                    TopTenIsAccurate = table.Column<bool>(nullable: true),
                    MeanReciprocalRank = table.Column<double>(nullable: true),
                    Expertise = table.Column<double>(nullable: false),
                    LossSimulationId = table.Column<long>(nullable: false),
                    IsSimulated = table.Column<bool>(nullable: false),
                    IsRisky = table.Column<bool>(nullable: true),
                    Features = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestRecommendationResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequestReviewerComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    UserLogin = table.Column<string>(nullable: true),
                    CommitId = table.Column<string>(nullable: true),
                    InReplyTo = table.Column<int>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: false),
                    PullRequestReviewId = table.Column<int>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    PullRequestUrl = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    PullRequestNumber = table.Column<int>(nullable: false)
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
                    UserLogin = table.Column<string>(nullable: true),
                    CommitId = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    PullRequestNumber = table.Column<long>(nullable: false)
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
                    UserLogin = table.Column<string>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: true),
                    ClosedAtDateTime = table.Column<DateTime>(nullable: true),
                    MergedAtDateTime = table.Column<DateTime>(nullable: true),
                    BaseSha = table.Column<string>(nullable: true),
                    IssueId = table.Column<long>(nullable: false),
                    IssueUrl = table.Column<string>(nullable: true),
                    HtmlUrl = table.Column<string>(nullable: true),
                    Merged = table.Column<bool>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    MergeCommitSha = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecommendedPullRequestCandidates",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PullRequestNumber = table.Column<long>(nullable: false),
                    NormalizedReviewerName = table.Column<string>(nullable: true),
                    LossSimulationId = table.Column<long>(nullable: false),
                    Rank = table.Column<int>(nullable: false),
                    Score = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedPullRequestCandidates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecommendedPullRequestReviewers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PullRequestNumber = table.Column<long>(nullable: false),
                    NormalizedReviewerName = table.Column<string>(nullable: true),
                    LossSimulationId = table.Column<long>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedPullRequestReviewers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulatedAbondonedFiles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FilePath = table.Column<string>(nullable: true),
                    LossSimulationId = table.Column<long>(nullable: false),
                    PeriodId = table.Column<long>(nullable: false),
                    TotalLinesInPeriod = table.Column<int>(nullable: false),
                    AbandonedLinesInPeriod = table.Column<int>(nullable: false),
                    SavedLinesInPeriod = table.Column<int>(nullable: false),
                    RiskType = table.Column<string>(nullable: true)
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
                    LossSimulationId = table.Column<long>(nullable: false),
                    PeriodId = table.Column<long>(nullable: false),
                    NormalizedName = table.Column<string>(nullable: true),
                    LeavingType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulatedLeavers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserLogin = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserLogin);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AliasedDeveloperNames_Email",
                table: "AliasedDeveloperNames",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AliasedDeveloperNames_Name",
                table: "AliasedDeveloperNames",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AliasedDeveloperNames_NormalizedName",
                table: "AliasedDeveloperNames",
                column: "NormalizedName");

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
                name: "IX_CommitBlobBlames_Ignore",
                table: "CommitBlobBlames",
                column: "Ignore");

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlames_NormalizedDeveloperIdentity",
                table: "CommitBlobBlames",
                column: "NormalizedDeveloperIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_AuthorEmail",
                table: "Commits",
                column: "AuthorEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_AuthorName",
                table: "Commits",
                column: "AuthorName");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_CommitterEmail",
                table: "Commits",
                column: "CommitterEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_Ignore",
                table: "Commits",
                column: "Ignore");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_NormalizedAuthorName",
                table: "Commits",
                column: "NormalizedAuthorName");

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

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_CanonicalPath",
                table: "FileTouches",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_NormalizeDeveloperName",
                table: "FileTouches",
                column: "NormalizeDeveloperName");

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_PeriodId",
                table: "FileTouches",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_TouchType",
                table: "FileTouches",
                column: "TouchType");

            migrationBuilder.CreateIndex(
                name: "IX_Issue_Number",
                table: "Issue",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_IssueNumber",
                table: "IssueComments",
                column: "IssueNumber");

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_UserLogin",
                table: "IssueComments",
                column: "UserLogin");

            migrationBuilder.CreateIndex(
                name: "IX_IssueEvents_CommitId",
                table: "IssueEvents",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueEvents_Event",
                table: "IssueEvents",
                column: "Event");

            migrationBuilder.CreateIndex(
                name: "IX_IssueEvents_IssueNumber",
                table: "IssueEvents",
                column: "IssueNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestFiles_FileName",
                table: "PullRequestFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestFiles_PullRequestNumber",
                table: "PullRequestFiles",
                column: "PullRequestNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestRecommendationResults_LossSimulationId",
                table: "PullRequestRecommendationResults",
                column: "LossSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestRecommendationResults_PullRequestNumber",
                table: "PullRequestRecommendationResults",
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

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestCandidates_LossSimulationId",
                table: "RecommendedPullRequestCandidates",
                column: "LossSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestCandidates_NormalizedReviewerName",
                table: "RecommendedPullRequestCandidates",
                column: "NormalizedReviewerName");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestCandidates_PullRequestNumber",
                table: "RecommendedPullRequestCandidates",
                column: "PullRequestNumber");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestReviewers_NormalizedReviewerName",
                table: "RecommendedPullRequestReviewers",
                column: "NormalizedReviewerName");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestReviewers_PullRequestNumber",
                table: "RecommendedPullRequestReviewers",
                column: "PullRequestNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AliasedDeveloperNames");

            migrationBuilder.DropTable(
                name: "CommitBlobBlames");

            migrationBuilder.DropTable(
                name: "CommitRelationships");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "CommittedBlob");

            migrationBuilder.DropTable(
                name: "CommittedChangeBlames");

            migrationBuilder.DropTable(
                name: "CommittedChanges");

            migrationBuilder.DropTable(
                name: "DeveloperContributions");

            migrationBuilder.DropTable(
                name: "Developers");

            migrationBuilder.DropTable(
                name: "FileKnowledgeables");

            migrationBuilder.DropTable(
                name: "FileTouches");

            migrationBuilder.DropTable(
                name: "GitHubGitUsers");

            migrationBuilder.DropTable(
                name: "Issue");

            migrationBuilder.DropTable(
                name: "IssueComments");

            migrationBuilder.DropTable(
                name: "IssueEvents");

            migrationBuilder.DropTable(
                name: "LossSimulations");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "PullRequestFiles");

            migrationBuilder.DropTable(
                name: "PullRequestRecommendationResults");

            migrationBuilder.DropTable(
                name: "PullRequestReviewerComments");

            migrationBuilder.DropTable(
                name: "PullRequestReviewers");

            migrationBuilder.DropTable(
                name: "PullRequests");

            migrationBuilder.DropTable(
                name: "RecommendedPullRequestCandidates");

            migrationBuilder.DropTable(
                name: "RecommendedPullRequestReviewers");

            migrationBuilder.DropTable(
                name: "SimulatedAbondonedFiles");

            migrationBuilder.DropTable(
                name: "SimulatedLeavers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
