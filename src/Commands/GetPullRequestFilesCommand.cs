﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestFilesCommand
    {
        private readonly ILogger _logger;

        public GetPullRequestFilesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                var loadedPullRequests = await dbContext.PullRequests.ToArrayAsync().ConfigureAwait(false);

                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                var files = await githubExtractor.FetchFilesOfPullRequests(owner, repo, loadedPullRequests).ConfigureAwait(false);

                _logger.LogInformation("{datetime}: saving {count} pull request files into database.", DateTime.Now, files.Length);

                dbContext.AddRange(files);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);

                _logger.LogInformation("{datetime}: pull request files have been saved successfully.", DateTime.Now, loadedPullRequests.Length);
            }
        }
    }
}
