using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetIssuesEventsCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                var loadedIssues = dbContext.Issue.ToArray();
                var githubExtractor = new GithubDataFetcher(token, agenName);
                var issueEvents = await githubExtractor.GetIssueEvents(owner, repo, loadedIssues);
                dbContext.AddRange(issueEvents);
                dbContext.SaveChanges();
            }
        }
    }
}
