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
    public class GetPullRequestIssueCommentsCommand
    {
        private readonly ILogger _logger;

        public GetPullRequestIssueCommentsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE IssueComments");
                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                var pullRequests = await dbContext.PullRequests.ToArrayAsync().ConfigureAwait(false);
                var issueComments = await githubExtractor.FetchPullRequestIssueCommentsFromRepository(owner, repo, pullRequests).ConfigureAwait(false);

                var issueCommentCount = issueComments.Count();

                _logger.LogInformation("{datetime}: saving {count} issue comments  into database.", DateTime.Now, issueCommentCount);

                dbContext.BulkInsert(issueComments.ToArray(), new BulkConfig { BatchSize = 50000, BulkCopyTimeout = 0 });

                _logger.LogInformation("{datetime}: {count} issue comments have been saved into database.", DateTime.Now, issueCommentCount);
            }
        }
    }
}
