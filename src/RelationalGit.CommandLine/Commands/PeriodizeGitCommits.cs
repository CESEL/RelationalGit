using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelationalGit.Data;

namespace RelationalGit.Commands
{
    public class PeriodizeGitCommits
    {
        private readonly ILogger _logger;

        public PeriodizeGitCommits(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string periodType, int periodLength)
        {
            var commitPeriods = new List<CommitPeriod>();

            using (var dbContext = new GitRepositoryDbContext())
            {
                var commits = dbContext.Commits.OrderBy(m => m.AuthorDateTime).ToArray();

                _logger.LogInformation("{datetime}: trying to periodize all the {count} commits. Period Type: {type}, Period Length: {length}",
                    DateTime.Now, commits.Count(), periodType, periodLength);

                var beginDatetime = commits.Min(m => m.AuthorDateTime);
                beginDatetime = new DateTime(beginDatetime.Year, beginDatetime.Month, 1);
                var endDatetime = commits.Max(m => m.AuthorDateTime);

                var periods = GetPeriods(periodType, periodLength, beginDatetime, endDatetime);
                var currentPeriodIndex = 0;

                periods[currentPeriodIndex].FirstCommitSha = commits[0].Sha;
                for (int i = 0; i < commits.Length; i++)
                {
                    if (periods[currentPeriodIndex].ToDateTime < commits[i].AuthorDateTime)
                    {
                        var candidateCommits = commits.Where(q => !q.Ignore && q.AuthorDateTime <= commits[i - 1].AuthorDateTime);

                        if (candidateCommits.Count() > 0) // the problem happens (rarely) in the first period.
                        {
                            periods[currentPeriodIndex].LastCommitSha = candidateCommits.Last().Sha;
                        }
                        else
                        {
                            periods[currentPeriodIndex].LastCommitSha = periods[currentPeriodIndex].FirstCommitSha;
                        }

                        currentPeriodIndex++;

                        periods[currentPeriodIndex].FirstCommitSha = commits
                        .First(q => !q.Ignore && q.AuthorDateTime >= commits[i].AuthorDateTime)
                        .Sha;
                    }

                    commits[i].PeriodId = periods[currentPeriodIndex].Id;
                }

                periods[currentPeriodIndex].LastCommitSha = commits[commits.Length - 1].Sha;

                _logger.LogInformation("{datetime}: trying to save {count} periods.", DateTime.Now, periods.Count());
                dbContext.Periods.AddRange(periods);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation("{datetime}: periods has been saved successfully.", DateTime.Now);
            }
        }

        private Period[] GetPeriods(string periodType, int periodLength, DateTime beginDatetime, DateTime endDatetime)
        {
            var periodId = 0;
            var periods = new List<Period>();
            var pinDatetime = beginDatetime;

            while (pinDatetime < endDatetime)
            {
                periods.Add(new Period()
                {
                    Id = ++periodId,
                    FromDateTime = pinDatetime,
                    ToDateTime = periodType == PeriodType.Month
                    ? pinDatetime.AddMonths(periodLength)
                    : pinDatetime.AddDays(periodLength),
                });

                pinDatetime = periodType == PeriodType.Month
                ? pinDatetime.AddMonths(periodLength)
                : pinDatetime.AddDays(periodLength);
            }

            return periods.ToArray();
        }
    }
}
