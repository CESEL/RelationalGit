using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.GitHub;

namespace RelationalGit.Commands
{
    public class GetPullRequestReviewersCommand
    {
        private readonly ILogger _logger;

        public GetPullRequestReviewersCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE PullRequestReviewers");

                var pullRequests = await dbContext.PullRequests.AsNoTracking()
                    .OrderBy(q => q.Number)
                    .ToArrayAsync().ConfigureAwait(false);

                _logger.LogInformation("{datetime}: trying to fetch all the assigned reviewers for all {count} pull requests.", DateTime.Now, pullRequests.Length);

                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                var pullRequestReviews = await githubExtractor.FetchReviewersOfPullRequests(owner, repo, pullRequests).ConfigureAwait(false);

                _logger.LogInformation("{datetime}: trying to save {count} reviewers into database.", DateTime.Now, pullRequestReviews.Length);

                dbContext.BulkInsert(pullRequestReviews, new BulkConfig { BatchSize = 50000, BulkCopyTimeout = 0 });

                _logger.LogInformation("{datetime}: reviewers has been save successfully.", DateTime.Now, pullRequestReviews.Length);
            }
        }
    }
}
