using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.Git;

namespace RelationalGit.Commands
{
    public class GetBlamesOfCommitedChangesCommand
    {
        private readonly ILogger _logger;

        public GetBlamesOfCommitedChangesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string repoPath, string branchName, string[] validExtensions, string[] excludedBlamePaths)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var gitRepository = new GitRepository(repoPath, _logger);
                var committedChanges = dbContext.CommittedChanges.ToArray();

                _logger.LogInformation("{datetime}: just started to extract blames.", DateTime.Now);
                var blames = gitRepository.GetBlameOfChanges(branchName, validExtensions, excludedBlamePaths, committedChanges);
                _logger.LogInformation("{datetime}: trying to save {count} blames into database.", DateTime.Now, blames.Count());
                await dbContext.BulkInsertAsync(blames).ConfigureAwait(false);
                _logger.LogInformation("{datetime}: blames has been saved successfully.", DateTime.Now);
            }
        }
    }
}
