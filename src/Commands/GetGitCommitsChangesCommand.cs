using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.Git;

namespace RelationalGit.Commands
{
    public class GetGitCommitsChangesCommand
    {
        private readonly ILogger _logger;

        public GetGitCommitsChangesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string repoPath, string branchName)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var gitRepository = new GitRepository(repoPath, _logger);
                var orderedCommits = gitRepository.ExtractCommitsFromBranch(branchName);
                gitRepository.LoadChangesOfCommits(orderedCommits);

                _logger.LogInformation("{dateTime}: saving committed changes", DateTime.Now);

                dbContext.BulkInsert(orderedCommits.SelectMany(q=>q.CommittedChanges).ToArray(), new BulkConfig { BatchSize = 50000, BulkCopyTimeout = 0 });

                _logger.LogInformation("{dateTime}: committed changes have been saved successfully", DateTime.Now);
            }
        }
    }
}
