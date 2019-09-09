using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.GitHub;

namespace RelationalGit.Commands
{
    public class GetIssuesEventsCommand
    {
        private readonly ILogger _logger;

        public GetIssuesEventsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var loadedIssues = dbContext.Issue.ToArray();
                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                var issueEvents = await githubExtractor.GetIssueEvents(owner, repo, loadedIssues).ConfigureAwait(false);
                dbContext.AddRange(issueEvents);
                dbContext.SaveChanges();
            }
        }
    }
}
