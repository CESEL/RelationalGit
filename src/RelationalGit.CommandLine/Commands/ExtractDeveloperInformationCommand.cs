
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RelationalGit.Data;

namespace RelationalGit.Commands
{
    public class ExtractDeveloperInformationCommand
    {
        private readonly ILogger _logger;

        public ExtractDeveloperInformationCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(double coreDeveloperThreshold, string coreDeveloperCalculationType)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                _logger.LogInformation("{datetime}: extracting developer statistics for each period.", DateTime.Now);

                var reviewersPeriodsDic = dbContext.GetPeriodReviewerCounts();
                var reviewersParticipationDic = dbContext.GetReviewersParticipationDates();
                await ExtractDevelopersInformation(reviewersPeriodsDic, reviewersParticipationDic, dbContext).ConfigureAwait(false);
                await ExctractContributionsPerPeriod(coreDeveloperThreshold, coreDeveloperCalculationType, reviewersPeriodsDic, dbContext).ConfigureAwait(false);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);

                _logger.LogInformation("{datetime}: developer information has been extracted and saved.", DateTime.Now);
            }
        }

        private async Task ExctractContributionsPerPeriod(double coreDeveloperThreshold, string coreDeveloperCalculationType, Dictionary<string, Dictionary<long, int>> reviewersInPeriods, GitRepositoryDbContext dbContext)
        {
            var commits = await dbContext.Commits
            .Where(q => !q.Ignore)
            .GroupBy(q => new { q.NormalizedAuthorName, q.PeriodId })
            .Select(q => new
            {
                DeveloperName = q.Key.NormalizedAuthorName,
                PeriodId = q.Key.PeriodId,
                TotalCommits = q.Count(),
            })
            .ToArrayAsync().ConfigureAwait(false);

            foreach (var period in dbContext.Periods.ToArray())
            {
                var commitSha = period.LastCommitSha;

                var blames = await dbContext.CommitBlobBlames
                .Where(q => q.CommitSha == commitSha && !q.Ignore)
                .GroupBy(q => q.NormalizedDeveloperIdentity)
                .Select(q => new
                {
                    DeveloperName = q.Key,
                    DeveloperKnowledge = q.Sum(l => l.AuditedLines)
                })
                .OrderByDescending(q => q.DeveloperKnowledge)
                .ToArrayAsync().ConfigureAwait(false);

                var totalKnowledge = (double)blames.Sum(q => q.DeveloperKnowledge);

                var knowledgePortions = 0.0;

                foreach (var dev in blames)
                {
                    var ownershipPercentage = dev.DeveloperKnowledge / totalKnowledge;
                    knowledgePortions += ownershipPercentage;

                    var totalReviews = reviewersInPeriods.ContainsKey(dev.DeveloperName)
                    ? reviewersInPeriods[dev.DeveloperName].ContainsKey(period.Id)
                    ? reviewersInPeriods[dev.DeveloperName][period.Id]
                    : 0
                    : 0;

                    dbContext.Add(new DeveloperContribution()
                    {
                        IsCore = IsCore(knowledgePortions, ownershipPercentage, dev.DeveloperKnowledge, coreDeveloperThreshold, coreDeveloperCalculationType),
                        NormalizedName = dev.DeveloperName,
                        TotalLines = dev.DeveloperKnowledge,
                        PeriodId = period.Id,
                        LinesPercentage = ownershipPercentage,
                        TotalCommits = commits.SingleOrDefault(q => q.PeriodId == period.Id && q.DeveloperName == dev.DeveloperName)?.TotalCommits ?? 0,
                        TotalReviews = totalReviews
                    });
                }

                var reviewersOfPeriod = reviewersInPeriods.Keys.Where(q => reviewersInPeriods[q].ContainsKey(period.Id));
                foreach (var soleReviewer in reviewersOfPeriod.Where(q => !blames.Any(d => d.DeveloperName == q)))
                {
                    var totalReviews = reviewersInPeriods[soleReviewer][period.Id];

                    dbContext.Add(new DeveloperContribution()
                    {
                        IsCore = false,
                        NormalizedName = soleReviewer,
                        TotalLines = 0,
                        PeriodId = period.Id,
                        LinesPercentage = 0,
                        TotalCommits = 0,
                        TotalReviews = totalReviews
                    });
                }
            }
        }

        private bool IsCore(double knowledgePortions, double authorshipPercentage, int authoredLines, double threshold, string coreDeveloperCalculationType)
        {
            if (coreDeveloperCalculationType == CoreDeveloperCalculationType.AuthorshipQuantile)
            {
                 return knowledgePortions <= threshold;
            }
            else if (coreDeveloperCalculationType == CoreDeveloperCalculationType.AuthoredLinesPercentage)
            {
                return authorshipPercentage >= threshold;
            }
            else if (coreDeveloperCalculationType == CoreDeveloperCalculationType.AuthoredLines)
            {
                return authoredLines >= threshold;
            }

            throw new ArgumentException($"Undefined {nameof(coreDeveloperCalculationType)}");
        }

        private async Task ExtractDevelopersInformation(Dictionary<string, Dictionary<long, int>> reviewersInPeriods, Dictionary<string, (DateTime? FirstReviewDateTime, DateTime? LastReviewDateTime)> reviewersParticipationDic, GitRepositoryDbContext dbContext)
        {

            var commitsDateTime = new Dictionary<string,(DateTime? FirstCommitDateTime,DateTime? LastCommitDateTime)>();
            var commits = await dbContext.Commits.OrderBy(q => q.AuthorDateTime).ToArrayAsync().ConfigureAwait(false);

            foreach (var commit in commits)
            {
                if (!commitsDateTime.ContainsKey(commit.NormalizedAuthorName))
                {
                    commitsDateTime[commit.NormalizedAuthorName] = (null, null);
                }

                if (!commitsDateTime[commit.NormalizedAuthorName].FirstCommitDateTime.HasValue)
                {
                    commitsDateTime[commit.NormalizedAuthorName] = (commit.AuthorDateTime,null);
                }

                commitsDateTime[commit.NormalizedAuthorName] = (commitsDateTime[commit.NormalizedAuthorName].FirstCommitDateTime, commit.AuthorDateTime);
            }

            var commitsGroupByNormalizedName = commits
            .Where(q => !q.Ignore)
            .Select(q => new { q.NormalizedAuthorName, q.PeriodId })
            .GroupBy(q => q.NormalizedAuthorName);

            foreach (var group in commitsGroupByNormalizedName)
            {
                var totalReviews = 0;
                long? firstReviewPeriodId = null;
                long? lastReviewPeriodId = null;
                string allReviewPeriods = null;

                DateTime? firstReviewDateTime = reviewersParticipationDic.ContainsKey(group.Key)
                    ? reviewersParticipationDic[group.Key].FirstReviewDateTime : null;
                DateTime? lastReviewDateTime = reviewersParticipationDic.ContainsKey(group.Key)
                    ? reviewersParticipationDic[group.Key].LastReviewDateTime : null;

                var allReviews = reviewersInPeriods.GetValueOrDefault(group.Key);

                if (allReviews != null)
                {
                    totalReviews = allReviews.Sum(q => q.Value);
                    firstReviewPeriodId = allReviews.Min(q => q.Key);
                    lastReviewPeriodId = allReviews.Max(q => q.Key);
                    allReviewPeriods = allReviews.Keys.Select(q => q.ToString()).Aggregate((a, b) => (a + "," + b));
                }

                dbContext.Add(new Developer()
                {
                    NormalizedName = group.Key,
                    TotalCommits = group.Count(),
                    AllCommitPeriods = group
                    .Select(q => q.PeriodId.ToString())
                    .Distinct()
                    .OrderBy(q => q)
                    .Aggregate((a, b) => a + "," + b),
                    LastCommitPeriodId = group.Max(q => q.PeriodId),
                    FirstCommitPeriodId = group.Min(q => q.PeriodId),
                    TotalReviews = totalReviews,
                    FirstReviewPeriodId = firstReviewPeriodId,
                    LastReviewPeriodId = lastReviewPeriodId,
                    AllReviewPeriods = allReviewPeriods,
                    FirstCommitDateTime = commitsDateTime[group.Key].FirstCommitDateTime,
                    LastCommitDateTime = commitsDateTime[group.Key].LastCommitDateTime,
                    FirstReviewDateTime = firstReviewDateTime,
                    LastReviewDateTime = lastReviewDateTime
                });
            }

            foreach (var soleReviewer in reviewersInPeriods.Keys.Where(q => !commitsGroupByNormalizedName.Any(d => d.Key == q)))
            {
                var allReviews = reviewersInPeriods.GetValueOrDefault(soleReviewer);

                var totalReviews = allReviews.Sum(q => q.Value);
                var firstReviewPeriodId = allReviews.Min(q => q.Key);
                var lastReviewPeriodId = allReviews.Max(q => q.Key);
                var allReviewPeriods = allReviews.Keys.Select(q => q.ToString()).Aggregate((a, b) => a + "," + b);

                DateTime? firstReviewDateTime = reviewersParticipationDic.ContainsKey(soleReviewer)
                    ? reviewersParticipationDic[soleReviewer].FirstReviewDateTime : null;
                DateTime? lastReviewDateTime = reviewersParticipationDic.ContainsKey(soleReviewer)
                    ? reviewersParticipationDic[soleReviewer].LastReviewDateTime : null;

                dbContext.Add(new Developer()
                {
                    NormalizedName = soleReviewer,
                    TotalCommits = 0,
                    AllCommitPeriods = null,
                    LastCommitPeriodId = null,
                    FirstCommitPeriodId = null,
                    TotalReviews = totalReviews,
                    FirstReviewPeriodId = firstReviewPeriodId,
                    LastReviewPeriodId = lastReviewPeriodId,
                    AllReviewPeriods = allReviewPeriods,
                    FirstReviewDateTime = firstReviewDateTime,
                    LastReviewDateTime = lastReviewDateTime
                });
            }
        }
    }
}
