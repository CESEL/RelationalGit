using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitBlobsAndTheirBlamesForPeriodsCommand
    {
        public async Task Execute(string repoPath,string branchName,string[] validExtensions)
        {
            using (var dbContext=new GitRepositoryDbContext())
            {
                var periods = dbContext.Periods.OrderBy(q => q.ToDateTime);

                foreach (var period in periods)
                {
                    var lastCommitOfPeriod = period.LastCommitSha;
                    var cmd = new GetGitBlobsAndTheirBlamesOfCommitCommand();
                    await cmd.Execute(repoPath, branchName, lastCommitOfPeriod, validExtensions);
                }
            }
        }
    }
}
