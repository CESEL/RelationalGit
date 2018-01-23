using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RelationalGit
{
    public class GitRepositoryDbContext:DbContext
    {
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
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Trusted_Connection=True;Database=RefugeeAI");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfiguration(new CommitRelationshipEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommitPeriodEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommitEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommitBlobBlameEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommittedBlobEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommittedChangeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PullRequestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PullRequestFileEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PullRequestReviewerCommentEntityTypeConfiguration());
        }

        public DbSet<PullRequestFile> PullRequestFiles { get; set; }
        public DbSet<CommitRelationship> CommitRelationships { get; set; }
        public DbSet<Commit> Commits { get; set; }
        public DbSet<CommittedChange> CommittedChanges { get; set; }
        public DbSet<CommittedBlob> CommittedBlob { get; set; }
        public DbSet<CommitBlobBlame> CommitBlobBlames { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<CommitPeriod> CommitPeriods { get; set; }
        public DbSet<PullRequestReviewer> PullRequestReviewers { get; set; }
        public DbSet<PullRequestReviewerComment> PullRequestReviewerComments { get; set; }
        public DbSet<PullRequest> PullRequests { get; set; }
        public DbSet<User> Users { get; set; }
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
    }

    class CommitRelationshipEntityTypeConfiguration : IEntityTypeConfiguration<CommitRelationship>
    {
        public void Configure(EntityTypeBuilder<CommitRelationship> configuration)
        {
            configuration
                .HasKey(b => new { b.Parent, b.Child });
        }
    }

    class CommitPeriodEntityTypeConfiguration : IEntityTypeConfiguration<CommitPeriod>
    {
        public void Configure(EntityTypeBuilder<CommitPeriod> configuration)
        {
            configuration
                .HasIndex(b => b.PeriodId);

            configuration
                .HasIndex(b => b.CommitSha);

            configuration
                .HasKey(b => new { b.CommitSha, b.PeriodId });
        }
    }

    class CommitEntityTypeConfiguration : IEntityTypeConfiguration<Commit>
    {
        public void Configure(EntityTypeBuilder<Commit> configuration)
        {
            configuration
                .HasIndex(b => b.AuthorEmail);

            configuration
                .HasIndex(b => b.CommitterEmail);

            configuration
                .HasIndex(b => b.Sha);
        }
    }

    class CommitBlobBlameEntityTypeConfiguration : IEntityTypeConfiguration<CommitBlobBlame>
    {
        public void Configure(EntityTypeBuilder<CommitBlobBlame> configuration)
        {

            configuration.HasKey(b => new { b.DeveloperIdentity, b.CommitSha, b.CanonicalPath });

            configuration.HasIndex(b => b.DeveloperIdentity);

            configuration.HasIndex(b => b.CommitSha);

            configuration.HasIndex(b => b.CanonicalPath);
        }
    }
    class CommittedBlobEntityTypeConfiguration : IEntityTypeConfiguration<CommittedBlob>
    {
        public void Configure(EntityTypeBuilder<CommittedBlob> configuration)
        {

            configuration.HasKey(b => new { b.CommitSha, b.CanonicalPath });

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
    class PullRequestReviewerEntityTypeConfiguration : IEntityTypeConfiguration<PullRequestReviewer>
    {
        public void Configure(EntityTypeBuilder<PullRequestReviewer> configuration)
        {
            configuration.HasIndex(b => b.PullRequestNumber);

            configuration.HasIndex(b => b.CommitId);
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
