﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationalGit.Commands
{
    public class GetUsersCommand
    {
        private ILogger _logger;

        public GetUsersCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agentName)
        {
            using (var dbContext = new GitRepositoryDbContext())
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE Users");
                var unknownUsers = dbContext.Users
                    .FromSql(@"Select UserLogin,null AS Name, NULL as Email from (
                            select distinct(UserLogin) AS UserLogin from PullRequests  WHERE UserLogin IS NOT null
                            union   
                            select distinct(UserLogin) AS UserLogin from PullRequestReviewers  WHERE UserLogin IS NOT null
                            union   
                            select distinct(UserLogin) from PullRequestReviewerComments WHERE UserLogin IS NOT null) AS Temp")
                            .ToArray();

                var githubExtractor = new GithubDataFetcher(token, agentName,_logger);
                await githubExtractor.GetUsers(unknownUsers);

                dbContext.AddRange(unknownUsers);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
