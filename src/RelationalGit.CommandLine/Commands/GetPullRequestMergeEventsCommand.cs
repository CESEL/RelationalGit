using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.GitHub;

namespace RelationalGit.Commands
{
    public class GetPullRequestMergeEventsCommand
    {
        private readonly ILogger _logger;

        public GetPullRequestMergeEventsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(true))
            {
                var loadedPullRequests = await dbContext.PullRequests.FromSql(@"select * from PullRequests WHERE Merged=1 and MergeCommitSha not in (select Sha from Commits)")
                    .ToArrayAsync().ConfigureAwait(false);

                _logger.LogInformation("{datetime}: there are {count} pull requests with no corresponding merged commit", DateTime.Now, loadedPullRequests.Length);

                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                await githubExtractor.MergeEvents(owner, repo, loadedPullRequests).ConfigureAwait(false);

                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation("{datetime}: corresponding merged commits has been resolved and saved.", DateTime.Now);
            }
        }
    }
}
