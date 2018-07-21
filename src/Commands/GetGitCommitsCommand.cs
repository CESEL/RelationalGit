using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetGitCommitsCommand
    {
        private ILogger _logger;

        public GetGitCommitsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string repoPath, string branchName)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var gitRepository = new GitRepository(repoPath,_logger);
                var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName);
                dbContext.Commits.AddRange(orderedCommits);
                var relationships = orderedCommits.SelectMany(q => q.CommitRelationship);
                dbContext.CommitRelationships.AddRange(relationships);
                _logger.LogInformation("{datetime}: trying to save {count} commits into database.",DateTime.Now,orderedCommits.Count());
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("{datetime}: commits has been saved successfully.", DateTime.Now);
            }
        }
    }
}
