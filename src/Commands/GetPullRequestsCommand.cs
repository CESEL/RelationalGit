using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestsCommand
    {
        private readonly ILogger _logger;

        public GetPullRequestsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(true))
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE PullRequests");
                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                var pullRequests = await githubExtractor.FetchAllPullRequests(owner, repo).ConfigureAwait(false);
                _logger.LogInformation("{datetime}: trying to save {count} pull requests.", DateTime.Now, pullRequests.Length);
                dbContext.AddRange(pullRequests);
                dbContext.SaveChanges();
                _logger.LogInformation("{datetime}: pull requests has been saved successfully.", DateTime.Now);
            }
        }
    }
}
