using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.IO;

namespace RelationalGit
{
    public class GitRepositoryDbContext:DbContext
    {
        internal static string AppSettingsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "relationalgit.json");
        public GitRepositoryDbContext()
        {

        }

        public GitRepositoryDbContext(bool autoDetectChangesEnabled=true,int commandTimeout= 150000)
        {
            Database.SetCommandTimeout(commandTimeout);
            ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile(AppSettingsPath);

            var Configuration = builder.Build();

            optionsBuilder.EnableSensitiveDataLogging(true);

            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("RelationalGit"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AliasedDeveloperNameEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FileTouchEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new RecommendedPullRequestReviewerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AliasedDeveloperNameEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new IssueEventEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new IssueEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommitRelationshipEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommitEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommitBlobBlameEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommittedBlobEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommittedChangeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PullRequestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PullRequestFileEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PullRequestReviewerCommentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new IssueCommentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FileKnowledgeableEntityTypeConfiguration());
        }

        public DbSet<IssueComment> IssueComments { get; set; }
        public DbSet<IssueEvent> IssueEvents { get; set; }
        public DbSet<Issue> Issue { get; set; }
        public DbSet<PullRequestFile> PullRequestFiles { get; set; }
        public DbSet<CommitRelationship> CommitRelationships { get; set; }
        public DbSet<Commit> Commits { get; set; }
        public DbSet<CommittedChange> CommittedChanges { get; set; }
        public DbSet<CommittedBlob> CommittedBlob { get; set; }
        public DbSet<CommitBlobBlame> CommitBlobBlames { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<PullRequestReviewer> PullRequestReviewers { get; set; }
        public DbSet<PullRequestReviewerComment> PullRequestReviewerComments { get; set; }
        public DbSet<PullRequest> PullRequests { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AliasedDeveloperName> AliasedDeveloperNames { get; set; }
        public DbSet<Developer> Developers { get; set; }
        public DbSet<DeveloperContribution> DeveloperContributions { get; set; }
        public DbSet<GitHubGitUser> GitHubGitUsers { get; set; }
        public DbSet<LossSimulation> LossSimulations { get; set; }
        public DbSet<FileTouch> FileTouches { get; set; }
        public DbSet<FileKnowledgeable> FileKnowledgeables { get; set; }
        public DbSet<RecommendedPullRequestReviewer> RecommendedPullRequestReviewers { get; set; }
        public DbSet<SimulatedAbondonedFile> SimulatedAbondonedFiles { get; set; }
        public DbSet<SimulatedLeaver> SimulatedLeavers { get; set; }
        public DbQuery<PeriodReviewerCountQuery> PeriodReviewerCountQuery {get;set;}
        public Dictionary<string, string> GetCanonicalPaths()
        {
            var canonicalResults = this.CommittedChanges.Select(m => new { m.CanonicalPath, m.Path })
                .AsNoTracking()
                .ToArray();

            var canonicalDictionary = new Dictionary<string, string>();

            var k = 0;

            foreach (var canonicalResult in canonicalResults)
            {
                if (canonicalDictionary.ContainsKey(canonicalResult.Path)
                    && canonicalDictionary[canonicalResult.Path] != canonicalResult.CanonicalPath)
                    k++;

                canonicalDictionary[canonicalResult.Path] = canonicalResult.CanonicalPath;
            }

            return canonicalDictionary;
        }

        public Dictionary<string, Dictionary<long,int>> GetPeriodReviewerCounts()
        {
            var query = @"select Commits.PeriodId,reviewers.UserLogin as GitHubUserLogin,count(*) as Count from PullRequests
            INNER JOIN Commits On Commits.Sha=PullRequests.MergeCommitSha
            INNER JOIN ( 
                SELECT distinct PullRequestNumber,PullRequestReviewers.UserLogin FROM PullRequestReviewers
                INNER JOIN PullRequests on PullRequests.Number=PullRequestReviewers.PullRequestNumber
                where PullRequestReviewers.UserLogin is not null and PullRequests.UserLogin!=PullRequestReviewers.UserLogin and Merged=1 and State!='DISMISSED'
                    UNION 
                SELECT distinct PullRequestNumber,PullRequestReviewerComments.UserLogin FROM PullRequestReviewerComments
                INNER JOIN PullRequests on PullRequests.Number=PullRequestReviewerComments.PullRequestNumber
                where PullRequestReviewerComments.UserLogin is not null and PullRequests.UserLogin!=PullRequestReviewerComments.UserLogin 
                AND PullRequestReviewerComments.CreatedAtDateTime <= PullRequests.MergedAtDateTime and Merged=1
                    UNION            
                SELECT distinct IssueNumber, IssueComments.UserLogin FROM dbo.IssueComments
				INNER JOIN PullRequests on PullRequests.Number=IssueComments.IssueNumber
				where IssueComments.UserLogin is not null and PullRequests.UserLogin!=IssueComments.UserLogin 
				AND IssueComments.CreatedAtDateTime <= PullRequests.MergedAtDateTime and Merged=1
                AND        (Body LIKE '%lgtm%') OR
                (Body LIKE '%looks good%') OR
                (Body LIKE '%its good%') OR
                (Body LIKE '%look good%') OR
                (Body LIKE '%good job%')) as reviewers
            on reviewers.PullRequestNumber=PullRequests.Number
            group by Commits.PeriodId,reviewers.UserLogin";

            var reviewersInPeriods = this.PeriodReviewerCountQuery.FromSql(query);
            var githubGitMapper = this.GitHubGitUsers.ToArray();

            var dic = new Dictionary<string, Dictionary<long,int>>();

            foreach(var reviewerInPeriod in reviewersInPeriods)
            {
                // we don't drop a reviewer if we couldn't find the corresponding normalized name. Instead we use the GitHub Login directly.
                var normalizedName = githubGitMapper.FirstOrDefault(q=>q.GitHubUsername==reviewerInPeriod.GitHubUserLogin)
                ?.GitNormalizedUsername ?? "UnmatchedGithubLogin-" + reviewerInPeriod.GitHubUserLogin;

                if(normalizedName==null)
                    continue;

                if(!dic.ContainsKey(normalizedName))
                    dic[normalizedName]=new Dictionary<long,int>();
                
                dic[normalizedName][reviewerInPeriod.PeriodId]=reviewerInPeriod.Count;
            }

            return dic;
        }
    }
    class FileKnowledgeableEntityTypeConfiguration : IEntityTypeConfiguration<FileKnowledgeable>
    {
        public void Configure(EntityTypeBuilder<FileKnowledgeable> configuration)
        {
            configuration.HasIndex(b => b.PeriodId);
            configuration.HasIndex(b => b.CanonicalPath);
            configuration.HasIndex(b => b.LossSimulationId);
            configuration.HasIndex(b => b.TotalKnowledgeables);
        }
    }

    class CommitRelationshipEntityTypeConfiguration : IEntityTypeConfiguration<CommitRelationship>
    {
        public void Configure(EntityTypeBuilder<CommitRelationship> configuration)
        {
            configuration
                .HasKey(b => new { b.Parent, b.Child });
        }
    }

    class FileTouchEntityTypeConfiguration : IEntityTypeConfiguration<FileTouch>
    {
        public void Configure(EntityTypeBuilder<FileTouch> configuration)
        {
            configuration
                .HasIndex(b => b.CanonicalPath);

            configuration
                .HasIndex(b => b.NormalizeDeveloperName);

            configuration
                .HasIndex(b => b.PeriodId);
            
            configuration
                .HasIndex(b => b.TouchType);
        }
    }

    class IssueCommentEntityTypeConfiguration : IEntityTypeConfiguration<IssueComment>
    {
        public void Configure(EntityTypeBuilder<IssueComment> configuration)
        {
            configuration
                .HasIndex(b => b.IssueNumber);

            configuration
                .HasIndex(b => b.UserLogin);
        }
    }


    class RecommendedPullRequestReviewerEntityTypeConfiguration : IEntityTypeConfiguration<RecommendedPullRequestReviewer>
    {
        public void Configure(EntityTypeBuilder<RecommendedPullRequestReviewer> configuration)
        {
            configuration
                .HasIndex(b => b.PullRequestNumber);

            configuration
                .HasIndex(b => b.NormalizedReviewerName);
        }
    }

    class CommitEntityTypeConfiguration : IEntityTypeConfiguration<Commit>
    {
        public void Configure(EntityTypeBuilder<Commit> configuration)
        {
            configuration
                .HasIndex(b => b.AuthorEmail);

              configuration
                .HasIndex(b => b.AuthorName);

            configuration
                .HasIndex(b => b.CommitterEmail);

            configuration
                .HasIndex(b => b.Sha);

            configuration.HasIndex(b => b.NormalizedAuthorName);

            configuration.HasIndex(b=>b.Ignore);

        }
    }

   class AliasedDeveloperNameEntityTypeConfiguration : IEntityTypeConfiguration<AliasedDeveloperName>
    {
        public void Configure(EntityTypeBuilder<AliasedDeveloperName> configuration)
        {
            configuration
                .HasIndex(b => b.Email);

            configuration
                .HasIndex(b => b.NormalizedName);

            configuration
                .HasIndex(b => b.Name);
        }
    }

    class IssueEventEntityTypeConfiguration : IEntityTypeConfiguration<IssueEvent>
    {
        public void Configure(EntityTypeBuilder<IssueEvent> configuration)
        {
            configuration.HasIndex(b => b.CommitId);

            configuration.HasIndex(b => b.IssueNumber);

            configuration.HasIndex(b => b.Event);
        }
    }

    class CommitBlobBlameEntityTypeConfiguration : IEntityTypeConfiguration<CommitBlobBlame>
    {
        public void Configure(EntityTypeBuilder<CommitBlobBlame> configuration)
        {

            configuration.HasIndex(b => b.NormalizedDeveloperIdentity);

            configuration.HasIndex(b => b.DeveloperIdentity);

            configuration.HasIndex(b => b.CommitSha);

            configuration.HasIndex(b => b.CanonicalPath);

            configuration.HasIndex(b=>b.Ignore);

        }
    }
    class CommittedBlobEntityTypeConfiguration : IEntityTypeConfiguration<CommittedBlob>
    {
        public void Configure(EntityTypeBuilder<CommittedBlob> configuration)
        {
            configuration.HasIndex(b => b.CommitSha);

            configuration.HasIndex(b => b.CanonicalPath);
        }
    }

    class CommittedChangeEntityTypeConfiguration : IEntityTypeConfiguration<CommittedChange>
    {
        public void Configure(EntityTypeBuilder<CommittedChange> configuration)
        {
            configuration.HasIndex(b => b.Status);

            configuration.HasIndex(b => b.CommitSha);

            configuration.HasIndex(b => b.Oid);

            configuration.HasIndex(b => b.OldOid);

            configuration.HasIndex(b => b.CanonicalPath);

            configuration.HasIndex(b => b.Path);
        }
    }

    class PullRequestEntityTypeConfiguration : IEntityTypeConfiguration<PullRequest>
    {
        public void Configure(EntityTypeBuilder<PullRequest> configuration)
        {
            configuration.HasIndex(b => b.Number);
            configuration.HasIndex(b => b.MergeCommitSha);
            configuration.HasIndex(b => b.BaseSha);
        }
    }
    class IssueEntityTypeConfiguration : IEntityTypeConfiguration<Issue>
    {
        public void Configure(EntityTypeBuilder<Issue> configuration)
        {
            configuration.HasIndex(b => b.Number);
        }
    }

    class PullRequestReviewerEntityTypeConfiguration : IEntityTypeConfiguration<PullRequestReviewer>
    {
        public void Configure(EntityTypeBuilder<PullRequestReviewer> configuration)
        {
            configuration.HasIndex(b => b.PullRequestNumber);

            configuration.HasIndex(b => b.CommitId);

            configuration.HasIndex(b => b.State);

        }
    }

    class PullRequestFileEntityTypeConfiguration : IEntityTypeConfiguration<PullRequestFile>
    {
        public void Configure(EntityTypeBuilder<PullRequestFile> configuration)
        {
            configuration.HasIndex(b => b.FileName);
            configuration.HasIndex(b => b.PullRequestNumber);
        }
    }

    class PullRequestReviewerCommentEntityTypeConfiguration : IEntityTypeConfiguration<PullRequestReviewerComment>
    {
        public void Configure(EntityTypeBuilder<PullRequestReviewerComment> configuration)
        {
            configuration.HasIndex(b => b.CommitId);

            configuration.HasIndex(b => b.Path);

            configuration.HasIndex(b => b.PullRequestReviewId);

            configuration.HasIndex(b => b.UserLogin);
        }
    }
}
