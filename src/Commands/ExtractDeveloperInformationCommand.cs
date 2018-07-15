
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diacritics.Extensions;
using F23.StringSimilarity;

namespace RelationalGit.Commands
{
    public class ExtractDeveloperInformationCommand
    {
        public async Task Execute(double topQuantileThreshold)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                await ExtractDevelopersInformation(dbContext);

                await ExctractContributionsPerPeriod(topQuantileThreshold, dbContext);

                await dbContext.SaveChangesAsync();
            }
        }

        private static async Task ExctractContributionsPerPeriod(double topQuantileThreshold, GitRepositoryDbContext dbContext)
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

                var blames = await dbContext
                .CommitBlobBlames
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
                    knowledgePortions += dev.DeveloperKnowledge / totalKnowledge;

                    dbContext.Add(new DeveloperContribution()
                    {
                        IsCore = knowledgePortions <= topQuantileThreshold,
                        NormalizedName = dev.DeveloperName,
                        TotalLines = dev.DeveloperKnowledge,
                        PeriodId = period.Id,
                        LinesPercentage = dev.DeveloperKnowledge / totalKnowledge,
                        TotalCommits = commits
                            .SingleOrDefault(q => q.PeriodId == period.Id && q.DeveloperName == dev.DeveloperName)
                            ?.TotalCommits ?? 0
                    });
                }
            }
        }

        private static async Task ExtractDevelopersInformation(GitRepositoryDbContext dbContext)
        {
            var commitsGroupByNormalizedName = await dbContext
            .Commits
            .Where(q => !q.Ignore)
            .Select(q => new { q.NormalizedAuthorName, q.PeriodId })
            .GroupBy(q => q.NormalizedAuthorName)
            .ToArrayAsync();

            foreach (var group in commitsGroupByNormalizedName)
            {
                dbContext.Add(new Developer()
                {
                    NormalizedName = group.Key,
                    TotalCommits = group.Count(),
                    AllPeriods = group
                    .Select(q => q.PeriodId.ToString())
                    .Distinct()
                    .OrderBy(q => q)
                    .Aggregate((a, b) => (a + "," + b)).ToString(),
                    LastPeriodId = group.Max(q => q.PeriodId),
                    FirstPeriodId = group.Min(q => q.PeriodId)
                });
            }
        }
    }
}
