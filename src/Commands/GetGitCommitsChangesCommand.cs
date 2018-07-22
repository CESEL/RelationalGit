using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitCommitsChangesCommand
    {
        private ILogger _logger;

        public GetGitCommitsChangesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string repoPath,string branchName)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var gitRepository = new GitRepository(repoPath,_logger);
                var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName);
                gitRepository.LoadChangesOfCommits(orderedCommits);

                foreach (var commit in orderedCommits)
                {
                    dbContext.CommittedChanges.AddRange(commit.CommittedChanges);
                }

                _logger.LogInformation("{dateTime}: saving committed changes", DateTime.Now);

                await dbContext.SaveChangesAsync();

                _logger.LogInformation("{dateTime}: committed changes have been saved successfully", DateTime.Now);

            }
        }
    }
}
