
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diacritics.Extensions;
using F23.StringSimilarity;
using Microsoft.Extensions.Logging;

namespace RelationalGit.Commands
{
    public class ExtractDeveloperInformationCommand
    {
        private ILogger _logger;

        public ExtractDeveloperInformationCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(double coreDeveloperThreshold,string coreDeveloperCalculationType)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                _logger.LogInformation("{datetime}: extracting developer statistics for each period.", DateTime.Now);

                var reviewersDic =dbContext.GetPeriodReviewerCounts();
                await ExtractDevelopersInformation(reviewersDic,dbContext);
                await ExctractContributionsPerPeriod(coreDeveloperThreshold,coreDeveloperCalculationType,reviewersDic, dbContext);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("{datetime}: developer information has been extracted and saved.", DateTime.Now);

            }
        }

        private  async Task ExctractContributionsPerPeriod(double coreDeveloperThreshold,
        string coreDeveloperCalculationType,
        Dictionary<string,Dictionary<long,int>> reviewersInPeriods, 
        GitRepositoryDbContext dbContext)
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
            .ToArrayAsync();

            var periods = dbContext.Periods.ToArray();

            foreach (var period in periods)
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
                .ToArrayAsync();

                var totalKnowledge = (double)blames.Sum(q => q.DeveloperKnowledge);

                var knowledgePortions = 0.0;

                foreach (var dev in blames)
                {
                    var ownershipPercentage = dev.DeveloperKnowledge / totalKnowledge;
                    knowledgePortions += ownershipPercentage;

                    var totalReviews = reviewersInPeriods.ContainsKey(dev.DeveloperName)
                    ?reviewersInPeriods[dev.DeveloperName].ContainsKey(period.Id)
                    ?reviewersInPeriods[dev.DeveloperName][period.Id]
                    :0
                    :0;

                    dbContext.Add(new DeveloperContribution()
                    {
                        IsCore =  IsCore(knowledgePortions,ownershipPercentage,dev.DeveloperKnowledge,coreDeveloperThreshold,coreDeveloperCalculationType),
                        NormalizedName = dev.DeveloperName,
                        TotalLines = dev.DeveloperKnowledge,
                        PeriodId = period.Id,
                        LinesPercentage = ownershipPercentage,
                        TotalCommits = commits
                            .SingleOrDefault(q => q.PeriodId == period.Id && q.DeveloperName == dev.DeveloperName)
                            ?.TotalCommits ?? 0,
                        TotalReviews=totalReviews
                    });
                }
            }
        }

        private bool IsCore(double knowledgePortions, double authorshipPercentage,int authoredLines, double threshold, string coreDeveloperCalculationType)
        {
            if(coreDeveloperCalculationType==CoreDeveloperCalculationType.AuthorshipQuantile){
                 return knowledgePortions <= threshold;
            }
            else if (coreDeveloperCalculationType==CoreDeveloperCalculationType.AuthoredLinesPercentage)
            {   
                return authorshipPercentage>=threshold;
            }
            else if (coreDeveloperCalculationType==CoreDeveloperCalculationType.AuthoredLines)
            {   
                return authoredLines>=threshold;
            }

            throw new ArgumentException($"Undefined {nameof(coreDeveloperCalculationType)}");
        }

        private  async Task ExtractDevelopersInformation(Dictionary<string,Dictionary<long,int>> reviewersInPeriods, 
        GitRepositoryDbContext dbContext)
        {
            var commitsGroupByNormalizedName = await dbContext
            .Commits
            .Where(q => !q.Ignore)
            .Select(q => new { q.NormalizedAuthorName, q.PeriodId })
            .GroupBy(q => q.NormalizedAuthorName)
            .ToArrayAsync();

            foreach (var group in commitsGroupByNormalizedName)
            {

                var totalReviews = 0;
                var firstReviewPeriodId = 0L;
                var lastReviewPeriodId = 0L;
                var allReviewPeriods ="";
                var allReviews = reviewersInPeriods.GetValueOrDefault(group.Key);

                if(allReviews!=null)
                {
                    totalReviews = allReviews.Sum(q=>q.Value);
                    firstReviewPeriodId = allReviews.Min(q=>q.Key);
                    lastReviewPeriodId = allReviews.Max(q=>q.Key);
                    allReviewPeriods = allReviews.Keys.Select(q=>q.ToString())
                    .Aggregate((a, b) => (a + "," + b)).ToString();
                }

                dbContext.Add(new Developer()
                {
                    NormalizedName = group.Key,
                    TotalCommits = group.Count(),
                    AllCommitPeriods = group
                    .Select(q => q.PeriodId.ToString())
                    .Distinct()
                    .OrderBy(q => q)
                    .Aggregate((a, b) => (a + "," + b)).ToString(),
                    LastCommitPeriodId = group.Max(q => q.PeriodId),
                    FirstCommitPeriodId = group.Min(q => q.PeriodId),
                    TotalReviews = totalReviews,
                    FirstReviewPeriodId=firstReviewPeriodId,
                    LastReviewPeriodId=lastReviewPeriodId,
                    AllReviewPeriods = allReviewPeriods
                });
            }
        }
    }
}
