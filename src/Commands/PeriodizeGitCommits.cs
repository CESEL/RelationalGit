
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
        public async Task Execute(string repoPath, string branchName, int gapBetweenPeriods)
        {
            var currentPeriodIndex = 0;
            var commitPeriods = new List<CommitPeriod>();

            using (var dbContext = new GitRepositoryDbContext())
            {
                var commits = dbContext.Commits
                .Select(m => new { m.AuthorDateTime, m.Sha })
                .OrderBy(m => m.AuthorDateTime)
                .ToArray();

                var beginDatetime = commits.Min(m=>m.AuthorDateTime);
                var endDatetime = commits.Max(m=>m.AuthorDateTime);

                var periods = GetPeriods(gapBetweenPeriods, beginDatetime, endDatetime);

                for (int i = 0; i < commits.Length; i++)
                {
                    if (periods[currentPeriodIndex].ToDateTime < commits[i].AuthorDateTime)
                    {
                        periods[currentPeriodIndex].LastCommitSha = commits[i - 1].Sha;
                        currentPeriodIndex++;
                    }

                    var commitPeriod = new CommitPeriod()
                    {
                        CommitSha = commits[i].Sha,
                        PeriodId = periods[currentPeriodIndex].Id
                    };

                    commitPeriods.Add(commitPeriod);
                }

                periods[currentPeriodIndex].LastCommitSha = commits[commits.Length - 1].Sha;


                dbContext.CommitPeriods.AddRange(commitPeriods);
                await dbContext.SaveChangesAsync();
            }
        }

        private Period[] GetPeriods(int gapBetweenPeriods, DateTime beginDatetime, DateTime endDatetime)
        {
            var periods = new List<Period>();
            var pinDatetime = beginDatetime;

            while (pinDatetime < endDatetime)
            {
                periods.Add(new Period()
                {
                    FromDateTime = pinDatetime,
                    ToDateTime = pinDatetime.AddDays(gapBetweenPeriods),
                });

                pinDatetime = pinDatetime.AddDays(gapBetweenPeriods);
            }

            return periods.ToArray();
        }
    }
}
