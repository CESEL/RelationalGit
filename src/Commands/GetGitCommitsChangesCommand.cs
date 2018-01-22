using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitCommitsChangesCommand
    {
        public async Task Execute(string repoPath,string branchName)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var gitRepository = new GitRepository(repoPath);
                var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName);
                gitRepository.LoadChangesOfCommits(orderedCommits);

                foreach (var commit in orderedCommits)
                {
                    dbContext.CommittedChanges.AddRange(commit.CommittedChanges);
                }
                
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
