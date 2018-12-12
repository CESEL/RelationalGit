﻿using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
                var relationships = orderedCommits.SelectMany(q => q.CommitRelationship).ToArray();
                _logger.LogInformation("{datetime}: trying to save {count} commits into database.", DateTime.Now, orderedCommits.Count());
                await dbContext.BulkInsertAsync(orderedCommits);
                await dbContext.BulkInsertAsync(relationships);
                _logger.LogInformation("{datetime}: commits has been saved successfully.", DateTime.Now);
            }
        }
    }
}
