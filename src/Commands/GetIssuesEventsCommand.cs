using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetIssuesEventsCommand
    {
        private ILogger _logger;

        public GetIssuesEventsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var loadedIssues = dbContext.Issue.ToArray();
                var githubExtractor = new GithubDataFetcher(token, agenName,_logger);
                var issueEvents = await githubExtractor.GetIssueEvents(owner, repo, loadedIssues);
                dbContext.AddRange(issueEvents);
                dbContext.SaveChanges();
            }
        }
    }
}
