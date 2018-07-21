using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestReviewerCommentsCommand
    {
        private ILogger _logger;

        public GetPullRequestReviewerCommentsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(false))
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE PullRequestReviewerComments");
                var githubExtractor = new GithubDataFetcher(token, agenName,_logger);
                var pullRequestReviewerComments = await githubExtractor.FetchReviewerCommentsFromRepository(owner, repo);

                _logger.LogInformation("{datetime}: saving {count} review comments  into database.", DateTime.Now, pullRequestReviewerComments.Length);

                dbContext.AddRange(pullRequestReviewerComments);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("{datetime}: {count} review comments have been saved into database.", DateTime.Now, pullRequestReviewerComments.Length);

            }
        }
    }
}
