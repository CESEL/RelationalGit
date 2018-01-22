using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetUsersCommand
    {
        public async Task Execute(string token, string agentName)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE Users");
                var unknownUsers = dbContext.Users
                    .FromSql(@"Select Username,null AS Name, NULL as Email from (
                            select distinct(Username) AS Username from PullRequests  WHERE Username IS NOT null
                            union   
                            select distinct(Username) AS Username from PullRequestReviewers  WHERE Username IS NOT null
                            union   
                            select distinct(Username) from PullRequestReviewerComments WHERE Username IS NOT null) AS Temp")
                            .ToArray();

                var githubExtractor = new GithubDataFetcher(token, agentName);
                await githubExtractor.GetUsers(unknownUsers);

                dbContext.AddRange(unknownUsers);
                dbContext.SaveChanges();
            }
        }
    }
}
