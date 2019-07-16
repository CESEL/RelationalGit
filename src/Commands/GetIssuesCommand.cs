using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.GitHub;

namespace RelationalGit.Commands
{
    public class GetIssuesCommand
    {
        private readonly ILogger _logger;

        public GetIssuesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string[] labels, string state = "All")
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE Issue");
                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                var issues = await githubExtractor.GetIssues(owner, repo, labels, state).ConfigureAwait(false);
                dbContext.AddRange(issues);
                dbContext.SaveChanges();
            }
        }
    }
}
