
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class PeriodizeGitCommits
    {
        public async Task Execute(string repoPath, string branchName, string periodType, int periodLength)
        {
            var commitPeriods = new List<CommitPeriod>();

            using (var dbContext = new GitRepositoryDbContext())
            {
                var commits = dbContext.Commits
                .OrderBy(m => m.AuthorDateTime)
                .ToArray();

                var beginDatetime = commits.Min(m => m.AuthorDateTime);
                var qaurterMonth = GetQaurterMonth(beginDatetime);
                beginDatetime = new DateTime(beginDatetime.Year, qaurterMonth, 1);
                var endDatetime = commits.Max(m => m.AuthorDateTime);

                var periods = GetPeriods(periodType, periodLength, beginDatetime, endDatetime);
                var currentPeriodIndex = 0;

                periods[currentPeriodIndex].FirstCommitSha = commits[0].Sha;
                for (int i = 0; i < commits.Length; i++)
                {
                    if (periods[currentPeriodIndex].ToDateTime < commits[i].AuthorDateTime)
                    {
                        periods[currentPeriodIndex].LastCommitSha = commits
                        .Last(q => !q.Ignore && q.AuthorDateTime <= commits[i - 1].AuthorDateTime)
                        .Sha;

                        currentPeriodIndex++;

                        periods[currentPeriodIndex].FirstCommitSha = commits
                        .First(q => !q.Ignore && q.AuthorDateTime >= commits[i].AuthorDateTime)
                        .Sha;
                    }

                    commits[i].PeriodId = periods[currentPeriodIndex].Id;
                }

                periods[currentPeriodIndex].LastCommitSha = commits[commits.Length - 1].Sha;

                dbContext.Periods.AddRange(periods);
                await dbContext.SaveChangesAsync();
            }
        }

        private static int GetQaurterMonth(DateTime beginDatetime)
        {
            return (int)Math.Ceiling(beginDatetime.Month / 3.0) * 3 - 2;
        }

        private Period[] GetPeriods(string periodType, int periodLength, DateTime beginDatetime, DateTime endDatetime)
        {
            var periodId=0;
            var periods = new List<Period>();
            var pinDatetime = beginDatetime;

            while (pinDatetime < endDatetime)
            {
                periods.Add(new Period()
                {
                    Id=++periodId,                    
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
