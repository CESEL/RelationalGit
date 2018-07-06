using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetPullRequestsCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(true))
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE PullRequests");
                var githubExtractor = new GithubDataFetcher(token, agenName);
                var pullRequests = await githubExtractor.FetchAllPullRequests(owner, repo, branch);
                dbContext.AddRange(pullRequests);
                dbContext.SaveChanges();
            }
        }
    }
}
