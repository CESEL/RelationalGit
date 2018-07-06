using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetIssuesCommand
    {
        public async Task Execute(string token, string agenName, string owner, string repo, string[] labels,string state="All")
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE Issue");
                var githubExtractor = new GithubDataFetcher(token, agenName);
                var issues = await githubExtractor.GetIssues(owner, repo, labels,state);
                dbContext.AddRange(issues);
                dbContext.SaveChanges();
            }
        }
    }
}
